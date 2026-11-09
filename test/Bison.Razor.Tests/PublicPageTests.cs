using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Pages;
using Bison.Razor.Data;
using Bison.Razor.Models;

namespace Bison.Razor.Tests;

public class PublicPageTests
{
    private readonly BisonDBContext _db;

    public PublicPageTests()
    {
        string databasePath = EFTestDatabase.CreatePath();

        var options = new DbContextOptionsBuilder<BisonDBContext>()
            .UseSqlite($"Data Source={databasePath}")
            .Options;

        _db = new BisonDBContext(options);

        _db.Database.EnsureCreated();

        _db.Authors.Add(new Author
        {
            Name = "Peter",
            Email = "peter@itu.dk"
        });

        _db.Taxons.Add(new Taxon
        {
            TaxonId = 1,
            dwc_TaxonID = "test-taxon",
            VernacularName = "Test taxon"
        });

        _db.SaveChanges();
    }


    [Fact]    
    public async Task OnPostAsync_CreatesObservation()
    {
        // Arrange
        PublicModel page = new PublicModel(
        new ObservationService(new PostRepository(":memory:")),
        _db,
        new TaxonFilterService());

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, "peter@itu.dk")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        page.PageContext = new PageContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(identity)
            }
        };

        page.ObservationText = "A new test observation";
        page.TaxonId = 1;

        // Act
        var result = await page.OnPostAsync();

        // Assert
        Assert.IsType<RedirectToPageResult>(result);

        Observation observation = await _db.Observations.SingleAsync(o => o.Text == "A new test observation");

        Author author = await _db.Authors.SingleAsync(a => a.Email == "peter@itu.dk");
        Assert.Equal(author.AuthorId, observation.AuthorId);
        Assert.Equal(1, observation.TaxonId);
    }

    
    
[Fact]
public async Task OnPostAsync_EmptyTextDoesNotCreateObservation()
{
    // Arrange
    PublicModel page = new PublicModel(
        new ObservationService(new PostRepository()),
        _db,
        new TaxonFilterService());

    page.ObservationText = "";

    page.PageContext = new PageContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[] { new Claim(ClaimTypes.Name, "peter@itu.dk") },
                    "Test"))
        }
    };

    // Simulate validation failure
    page.ModelState.AddModelError( nameof(page.ObservationText), "Observation text is required.");

    // Act
    var result = await page.OnPostAsync();

    // Assert
    Assert.IsType<PageResult>(result);
    Assert.Empty(await _db.Observations.ToListAsync());
}


}