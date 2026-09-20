public class BisonServiceTests
{
    [Fact]
    public async Task AddComment_ObservationDoesNotExist_ReturnsFalse()
    {
        BisonService service = new BisonService();

        bool result = await service.AddComment(
            "Test Comment",
            observationID: 999999
        );

        Assert.False(result);
    }

    [Fact]
    public async Task AddObservation_AddsObservation()
    {
        BisonService service = new BisonService();

        Observation result = await service.AddObservation(
            "Test observation"
        );

        Assert.NotNull(result);
        Assert.True(result.ID > 0);
        Assert.Equal("Test observation", result.Message);
    }

    [Fact]
    public async Task AddObservation_WithLocation_AddsObservation()
    {
        BisonService service = new BisonService();

        Observation result = await service.AddObservation(
            "Test observation with location",
            "Copenhagen"
        );

        Assert.NotNull(result);
        Assert.True(result.ID > 0);
        Assert.Equal("Copenhagen", result.Location);
    }

    [Fact]
    public async Task GetComments_ReturnsOnlyCommentsForRequestedObservation()
    {
        BisonService service = new BisonService();

        // Use an observation that actually exists in the web service.
        IEnumerable<Comment> result =
            await service.GetComments(observationID: 14);

        Assert.All(
            result,
            comment => Assert.Equal(14, comment.ObservationID)
        );
    }
}