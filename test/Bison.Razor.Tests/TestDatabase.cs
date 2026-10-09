using Microsoft.Data.Sqlite;

namespace Bison.Razor.Tests;

//makes a new database file for each test, with the example data from the assignment
public static class TestDatabase {
    public const string PetersObservation = "A big gray bird in a pond at DR byen";
    public const string PetrasObservation = "A heron";
    public const string EduardsObservation = "Two swans at Peblinge Lake";

    private const string ExampleData = """
        INSERT INTO user VALUES(1,'Peter','peter@itu.dk');
        INSERT INTO user VALUES(2,'Petra','petra@itu.dk');
        INSERT INTO user VALUES(3,'Eduard','edka@itu.dk');

        INSERT INTO observation VALUES(1,2,'A heron',1690892208);
        INSERT INTO observation VALUES(2,1,'A big gray bird in a pond at DR byen',1690895308);
        INSERT INTO observation VALUES(3,3,'Two swans at Peblinge Lake',1690899000);
        """;

    public static string CreateExampleDatabase() {
        string fileName = "bison_test_" + Guid.NewGuid() + ".db";
        string databasePath = Path.Combine(Path.GetTempPath(), fileName);

        using SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();

        using SqliteCommand command = connection.CreateCommand();
        command.CommandText = ReadSchema() + ExampleData;
        command.ExecuteNonQuery();

        return databasePath;
    }

    public static void Delete(string databasePath) {
        //the connection pool keeps the file open, so it cant be deleted on windows without clearing it first
        SqliteConnection.ClearAllPools();
        File.Delete(databasePath);
    }

    private static string ReadSchema() {
        using Stream? stream = typeof(PostRepository).Assembly
            .GetManifestResourceStream("Bison.SQLite.data.schema.sql");

        if (stream == null) {
            throw new InvalidOperationException("Could not find the embedded schema.sql in Bison.SQLite.");
        }

        using StreamReader reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
