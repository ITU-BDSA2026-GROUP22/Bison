using SimpleDB;

public class BisonServiceTests
{
    [Fact]
    public async Task AddObservation_ReturnsCreatedObservation()
    {
        BisonService service = new BisonService();

        Observation result =
            await service.AddObservation("Test observation", "Copenhagen");

        Assert.NotNull(result);
        Assert.Equal("Test observation", result.Message);
    }

    [Fact]
    public async Task ObservationExists_ReturnsTrueForExistingObservation()
    {
        BisonService service = new BisonService();

        Observation observation =
            await service.AddObservation("Test observation", "Copenhagen");

        bool result =
            await service.ObservationExists(observation.ID);

        Assert.True(result);
    }

    [Fact]
    public async Task ObservationExists_ReturnsFalseForNonExistingObservation()
    {
        BisonService service = new BisonService();

        bool result =
            await service.ObservationExists(999999);

        Assert.False(result);
    }

    [Fact]
    public async Task AddComment_ReturnsFalseWhenObservationDoesNotExist()
    {
        BisonService service = new BisonService();

        bool result =
            await service.AddComment("Test comment", 999999);

        Assert.False(result);
    }

    [Fact]
    public async Task AddComment_ReturnsTrueForExistingObservation()
    {
        BisonService service = new BisonService();

        Observation observation =
            await service.AddObservation("Test observation", "Copenhagen");

        bool result =
            await service.AddComment("Test comment", observation.ID);

        Assert.True(result);
    }

    [Fact]
    public async Task GetComments_ReturnsCommentsForObservation()
    {
        BisonService service = new BisonService();

        Observation observation =
            await service.AddObservation("Test observation", "Copenhagen");

        await service.AddComment("Test comment", observation.ID);

        IEnumerable<Comment> comments =
            await service.GetComments(observation.ID);

        Assert.Contains(
            comments,
            comment => comment.ObservationID == observation.ID
        );
    }
}