
using Bison.Razor.Data;
using Bison.Razor.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public class OwnDatabaseFactory : WebApplicationFactory<Program>
{
    public string EFDatabasePath { get; } = EFTestDatabase.CreatePath();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            EFTestDatabase.UseDatabase(services, EFDatabasePath);
        });
    }

    public void SeedPaginationData()
    {
        using IServiceScope scope = Services.CreateScope();

        BisonDBContext db =
            scope.ServiceProvider.GetRequiredService<BisonDBContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();

        Author[] authors =
        {
            new Author { Name = "Alexander", Email = "alexander@example.com" },
            new Author { Name = "Lars", Email = "lars@example.com" },
            new Author { Name = "Sebastian", Email = "sebastian@example.com" },
            new Author { Name = "Tony", Email = "tony@example.com" }
        };

        db.Authors.AddRange(authors);

        Taxon taxon = new Taxon
        {
            dwc_TaxonID = "test-taxon",
            VernacularName = "Test"
        };

        db.Taxons.Add(taxon);
        db.SaveChanges();

        for (int i = 0; i < 70; i++)
        {
            int authorIndex = i < 40 ? 0 :
                              i < 55 ? 1 :
                              i < 65 ? 2 : 3;

            Observation observation = new Observation
            {
                Author = authors[authorIndex],
                Taxon = taxon,
                Text = "Observation number " + i,
                TimeStamp = DateTimeOffset
                    .FromUnixTimeSeconds(1690000000 + i)
                    .DateTime
            };

            db.Observations.Add(observation);
        }

        db.SaveChanges();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        TestDatabase.Delete(EFDatabasePath);
    }
}
