using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace WpfCrud
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string dbPath = "contacts.db")
        {
            _connectionString = $"Data Source={dbPath}";
            EnsureSchema();
        }

        // Schema

        private void EnsureSchema()
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                CREATE TABLE IF NOT EXISTS Contacts (
                    Id        INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name      TEXT    NOT NULL,
                    Email     TEXT    NOT NULL DEFAULT '',
                    Phone     TEXT    NOT NULL DEFAULT '',
                    CreatedAt TEXT    NOT NULL DEFAULT (datetime('now','localtime'))
                );
                """;
            cmd.ExecuteNonQuery();
        }

        // CRUD 

        public List<Contact> GetAll()
        {
            var list = new List<Contact>();
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Id, Name, Email, Phone, CreatedAt FROM Contacts ORDER BY Id DESC;";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(Map(reader));
            return list;
        }

        public void Insert(Contact c)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                INSERT INTO Contacts (Name, Email, Phone)
                VALUES ($name, $email, $phone);
                """;
            cmd.Parameters.AddWithValue("$name",  c.Name);
            cmd.Parameters.AddWithValue("$email", c.Email);
            cmd.Parameters.AddWithValue("$phone", c.Phone);
            cmd.ExecuteNonQuery();
        }

        public void Update(Contact c)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = """
                UPDATE Contacts
                SET Name=$name, Email=$email, Phone=$phone
                WHERE Id=$id;
                """;
            cmd.Parameters.AddWithValue("$id",    c.Id);
            cmd.Parameters.AddWithValue("$name",  c.Name);
            cmd.Parameters.AddWithValue("$email", c.Email);
            cmd.Parameters.AddWithValue("$phone", c.Phone);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM Contacts WHERE Id=$id;";
            cmd.Parameters.AddWithValue("$id", id);
            cmd.ExecuteNonQuery();
        }

        // Helpers

        private SqliteConnection Open()
        {
            var conn = new SqliteConnection(_connectionString);
            conn.Open();
            return conn;
        }

        private static Contact Map(SqliteDataReader r) => new Contact
        {
            Id        = r.GetInt32(0),
            Name      = r.GetString(1),
            Email     = r.GetString(2),
            Phone     = r.GetString(3),
            CreatedAt = DateTime.Parse(r.GetString(4))
        };
    }
}
