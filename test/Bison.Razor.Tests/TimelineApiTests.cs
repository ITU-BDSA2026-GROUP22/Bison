using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Bison.Infrastructure.Data;
using Bison.Infrastructure.Models;

namespace Bison.Razor.Tests;

//starts the app with its own test database, so these tests dont touch the real bison.db
public class ExampleDatabaseFactory : WebApplicationFactory<Program> {
    public string EFDatabasePath { get; } = EFTestDatabase.CreatePath();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
            EFTestDatabase.UseDatabase(services, EFDatabasePath);
        });
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        EFTestDatabase.Delete(EFDatabasePath);
    }

        public void SeedExampleEFDatabase()
    {
        using IServiceScope scope = Services.CreateScope();

        BisonDBContext db =
            scope.ServiceProvider.GetRequiredService<BisonDBContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        Author peter = new()
        {
            Name = "Peter",
            Email = "peter@example.com"
        };

        Author petra = new()
        {
            Name = "Petra",
            Email = "petra@example.com"
        };

        Author eduard = new()
        {
            Name = "Eduard",
            Email = "eduard@example.com"
        };

        Taxon taxon = new()
        {
            dwc_TaxonID = "test-taxon",
            VernacularName = "Test"
        };

        db.Authors.AddRange(peter, petra, eduard);
        db.Taxons.Add(taxon);
        db.SaveChanges();

        db.Observations.AddRange(
            new Observation
            {
                Author = peter,
                Taxon = taxon,
                Text = EFTestDatabase.PetersObservation,
                TimeStamp = DateTime.UtcNow
            },
            new Observation
            {
                Author = petra,
                Taxon = taxon,
                Text = EFTestDatabase.PetrasObservation,
                TimeStamp = DateTime.UtcNow.AddSeconds(1)
            },
            new Observation
            {
                Author = eduard,
                Taxon = taxon,
                Text = EFTestDatabase.EduardsObservation,
                TimeStamp = DateTime.UtcNow.AddSeconds(2)
            });

        db.SaveChanges();
    }
}

public class TimelineApiTests : IClassFixture<ExampleDatabaseFactory> {
    private readonly HttpClient _client;

    public TimelineApiTests(ExampleDatabaseFactory factory) {
        factory.SeedExampleEFDatabase();
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
            observation => IsObservationBy(observation, "Peter", EFTestDatabase.PetersObservation));
    }

    [Fact]
    public async Task PublicTimeline_ContainsObservationsFromAllAuthors() {
        string html = await _client.GetStringAsync("/obs");

        List<string> observations = ObservationsIn(html);

        Assert.Equal(3, observations.Count);
        Assert.Contains(observations, observation => IsObservationBy(observation, "Petra", EFTestDatabase.PetrasObservation));
        Assert.Contains(observations, observation => IsObservationBy(observation, "Eduard", EFTestDatabase.EduardsObservation));
    }

    [Theory]
    [InlineData("/Petra")]
    [InlineData("/obs/Petra")]
    public async Task PrivateTimeline_ContainsPetrasObservation(string url) {
        string html = await _client.GetStringAsync(url);

        Assert.Contains(
            ObservationsIn(html),
            observation => IsObservationBy(observation, "Petra", EFTestDatabase.PetrasObservation));
    }

    [Fact]
    public async Task PrivateTimeline_OnlyContainsThatAuthorsObservations() {
        string html = await _client.GetStringAsync("/obs/Petra");

        string observation = Assert.Single(ObservationsIn(html));
        Assert.Contains(">Petra</a>", observation);
        Assert.DoesNotContain(EFTestDatabase.PetersObservation, html);
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
