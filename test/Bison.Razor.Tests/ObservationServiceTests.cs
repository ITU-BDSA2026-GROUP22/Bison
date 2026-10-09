using System.Globalization;

namespace Bison.Razor.Tests;

public class ObservationServiceTests : IDisposable {
    private readonly string _databasePath;
    private readonly ObservationService _service;

    public ObservationServiceTests() {
        _databasePath = TestDatabase.CreateExampleDatabase();
        _service = new ObservationService(new PostRepository(_databasePath));
    }

    public void Dispose() {
        TestDatabase.Delete(_databasePath);
    }

    [Fact]
    public void GetObservations_KeepsAuthorAndMessage() {
        List<ObservationViewModel> observations = _service.GetObservations();

        Assert.Contains(
            observations,
            observation => observation.Author == "Peter" && observation.Message == TestDatabase.PetersObservation);
    }

    [Fact]
    public void GetObservationsFromAuthor_OnlyReturnsThatAuthorsObservations() {
        List<ObservationViewModel> observations = _service.GetObservationsFromAuthor("Petra");

        ObservationViewModel observation = Assert.Single(observations);
        Assert.Equal("Petra", observation.Author);
        Assert.Equal(TestDatabase.PetrasObservation, observation.Message);
    }

    [Fact]
    public void GetObservationsFromAuthor_ConvertsUnixTimestampToUtcDateTime() {
        ObservationViewModel observation = Assert.Single(_service.GetObservationsFromAuthor("Petra"));

        //parses with the current culture, since the date separator is different on e.g. danish computers
        DateTime shownTime = DateTime.ParseExact(
            observation.Timestamp,
            "MM/dd/yy H:mm:ss",
            CultureInfo.CurrentCulture);

        //petras observation has the unix timestamp 1690892208
        Assert.Equal(new DateTime(2023, 8, 1, 12, 16, 48), shownTime);
    }
}
