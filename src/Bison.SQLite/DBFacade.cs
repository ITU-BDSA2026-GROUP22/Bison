using Microsoft.Data.Sqlite;
using SimpleDB;

public class DBFacade
{
    private readonly string connectionString;

    public DBFacade()
    {
        string? databasePath =
            Environment.GetEnvironmentVariable("BISONDBPATH");

        if (string.IsNullOrEmpty(databasePath))
        {
            databasePath = Path.Combine(
                Path.GetTempPath(),
                "bison.db"
            );
        }

        connectionString = $"Data Source={databasePath}";
    }

    public List<Observation> GetObservations()
    {
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
            """;

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

    public List<Observation> GetObservationsFromAuthor(string author)
    {
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
            """;

        command.Parameters.AddWithValue("$author", author);

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
}