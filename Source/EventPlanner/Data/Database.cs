using Microsoft.Data.Sqlite;
using System.IO;

namespace EventPlanner.Data;

public static class Database
{
    public static string DatabasePath { get; } =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "eventplanner.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection GetConnection()
    {
        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        // SQLite enforces foreign keys only when this pragma is set, and only per connection.
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }

    public static void Initialize()
    {
        using var connection = GetConnection();
        using var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Events (
                EventId INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                StartAt TEXT NOT NULL,
                Location TEXT NOT NULL,
                Capacity INTEGER NOT NULL,
                Status TEXT NOT NULL,
                Description TEXT
            );

            CREATE TABLE IF NOT EXISTS People (
                PersonId INTEGER PRIMARY KEY AUTOINCREMENT,
                FullName TEXT NOT NULL,
                Phone TEXT NOT NULL,
                Email TEXT,
                Role TEXT NOT NULL,
                IsActive INTEGER NOT NULL,
                Notes TEXT,
                Topic TEXT,
                Bio TEXT
            );

            CREATE TABLE IF NOT EXISTS Registrations (
                RegistrationId INTEGER PRIMARY KEY AUTOINCREMENT,
                EventId INTEGER NOT NULL,
                PersonId INTEGER NOT NULL,
                TicketType TEXT NOT NULL,
                Price REAL NOT NULL,
                PaidAmount REAL NOT NULL,
                Status TEXT NOT NULL,
                Notes TEXT,
                CheckedInAt TEXT NULL,
                CreatedAt TEXT NOT NULL,
                UNIQUE(EventId, PersonId),
                FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE,
                FOREIGN KEY (PersonId) REFERENCES People(PersonId) ON DELETE CASCADE
            );

            CREATE TABLE IF NOT EXISTS Expenses (
                ExpenseId INTEGER PRIMARY KEY AUTOINCREMENT,
                EventId INTEGER NOT NULL,
                Category TEXT NOT NULL,
                Amount REAL NOT NULL,
                Notes TEXT,
                PaidAt TEXT NULL,
                CreatedAt TEXT NOT NULL,
                FOREIGN KEY (EventId) REFERENCES Events(EventId) ON DELETE CASCADE
            );";
        command.ExecuteNonQuery();
    }
}
