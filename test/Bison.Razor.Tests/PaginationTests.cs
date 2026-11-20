using System.Text.RegularExpressions;

namespace Bison.Razor.Tests;

// Test database: 70 observations in total.
// Alexander wrote 40 of them, Lars wrote 15, Sebastian wrote 10 and Tony wrote 5.
public class PaginationTests
{
    //each <li> in the html is one observation
    private static async Task<List<string>> GetObservationsFrom(string url) {
        using OwnDatabaseFactory factory = new OwnDatabaseFactory();

        factory.SeedPaginationData();
        using HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync(url);

        return Regex.Matches(html, "<li>(.*?)</li>", RegexOptions.Singleline)
            .Select(match => match.Groups[1].Value)
            .ToList();
    }

    [Fact]
    public async Task FirstPage_Has32Observations() {
        List<string> page = await GetObservationsFrom("/obs?page=1");

        Assert.Equal(32, page.Count);
    }

    [Fact]
    public async Task LastPage_HasTheRemaining6Observations() {
        List<string> page = await GetObservationsFrom("/obs?page=3");

        Assert.Equal(6, page.Count);
    }

    [Fact]
    public async Task PageZero_ReturnsTheFirstPage() {
        List<string> pageZero = await GetObservationsFrom("/obs?page=0");
        List<string> pageOne = await GetObservationsFrom("/obs?page=1");

        Assert.Equal(pageOne, pageZero);
    }

    [Fact]
    public async Task FirstAndSecondPage_DoNotOverlap() {
        List<string> pageOne = await GetObservationsFrom("/obs?page=1");
        List<string> pageTwo = await GetObservationsFrom("/obs?page=2");

        foreach (string observation in pageTwo) {
            Assert.DoesNotContain(observation, pageOne);
        }
    }

    [Fact]
    public async Task FirstPage_StartsWithTheNewestObservation() {
        List<string> page = await GetObservationsFrom("/obs?page=1");

        Assert.Contains("Observation number 69", page[0]);
    }

    [Fact]
    public async Task AuthorSecondPage_HasTheRemaining8ObservationsFromThatAuthor() {
        List<string> page = await GetObservationsFrom("/obs/Alexander?page=2");

        Assert.Equal(8, page.Count);
        foreach (string observation in page) {
            Assert.Contains(">Alexander</a>", observation);
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
        using OwnDatabaseFactory factory = new OwnDatabaseFactory();

        factory.SeedPaginationData();
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
        using OwnDatabaseFactory factory = new OwnDatabaseFactory();

        factory.SeedPaginationData();
        using HttpClient client = factory.CreateClient();

        string html = await client.GetStringAsync(url);

        Assert.Contains("Observation number 69", html);
        Assert.Contains("Observation number 38", html);
        Assert.DoesNotContain("Observation number 37", html);
    }
}
