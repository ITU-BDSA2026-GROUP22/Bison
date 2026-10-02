using Microsoft.Data.Sqlite;
using SimpleDB;

public class DBFacade
{
    // Maximum number of observations returned per page
    public const int PageSize = 32;

    private readonly string connectionString;

    public string DatabasePath { get; }

    public DBFacade() : this(GetDatabasePathFromEnvironment()) {
    }

    //lets the tests use their own database instead of the one from BISONDBPATH
    public DBFacade(string databasePath) {
        DatabasePath = databasePath;
        connectionString = $"Data Source={databasePath}";
    }

    private static string GetDatabasePathFromEnvironment() {
        string? databasePath = Environment.GetEnvironmentVariable("BISONDBPATH");

        if (string.IsNullOrEmpty(databasePath)) {
            databasePath = Path.Combine(Path.GetTempPath(), "bison.db");
        }

        return databasePath;
    }

    // Returns one page of observations, newest first. Pages start at 1.
    public List<Observation> GetObservations(int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        List<Observation> observations = new List<Observation>();

        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT
                observation.observation_id,
                observation.text,
                observation.pub_date,
                user.username
            FROM observation
            JOIN user
                ON observation.author_id = user.user_id
            ORDER BY observation.pub_date DESC
            LIMIT $limit OFFSET $offset
            """;

        int rowsToSkip = (page - 1) * PageSize;

        command.Parameters.AddWithValue("$limit", PageSize);
        command.Parameters.AddWithValue("$offset", rowsToSkip);

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            Observation observation = new Observation(
                reader.GetInt32(
                    reader.GetOrdinal("observation_id")
                ),
                reader.GetString(
                    reader.GetOrdinal("username")
                ),
                reader.GetString(
                    reader.GetOrdinal("text")
                ),
                reader.GetInt64(
                    reader.GetOrdinal("pub_date")
                )
            );

            observations.Add(observation);
        }

        return observations;
    }

    // Returns one page of the given author's observations, newest first. Pages start at 1.
    public List<Observation> GetObservationsFromAuthor(string author, int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        List<Observation> observations = new List<Observation>();

        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT
                observation.observation_id,
                observation.text,
                observation.pub_date,
                user.username
            FROM observation
            JOIN user
                ON observation.author_id = user.user_id
            WHERE user.username = $author
            ORDER BY observation.pub_date DESC
            LIMIT $limit OFFSET $offset
            """;

        int rowsToSkip = (page - 1) * PageSize;

        command.Parameters.AddWithValue("$author", author);
        command.Parameters.AddWithValue("$limit", PageSize);
        command.Parameters.AddWithValue("$offset", rowsToSkip);

        using SqliteDataReader reader = command.ExecuteReader();

        while (reader.Read())
        {
            Observation observation = new Observation(
                reader.GetInt32(
                    reader.GetOrdinal("observation_id")
                ),
                reader.GetString(
                    reader.GetOrdinal("username")
                ),
                reader.GetString(
                    reader.GetOrdinal("text")
                ),
                reader.GetInt64(
                    reader.GetOrdinal("pub_date")
                )
            );

            observations.Add(observation);
        }

        return observations;
    }

    public Observation? GetObservation(int id)
    {
        using SqliteConnection connection = new SqliteConnection(connectionString);
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();

        command.CommandText =
            "SELECT observation.observation_id, user.username, observation.text, observation.pub_date " +
            "FROM observation " +
            "JOIN user ON observation.author_id = user.user_id " +
            "WHERE observation.observation_id = $id";

        command.Parameters.AddWithValue("$id", id);

        using SqliteDataReader reader = command.ExecuteReader();

        if (reader.Read())
        {
            int observationId = reader.GetInt32(0);
            string author = reader.GetString(1);
            string message = reader.GetString(2);
            long timestamp = reader.GetInt64(3);

            Observation observation = new Observation(
                observationId,
                author,
                message,
                timestamp
            );

            return observation;
        }

        return null;
    }
    
    public int CountObservations()
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT COUNT(*)
            FROM observation
            """;

        object? result = command.ExecuteScalar();
        int count = Convert.ToInt32(result);

        return count;
    }

    public int CountObservationsFromAuthor(string author)
    {
        using SqliteConnection connection =
            new SqliteConnection(connectionString);

        connection.Open();

        using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = """
            SELECT COUNT(*)
            FROM observation
            JOIN user
                ON observation.author_id = user.user_id
            WHERE user.username = $author
            """;

        command.Parameters.AddWithValue("$author", author);

        object? result = command.ExecuteScalar();
        int count = Convert.ToInt32(result);

        return count;
    }
}