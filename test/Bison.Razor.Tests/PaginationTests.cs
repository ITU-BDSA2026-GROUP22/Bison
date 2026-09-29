using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using SimpleDB;

namespace Bison.Razor.Tests;

// Test database: 70 observations in total.
// Alexander wrote 40 of them, Lars wrote 15, Sebastian wrote 10 and Tony wrote 5.
public class PaginationTests : IDisposable
{
    private readonly string _databasePath;
    private readonly DBFacade _db;

    public PaginationTests()
    {
        string fileName = "bison_pagination_test_" + Guid.NewGuid() + ".db";
        _databasePath = Path.Combine(Path.GetTempPath(), fileName);

        CreateTestDatabase(_databasePath);

        Environment.SetEnvironmentVariable("BISONDBPATH", _databasePath);
        _db = new DBFacade();
    }

    public void Dispose()
    {
        Environment.SetEnvironmentVariable("BISONDBPATH", null);
        SqliteConnection.ClearAllPools();
        File.Delete(_databasePath);
    }

    private static void CreateTestDatabase(string databasePath)
    {
        using SqliteConnection connection = new SqliteConnection($"Data Source={databasePath}");
        connection.Open();

        using SqliteCommand createTables = connection.CreateCommand();
        createTables.CommandText = """
            CREATE TABLE user (
                user_id INTEGER PRIMARY KEY AUTOINCREMENT,
                username STRING NOT NULL,
                email STRING NOT NULL
            );
            CREATE TABLE observation (
                observation_id INTEGER PRIMARY KEY AUTOINCREMENT,
                author_id INTEGER NOT NULL,
                text STRING NOT NULL,
                pub_date INTEGER
            );
            INSERT INTO user VALUES (1, 'Alexander', 'alexander@example.com');
            INSERT INTO user VALUES (2, 'Lars', 'lars@example.com');
            INSERT INTO user VALUES (3, 'Sebastian', 'sebastian@example.com');
            INSERT INTO user VALUES (4, 'Tony', 'tony@example.com');
            """;
        createTables.ExecuteNonQuery();

        for (int i = 0; i < 70; i++)
        {
            int authorId;
            if (i < 40)
            {
                authorId = 1;
            }
            else if (i < 55)
            {
                authorId = 2;
            }
            else if (i < 65)
            {
                authorId = 3;
            }
            else
            {
                authorId = 4;
            }

            using SqliteCommand insertObservation = connection.CreateCommand();
            insertObservation.CommandText = """
                INSERT INTO observation (author_id, text, pub_date)
                VALUES ($authorId, $text, $pubDate)
                """;
            insertObservation.Parameters.AddWithValue("$authorId", authorId);
            insertObservation.Parameters.AddWithValue("$text", "Observation number " + i);
            insertObservation.Parameters.AddWithValue("$pubDate", 1690000000 + i);
            insertObservation.ExecuteNonQuery();
        }
    }

    [Fact]
    public void FirstPage_Has32Observations()
    {
        List<Observation> page = _db.GetObservations(1);

        Assert.Equal(32, page.Count);
    }

    [Fact]
    public void LastPage_HasTheRemaining6Observations()
    {
        List<Observation> page = _db.GetObservations(3);

        Assert.Equal(6, page.Count);
    }

    [Fact]
    public void PageZero_ReturnsTheFirstPage()
    {
        List<Observation> pageZero = _db.GetObservations(0);
        List<Observation> pageOne = _db.GetObservations(1);

        Assert.Equal(pageOne, pageZero);
    }

    [Fact]
    public void FirstAndSecondPage_DoNotOverlap()
    {
        List<Observation> pageOne = _db.GetObservations(1);
        List<Observation> pageTwo = _db.GetObservations(2);

        foreach (Observation observation in pageTwo)
        {
            Assert.DoesNotContain(observation, pageOne);
        }
    }

    [Fact]
    public void FirstPage_StartsWithTheNewestObservation()
    {
        List<Observation> page = _db.GetObservations(1);

        Assert.Equal("Observation number 69", page[0].Message);
    }

    [Fact]
    public void AuthorSecondPage_HasTheRemaining8ObservationsFromThatAuthor()
    {
        List<Observation> page = _db.GetObservationsFromAuthor("Alexander", 2);

        Assert.Equal(8, page.Count);
        foreach (Observation observation in page)
        {
            Assert.Equal("Alexander", observation.Author);
        }
    }

    [Theory]
    [InlineData("/obs", true)]
    [InlineData("/obs?page=2", true)]
    [InlineData("/obs?page=3", false)]
    [InlineData("/obs/Alexander", true)]
    [InlineData("/obs/Alexander?page=2", false)]
    [InlineData("/obs/Lars", false)]
    public async Task NextLink_IsOnlyShownWhenThereIsANextPage(string url, bool expectNextLink)
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync(url);

        if (expectNextLink)
        {
            Assert.Contains(">Next</a>", html);
        }
        else
        {
            Assert.DoesNotContain(">Next</a>", html);
        }
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/obs")]
    [InlineData("/obs?page=1")]
    public async Task PublicTimeline_WithoutPageOrWithPage1_ShowsTheSameFirstPage(string url)
    {
        using WebApplicationFactory<Program> factory = new WebApplicationFactory<Program>();
        using HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync(url);

        Assert.Contains("Observation number 69", html);
        Assert.Contains("Observation number 38", html);
        Assert.DoesNotContain("Observation number 37", html);
    }
}
