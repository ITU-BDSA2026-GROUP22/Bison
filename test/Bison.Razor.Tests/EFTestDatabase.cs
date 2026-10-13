using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Bison.Razor.Tests;

public static class EFTestDatabase {
    public static string CreatePath() {
        string fileName = "bison_ef_test_" + Guid.NewGuid() + ".db";
        string databasePath = Path.Combine(Path.GetTempPath(), fileName);

        return databasePath;
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
