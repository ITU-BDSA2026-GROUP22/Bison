using System.Diagnostics;
using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Playwright;

namespace Bison.Razor.E2ETests;

public class BisonAppFixture : IAsyncLifetime
{
    private string baseUrl = "http://localhost:5299";
    private string databasePath = Path.Combine(Path.GetTempPath(), "bison_e2e_test.db");
    private Process? appProcess;
    private IPlaywright? playwright;
    private IBrowser? browser;

    public async Task InitializeAsync()
    {
        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }

        StartApp();
        await WaitForAppToStart();

        int installResult = Microsoft.Playwright.Program.Main(new string[] { "install", "chromium" });
        if (installResult != 0)
        {
            throw new Exception("Playwright could not install Chromium");
        }

        playwright = await Playwright.CreateAsync();
        browser = await playwright.Chromium.LaunchAsync();
    }

    public async Task DisposeAsync()
    {
        if (browser != null)
        {
            await browser.CloseAsync();
        }

        if (playwright != null)
        {
            playwright.Dispose();
        }

        if (appProcess != null)
        {
            appProcess.Kill(true);
            await appProcess.WaitForExitAsync();
        }

        if (File.Exists(databasePath))
        {
            File.Delete(databasePath);
        }
    }

    public async Task<IPage> NewPageAsync()
    {
        if (browser == null)
        {
            throw new Exception("The browser has not been started");
        }

        BrowserNewContextOptions options = new BrowserNewContextOptions();
        options.BaseURL = baseUrl;

        IBrowserContext browserContext = await browser.NewContextAsync(options);
        IPage page = await browserContext.NewPageAsync();
        return page;
    }

    public BisonDBContext CreateDbContext()
    {
        DbContextOptionsBuilder<BisonDBContext> optionsBuilder = new DbContextOptionsBuilder<BisonDBContext>();
        optionsBuilder.UseSqlite("Data Source=" + databasePath);

        BisonDBContext database = new BisonDBContext(optionsBuilder.Options);
        return database;
    }

    private void StartApp()
    {
        string razorProjectFolder = Path.Combine(FindRepositoryFolder(), "src", "Bison.Razor");

        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "dotnet";
        startInfo.WorkingDirectory = razorProjectFolder;
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add("--no-launch-profile");
        startInfo.ArgumentList.Add("--urls");
        startInfo.ArgumentList.Add(baseUrl);
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["BisonDatabasePath"] = databasePath;

        appProcess = Process.Start(startInfo);
    }

    private async Task WaitForAppToStart()
    {
        HttpClient client = new HttpClient();

        for (int attempt = 0; attempt < 120; attempt++)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(baseUrl);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }

            await Task.Delay(1000);
        }

        throw new Exception("The Bison app did not start within 2 minutes");
    }

    private string FindRepositoryFolder()
    {
        DirectoryInfo? folder = new DirectoryInfo(AppContext.BaseDirectory);

        while (folder != null)
        {
            string solutionFile = Path.Combine(folder.FullName, "Bison.sln");
            if (File.Exists(solutionFile))
            {
                return folder.FullName;
            }

            folder = folder.Parent;
        }

        throw new Exception("Could not find the folder with Bison.sln");
    }
}

[CollectionDefinition("Bison app")]
public class BisonAppCollection : ICollectionFixture<BisonAppFixture>
{
}
