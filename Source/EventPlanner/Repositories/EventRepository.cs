using System.Globalization;
using EventPlanner.Data;
using EventPlanner.Domain;

namespace EventPlanner.Repositories;

public class EventRepository : IRepository<Event>
{
    private static Event ReadRow(Microsoft.Data.Sqlite.SqliteDataReader reader) => new()
    {
        EventId = reader.GetInt32(0),
        Title = reader.GetString(1),
        StartAt = DateTime.ParseExact(reader.GetString(2), "s", CultureInfo.InvariantCulture),
        Location = reader.GetString(3),
        Capacity = reader.GetInt32(4),
        Status = Enum.Parse<EventStatus>(reader.GetString(5)),
        Description = reader.IsDBNull(6) ? "" : reader.GetString(6)
    };

    public List<Event> GetAll()
    {
        var result = new List<Event>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EventId, Title, StartAt, Location, Capacity, Status, Description FROM Events ORDER BY StartAt DESC";
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public Event? GetById(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT EventId, Title, StartAt, Location, Capacity, Status, Description FROM Events WHERE EventId = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadRow(reader) : null;
    }

    public int Add(Event item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Events (Title, StartAt, Location, Capacity, Status, Description)
            VALUES ($title, $startAt, $location, $capacity, $status, $description);
            SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("$title", item.Title);
        command.Parameters.AddWithValue("$startAt", item.StartAt.ToString("s", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$location", item.Location);
        command.Parameters.AddWithValue("$capacity", item.Capacity);
        command.Parameters.AddWithValue("$status", item.Status.ToString());
        command.Parameters.AddWithValue("$description", item.Description);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Event item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Events
            SET Title=$title, StartAt=$startAt, Location=$location, Capacity=$capacity, Status=$status, Description=$description
            WHERE EventId=$id";
        command.Parameters.AddWithValue("$id", item.EventId);
        command.Parameters.AddWithValue("$title", item.Title);
        command.Parameters.AddWithValue("$startAt", item.StartAt.ToString("s", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$location", item.Location);
        command.Parameters.AddWithValue("$capacity", item.Capacity);
        command.Parameters.AddWithValue("$status", item.Status.ToString());
        command.Parameters.AddWithValue("$description", item.Description);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Events WHERE EventId=$id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }
}
