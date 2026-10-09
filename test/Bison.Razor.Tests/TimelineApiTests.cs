using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

//starts the app with its own test database. The DBFacade gets replaced directly, so these tests dont use BISONDBPATH
public class ExampleDatabaseFactory : WebApplicationFactory<Program> {
    public string DatabasePath { get; } = TestDatabase.CreateExampleDatabase();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
            services.AddScoped<IPostRepository>(
            provider => new PostRepository(DatabasePath));
        });
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        TestDatabase.Delete(DatabasePath);
    }
}

public class TimelineApiTests : IClassFixture<ExampleDatabaseFactory> {
    private readonly HttpClient _client;

    public TimelineApiTests(ExampleDatabaseFactory factory) {
        _client = factory.CreateClient();
    }

    //each <li> in the html is one observation
    private static List<string> ObservationsIn(string html) {
        return Regex.Matches(html, "<li>(.*?)</li>", RegexOptions.Singleline)
            .Select(match => match.Groups[1].Value)
            .ToList();
    }

    private static bool IsObservationBy(string observationHtml, string author, string message) {
        return observationHtml.Contains($">{author}</a>") && observationHtml.Contains(message);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/obs")]
    public async Task PublicTimeline_RespondsWithHtml(string url) {
        HttpResponseMessage response = await _client.GetAsync(url);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Theory]
    [InlineData("/")]
    [InlineData("/obs")]
    public async Task PublicTimeline_ContainsPetersObservation(string url) {
        string html = await _client.GetStringAsync(url);

        Assert.Contains(
            ObservationsIn(html),
            observation => IsObservationBy(observation, "Peter", TestDatabase.PetersObservation));
    }

    [Fact]
    public async Task PublicTimeline_ContainsObservationsFromAllAuthors() {
        string html = await _client.GetStringAsync("/obs");

        List<string> observations = ObservationsIn(html);

        Assert.Equal(3, observations.Count);
        Assert.Contains(observations, observation => IsObservationBy(observation, "Petra", TestDatabase.PetrasObservation));
        Assert.Contains(observations, observation => IsObservationBy(observation, "Eduard", TestDatabase.EduardsObservation));
    }

    [Theory]
    [InlineData("/Petra")]
    [InlineData("/obs/Petra")]
    public async Task PrivateTimeline_ContainsPetrasObservation(string url) {
        string html = await _client.GetStringAsync(url);

        Assert.Contains(
            ObservationsIn(html),
            observation => IsObservationBy(observation, "Petra", TestDatabase.PetrasObservation));
    }

    [Fact]
    public async Task PrivateTimeline_OnlyContainsThatAuthorsObservations() {
        string html = await _client.GetStringAsync("/obs/Petra");

        string observation = Assert.Single(ObservationsIn(html));
        Assert.Contains(">Petra</a>", observation);
        Assert.DoesNotContain(TestDatabase.PetersObservation, html);
    }

    [Fact]
    public async Task PrivateTimeline_ForUnknownUser_ShowsNoObservations() {
        HttpResponseMessage response = await _client.GetAsync("/obs/NoSuchUser");
        string html = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(ObservationsIn(html));
        Assert.Contains("There are no Observations so far.", html);
    }
}
