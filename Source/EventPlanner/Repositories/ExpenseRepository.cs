using System.Globalization;
using EventPlanner.Data;
using EventPlanner.Domain;

namespace EventPlanner.Repositories;

public class ExpenseRepository : IRepository<Expense>
{
    private const string SelectColumns =
        "ExpenseId, EventId, Category, Amount, Notes, PaidAt, CreatedAt";

    private static Expense ReadRow(Microsoft.Data.Sqlite.SqliteDataReader reader) => new()
    {
        ExpenseId = reader.GetInt32(0),
        EventId = reader.GetInt32(1),
        Category = reader.GetString(2),
        Amount = Convert.ToDecimal(reader.GetDouble(3)),
        Notes = reader.IsDBNull(4) ? "" : reader.GetString(4),
        PaidAt = reader.IsDBNull(5) ? null : DateTime.ParseExact(reader.GetString(5), "s", CultureInfo.InvariantCulture),
        CreatedAt = DateTime.ParseExact(reader.GetString(6), "s", CultureInfo.InvariantCulture)
    };

    public List<Expense> GetAll()
    {
        var result = new List<Expense>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Expenses ORDER BY CreatedAt DESC";
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public Expense? GetById(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Expenses WHERE ExpenseId = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadRow(reader) : null;
    }

    public List<Expense> GetByEventId(int eventId)
    {
        var result = new List<Expense>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {SelectColumns} FROM Expenses WHERE EventId = $eventId ORDER BY CreatedAt DESC";
        command.Parameters.AddWithValue("$eventId", eventId);
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public int Add(Expense item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Expenses (EventId, Category, Amount, Notes, PaidAt, CreatedAt)
            VALUES ($eventId, $category, $amount, $notes, $paidAt, $createdAt);
            SELECT last_insert_rowid();";
        BindParameters(command, item);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Expense item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Expenses
            SET EventId=$eventId, Category=$category, Amount=$amount, Notes=$notes, PaidAt=$paidAt
            WHERE ExpenseId=$id";
        command.Parameters.AddWithValue("$id", item.ExpenseId);
        BindParameters(command, item);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Expenses WHERE ExpenseId=$id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void BindParameters(Microsoft.Data.Sqlite.SqliteCommand command, Expense item)
    {
        command.Parameters.AddWithValue("$eventId", item.EventId);
        command.Parameters.AddWithValue("$category", item.Category);
        command.Parameters.AddWithValue("$amount", item.Amount);
        command.Parameters.AddWithValue("$notes", item.Notes);
        command.Parameters.AddWithValue("$paidAt", item.PaidAt?.ToString("s", CultureInfo.InvariantCulture) ?? (object)DBNull.Value);
        // CreatedAt is used by INSERT only; UPDATE ignores this parameter.
        command.Parameters.AddWithValue("$createdAt", item.CreatedAt.ToString("s", CultureInfo.InvariantCulture));
    }
}
