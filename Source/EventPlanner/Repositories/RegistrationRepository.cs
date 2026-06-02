using System.Globalization;
using EventPlanner.Data;
using EventPlanner.Domain;

namespace EventPlanner.Repositories;

public class RegistrationRepository : IRepository<Registration>
{
    private const string SelectColumns =
        "RegistrationId, EventId, PersonId, TicketType, Price, PaidAmount, Status, Notes, CheckedInAt, CreatedAt";

    private static Registration ReadRow(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        var registration = new Registration
        {
            RegistrationId = reader.GetInt32(0),
            EventId = reader.GetInt32(1),
            PersonId = reader.GetInt32(2),
            TicketType = reader.GetString(3),
            Notes = reader.IsDBNull(7) ? "" : reader.GetString(7),
            CheckedInAt = reader.IsDBNull(8) ? null : DateTime.ParseExact(reader.GetString(8), "s", CultureInfo.InvariantCulture),
            CreatedAt = DateTime.ParseExact(reader.GetString(9), "s", CultureInfo.InvariantCulture)
        };
        var storedStatus = Enum.Parse<PaymentStatus>(reader.GetString(6));
        registration.UpdatePayment(
            Convert.ToDecimal(reader.GetDouble(4)),
            Convert.ToDecimal(reader.GetDouble(5)),
            storedStatus == PaymentStatus.Cancelled);
        return registration;
    }

    public List<Registration> GetAll()
    {
        var result = new List<Registration>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Registrations ORDER BY CreatedAt DESC";
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public Registration? GetById(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Registrations WHERE RegistrationId = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadRow(reader) : null;
    }

    public List<Registration> GetByEventId(int eventId)
    {
        var result = new List<Registration>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Registrations WHERE EventId = $eventId";
        command.Parameters.AddWithValue("$eventId", eventId);
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public int Add(Registration item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Registrations (EventId, PersonId, TicketType, Price, PaidAmount, Status, Notes, CheckedInAt, CreatedAt)
            VALUES ($eventId, $personId, $ticketType, $price, $paidAmount, $status, $notes, $checkedInAt, $createdAt);
            SELECT last_insert_rowid();";
        BindParameters(command, item);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public int AddOrReactivate(Registration item)
    {
        // The table enforces UNIQUE(EventId, PersonId), so a previously cancelled
        // registration still occupies that slot. Inserting a fresh row for the same
        // person would fail with a raw SQLite UNIQUE error, so reuse the existing
        // row instead and reactivate it with the new payment details.
        var existingId = FindId(item.EventId, item.PersonId);
        if (existingId is null) return Add(item);

        item.RegistrationId = existingId.Value;
        Update(item);
        return existingId.Value;
    }

    private static int? FindId(int eventId, int personId)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT RegistrationId FROM Registrations WHERE EventId=$eventId AND PersonId=$personId";
        command.Parameters.AddWithValue("$eventId", eventId);
        command.Parameters.AddWithValue("$personId", personId);
        var result = command.ExecuteScalar();
        return result is null || result is DBNull ? null : Convert.ToInt32(result);
    }

    public void Update(Registration item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Registrations
            SET EventId=$eventId, PersonId=$personId, TicketType=$ticketType, Price=$price, PaidAmount=$paidAmount,
                Status=$status, Notes=$notes, CheckedInAt=$checkedInAt
            WHERE RegistrationId=$id";
        command.Parameters.AddWithValue("$id", item.RegistrationId);
        BindParameters(command, item);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Registrations WHERE RegistrationId=$id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void BindParameters(Microsoft.Data.Sqlite.SqliteCommand command, Registration item)
    {
        command.Parameters.AddWithValue("$eventId", item.EventId);
        command.Parameters.AddWithValue("$personId", item.PersonId);
        command.Parameters.AddWithValue("$ticketType", item.TicketType);
        command.Parameters.AddWithValue("$price", item.Price);
        command.Parameters.AddWithValue("$paidAmount", item.PaidAmount);
        command.Parameters.AddWithValue("$status", item.Status.ToString());
        command.Parameters.AddWithValue("$notes", item.Notes);
        command.Parameters.AddWithValue("$checkedInAt", item.CheckedInAt?.ToString("s", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
        // CreatedAt is used by INSERT only; UPDATE ignores this parameter.
        command.Parameters.AddWithValue("$createdAt", item.CreatedAt.ToString("s", CultureInfo.InvariantCulture));
    }
}
