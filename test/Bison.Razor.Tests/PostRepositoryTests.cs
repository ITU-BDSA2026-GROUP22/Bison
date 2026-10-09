using SimpleDB;

namespace Bison.Razor.Tests;

public class PostRepositoryTests : IDisposable {
    private readonly string _databasePath;
    private readonly IPostRepository _db;

    public PostRepositoryTests() {
        _databasePath = TestDatabase.CreateExampleDatabase();
        _db = new PostRepository(_databasePath);
    }

    public void Dispose() {
        TestDatabase.Delete(_databasePath);
    }

    [Fact]
    public void GetObservations_ReturnsAllObservations() {
        List<Observation> observations = _db.GetObservations();

        Assert.Equal(3, observations.Count);
    }

    [Fact]
    public void GetObservations_JoinsEachObservationWithItsAuthor() {
        List<Observation> observations = _db.GetObservations();

        Observation petersObservation = Assert.Single(
            observations,
            observation => observation.Message == TestDatabase.PetersObservation);
        Observation petrasObservation = Assert.Single(
            observations,
            observation => observation.Message == TestDatabase.PetrasObservation);

        Assert.Equal("Peter", petersObservation.Author);
        Assert.Equal("Petra", petrasObservation.Author);
    }

    [Fact]
    public void GetObservations_ReturnsNewestFirst() {
        List<Observation> observations = _db.GetObservations();

        Assert.Equal(TestDatabase.EduardsObservation, observations[0].Message);
        Assert.Equal(TestDatabase.PetrasObservation, observations[2].Message);
    }

    [Fact]
    public void GetObservationsFromAuthor_OnlyReturnsThatAuthorsObservations() {
        List<Observation> observations = _db.GetObservationsFromAuthor("Petra");

        Observation observation = Assert.Single(observations);
        Assert.Equal("Petra", observation.Author);
        Assert.Equal(TestDatabase.PetrasObservation, observation.Message);
    }

    [Fact]
    public void GetObservationsFromAuthor_ReturnsEmptyListForUnknownAuthor() {
        List<Observation> observations = _db.GetObservationsFromAuthor("NoSuchUser");

        Assert.Empty(observations);
    }

    [Fact]
    public void GetObservationsFromAuthor_TreatsAuthorAsValueNotAsSql() {
        //if the author got put straight into the sql query, this would return every observation
        List<Observation> observations = _db.GetObservationsFromAuthor("' OR '1'='1");

        Assert.Empty(observations);
    }

    [Fact]
    public void CountObservations_CountsAllObservations() {
        Assert.Equal(3, _db.CountObservations());
    }

    [Fact]
    public void CountObservationsFromAuthor_OnlyCountsThatAuthorsObservations() {
        Assert.Equal(1, _db.CountObservationsFromAuthor("Peter"));
        Assert.Equal(0, _db.CountObservationsFromAuthor("NoSuchUser"));
    }
}
