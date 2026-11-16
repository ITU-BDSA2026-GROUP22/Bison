using Bison.Razor.Data;
using Bison.Razor.Models;
using Deque.AxeCore.Commons;
using Deque.AxeCore.Playwright;
using Microsoft.Playwright;

namespace Bison.Razor.E2ETests;

[Collection("Bison app")]
public class EndToEndTests
{
    private BisonAppFixture app;

    public EndToEndTests(BisonAppFixture app)
    {
        this.app = app;
    }

    [Fact]
    public async Task ObservationIsSavedInTheDatabaseForTheAuthor()
    {
        IPage page = await app.NewPageAsync();
        string observationText = "I saw a bison " + Guid.NewGuid();

        await LogIn(page, "edka@itu.dk", "123456");

        await page.Locator("#ObservationText").FillAsync(observationText);
        await page.Locator("input[value='Observe']").ClickAsync();

        await Assertions.Expect(page.Locator("#messagelist")).ToContainTextAsync(observationText);

        using (BisonDBContext database = app.CreateDbContext())
        {
            Author author = database.Authors.Single(author => author.Email == "edka@itu.dk");
            Observation observation = database.Observations.Single(observation => observation.Text == observationText);

            Assert.Equal(author.AuthorId, observation.AuthorId);
        }
    }

    [Fact]
    public async Task ObservationFromSecondUserIsSavedForThatUser()
    {
        IPage page = await app.NewPageAsync();
        string observationText = "I saw a heron " + Guid.NewGuid();

        await LogIn(page, "debloh@itu.dk", "password");

        await page.Locator("#ObservationText").FillAsync(observationText);
        await page.Locator("input[value='Observe']").ClickAsync();

        await Assertions.Expect(page.Locator("#messagelist")).ToContainTextAsync(observationText);

        using (BisonDBContext database = app.CreateDbContext())
        {
            Author author = database.Authors.Single(author => author.Email == "debloh@itu.dk");
            Observation observation = database.Observations.Single(observation => observation.Text == observationText);

            Assert.Equal(author.AuthorId, observation.AuthorId);
        }
    }

    [Fact]
    public async Task EmptyObservationIsNotSaved()
    {
        IPage page = await app.NewPageAsync();
        await LogIn(page, "edka@itu.dk", "123456");

        int observationsBefore = CountObservationsFor("edka@itu.dk");

        Task<IResponse> postResponse = page.WaitForResponseAsync(page.Url);
        await page.Locator("input[value='Observe']").ClickAsync();
        await postResponse;

        int observationsAfter = CountObservationsFor("edka@itu.dk");
        Assert.Equal(observationsBefore, observationsAfter);
    }

    [Fact]
    public async Task ObservationLongerThan500CharactersIsNotSaved()
    {
        IPage page = await app.NewPageAsync();
        await LogIn(page, "edka@itu.dk", "123456");

        int observationsBefore = CountObservationsFor("edka@itu.dk");
        string tooLongText = new string('a', 501);

        await page.Locator("#ObservationText").EvaluateAsync("input => input.removeAttribute('maxlength')");
        await page.Locator("#ObservationText").FillAsync(tooLongText);
        Task<IResponse> postResponse = page.WaitForResponseAsync(page.Url);
        await page.Locator("input[value='Observe']").ClickAsync();
        await postResponse;

        int observationsAfter = CountObservationsFor("edka@itu.dk");
        Assert.Equal(observationsBefore, observationsAfter);
    }

    [Fact]
    public async Task WrongPasswordDoesNotLogTheUserIn()
    {
        IPage page = await app.NewPageAsync();

        await page.GotoAsync("/Identity/Account/Login");
        await page.Locator("#Input_Email").FillAsync("edka@itu.dk");
        await page.Locator("#Input_Password").FillAsync("wrongpassword");
        await page.Locator("#login-submit").ClickAsync();

        await Assertions.Expect(page.GetByText("Invalid login attempt.")).ToBeVisibleAsync();

        await page.GotoAsync("/");
        await Assertions.Expect(page.Locator(".observationbox")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task LoggedOutUserDoesNotSeeTheObservationBox()
    {
        IPage page = await app.NewPageAsync();

        await page.GotoAsync("/");

        await Assertions.Expect(page.Locator(".observationbox")).ToHaveCountAsync(0);
    }

    [Fact]
    public async Task FilteringByTaxonThatDoesNotExistGivesNotFound()
    {
        IPage page = await app.NewPageAsync();

        IResponse? response = await page.GotoAsync("/?taxon=999999");

        Assert.NotNull(response);
        Assert.Equal(404, response.Status);
    }

    [Fact]
    public async Task FrontPageHasNoAccessibilityProblems()
    {
        IPage page = await app.NewPageAsync();

        await page.GotoAsync("/");
        AxeResult result = await page.RunAxe();

        Assert.True(result.Violations.Length == 0, DescribeProblems(result));
    }

    [Fact]
    public async Task FrontPageHasNoAccessibilityProblemsWhenLoggedIn()
    {
        IPage page = await app.NewPageAsync();

        await LogIn(page, "edka@itu.dk", "123456");
        AxeResult result = await page.RunAxe();

        Assert.True(result.Violations.Length == 0, DescribeProblems(result));
    }

    private async Task LogIn(IPage page, string email, string password)
    {
        await page.GotoAsync("/Identity/Account/Login");
        await page.Locator("#Input_Email").FillAsync(email);
        await page.Locator("#Input_Password").FillAsync(password);
        await page.Locator("#login-submit").ClickAsync();

        await Assertions.Expect(page.Locator(".observationbox")).ToBeVisibleAsync();
    }

    private int CountObservationsFor(string email)
    {
        using (BisonDBContext database = app.CreateDbContext())
        {
            Author author = database.Authors.Single(author => author.Email == email);
            int count = database.Observations.Count(observation => observation.AuthorId == author.AuthorId);
            return count;
        }
    }

    private string DescribeProblems(AxeResult result)
    {
        string problems = "Axe found these problems:\n";
        foreach (AxeResultItem violation in result.Violations)
        {
            problems = problems + violation.Id + ": " + violation.Help + "\n";
        }
        return problems;
    }
}
