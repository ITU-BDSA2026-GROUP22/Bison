public class BisonServiceTests
{
    [Fact]
    public void AddComment_ObservationDoesNotExist_ReturnsFalseAndDoesNotStore()
    {
        TestDatabaseRepository<Observation> observations = new TestDatabaseRepository<Observation>();
        TestDatabaseRepository<Comment> comments = new TestDatabaseRepository<Comment>();
        BisonService service = new BisonService(observations, comments);

        bool result = service.AddComment("Test Comment", observationID: 42);

        Assert.False(result);
        Assert.Empty(comments.Records);
    }

    [Fact]
    public void AddObservation_NoExistingObservations_AssignsID1()
    {
        TestDatabaseRepository<Observation> observations = new TestDatabaseRepository<Observation>();
        TestDatabaseRepository<Comment> comments = new TestDatabaseRepository<Comment>();
        BisonService service = new BisonService(observations, comments);

        Observation result = service.AddObservation("first sighting");

        Assert.Equal(1, result.ID);
    }

    [Fact]
    public void AddObservation_ExistingObservations_AssignsMaxIDPlusOne()
    {
        TestDatabaseRepository<Observation> observations = new TestDatabaseRepository<Observation>();
        Observation firstExistingObservation = new Observation(1, "Bob", "TestMessage1", 1000);
        Observation secondExistingObservation = new Observation(5, "Carl", "TestMessage2", 1000);
        observations.Store(firstExistingObservation);
        observations.Store(secondExistingObservation);

        TestDatabaseRepository<Comment> comments = new TestDatabaseRepository<Comment>();
        BisonService service = new BisonService(observations, comments);

        Observation result = service.AddObservation("new sighting");

        Assert.Equal(6, result.ID);
    }

    [Fact]
    public void GetComments_ReturnsOnlyCommentsForRequestedObservation()
    {
        TestDatabaseRepository<Observation> observations = new TestDatabaseRepository<Observation>();
        TestDatabaseRepository<Comment> comments = new TestDatabaseRepository<Comment>();

        Comment firstCommentOnObservationOne = new Comment(1, "Bob", "TestComment1", 1000);
        Comment commentOnObservationTwo = new Comment(2, "Carl", "TestComment2", 1000);
        Comment secondCommentOnObservationOne = new Comment(1, "Daniel", "TestComment3", 1000);
        comments.Store(firstCommentOnObservationOne);
        comments.Store(commentOnObservationTwo);
        comments.Store(secondCommentOnObservationOne);

        BisonService service = new BisonService(observations, comments);

        IEnumerable<Comment> result = service.GetComments(observationID: 1);

        Assert.Equal(2, result.Count());
    }
}