using EventPlanner.Data;
using EventPlanner.Domain;

namespace EventPlanner.Repositories;

public class PersonRepository : IRepository<Person>
{
    private static Person ReadRow(Microsoft.Data.Sqlite.SqliteDataReader reader)
    {
        var role = Enum.Parse<PersonRole>(reader.GetString(4));
        Person person = role == PersonRole.Speaker ? new Speaker() : new Attendee();
        person.PersonId = reader.GetInt32(0);
        person.FullName = reader.GetString(1);
        person.Phone = reader.GetString(2);
        person.Email = reader.IsDBNull(3) ? "" : reader.GetString(3);
        person.IsActive = reader.GetInt32(5) == 1;

        if (person is Attendee attendee)
            attendee.Notes = reader.IsDBNull(6) ? "" : reader.GetString(6);

        if (person is Speaker speaker)
        {
            speaker.Topic = reader.IsDBNull(7) ? "" : reader.GetString(7);
            speaker.Bio = reader.IsDBNull(8) ? "" : reader.GetString(8);
        }
        return person;
    }

    public List<Person> GetAll()
    {
        var result = new List<Person>();
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PersonId, FullName, Phone, Email, Role, IsActive, Notes, Topic, Bio FROM People ORDER BY FullName";
        using var reader = command.ExecuteReader();
        while (reader.Read()) result.Add(ReadRow(reader));
        return result;
    }

    public Person? GetById(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT PersonId, FullName, Phone, Email, Role, IsActive, Notes, Topic, Bio FROM People WHERE PersonId = $id";
        command.Parameters.AddWithValue("$id", id);
        using var reader = command.ExecuteReader();
        return reader.Read() ? ReadRow(reader) : null;
    }

    public int Add(Person item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO People (FullName, Phone, Email, Role, IsActive, Notes, Topic, Bio)
            VALUES ($fullName, $phone, $email, $role, $isActive, $notes, $topic, $bio);
            SELECT last_insert_rowid();";
        BindCommonParameters(command, item);
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public void Update(Person item)
    {
        var errors = item.Validate();
        if (errors.Any()) throw new ArgumentException(string.Join(Environment.NewLine, errors));

        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE People
            SET FullName=$fullName, Phone=$phone, Email=$email, Role=$role, IsActive=$isActive,
                Notes=$notes, Topic=$topic, Bio=$bio
            WHERE PersonId=$id";
        command.Parameters.AddWithValue("$id", item.PersonId);
        BindCommonParameters(command, item);
        command.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = Database.GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM People WHERE PersonId=$id";
        command.Parameters.AddWithValue("$id", id);
        command.ExecuteNonQuery();
    }

    private static void BindCommonParameters(Microsoft.Data.Sqlite.SqliteCommand command, Person item)
    {
        command.Parameters.AddWithValue("$fullName", item.FullName);
        command.Parameters.AddWithValue("$phone", item.Phone);
        command.Parameters.AddWithValue("$email", item.Email);
        command.Parameters.AddWithValue("$role", item.Role.ToString());
        command.Parameters.AddWithValue("$isActive", item.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("$notes", item is Attendee a ? a.Notes : "");
        command.Parameters.AddWithValue("$topic", item is Speaker s ? s.Topic : "");
        command.Parameters.AddWithValue("$bio", item is Speaker sp ? sp.Bio : "");
    }
}
