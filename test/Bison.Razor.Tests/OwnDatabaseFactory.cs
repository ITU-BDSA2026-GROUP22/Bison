using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Bison.Razor.Tests;

public class OwnDatabaseFactory : WebApplicationFactory<Program> {
    public string EFDatabasePath { get; } = EFTestDatabase.CreatePath();

    protected override void ConfigureWebHost(IWebHostBuilder builder) {
        builder.ConfigureServices(services => {
            EFTestDatabase.UseDatabase(services, EFDatabasePath);
        });
    }

    protected override void Dispose(bool disposing) {
        base.Dispose(disposing);
        TestDatabase.Delete(EFDatabasePath);
    }
}
