using Bison.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public static class EFTestDatabase {
    public const string PetersObservation = "A big gray bird in a pond at DR byen";
    public const string PetrasObservation = "A heron";
    public const string EduardsObservation = "Two swans at Peblinge Lake";

    public static string CreatePath() {
        string fileName = "bison_ef_test_" + Guid.NewGuid() + ".db";
        string databasePath = Path.Combine(Path.GetTempPath(), fileName);

        return databasePath;
    }

    public static void Delete(string databasePath) {
        // the connection pool keeps the file open so that it cant be deleted on windows without first clearing it
        SqliteConnection.ClearAllPools();
        File.Delete(databasePath);
    }

    public static void UseDatabase(IServiceCollection services, string databasePath) {
        List<ServiceDescriptor> oldRegistrations = new List<ServiceDescriptor>();

        foreach (ServiceDescriptor registration in services) {
            if (registration.ServiceType == typeof(DbContextOptions<BisonDBContext>)) {
                oldRegistrations.Add(registration);
            }
        }

        foreach (ServiceDescriptor oldRegistration in oldRegistrations) {
            services.Remove(oldRegistration);
        }

        string connectionString = "Data Source=" + databasePath;

        services.AddDbContext<BisonDBContext>(options => {
            options.UseSqlite(connectionString);
        });
    }
}
