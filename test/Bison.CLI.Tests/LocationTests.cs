using SimpleDB;

public class LocationTests
{
    // Integration test:
    // tests that BisonService can retrieve observations through the web service
    // and filter them by location.
    [Fact]
    public async Task ReadObservationsAtOnlyReturnsMatchingLocation()
    {
        BisonService service = new BisonService();

        await service.AddObservation("Heron", "Copenhagen");
        await service.AddObservation("Penguin", "Oelstykke");

        // Lowercase to test case-insensitive filtering
        IEnumerable<Observation> result =
            await service.ReadObservationsAt("copenhagen");

        Observation observation = Assert.Single(result);

        Assert.Equal("Heron", observation.Message);
        Assert.Equal("Copenhagen", observation.Location);
    }

    // Integration test:
    // checks that the real CSVDatabase can store and retrieve a location.
    [Fact]
    public void StoredLocationCanBeRetrieved()
    {
        string file = Path.GetTempFileName();

        try
        {
            CSVDatabase<Observation> database =
                new CSVDatabase<Observation>(file);

            Observation observation =
                new Observation(1, "Tony", "Heron", 0, "Copenhagen");

            database.Store(observation);

            Observation result = database.Read().Single();

            Assert.Equal(observation, result);
        }
        finally
        {
            File.Delete(file);
        }
    }
}