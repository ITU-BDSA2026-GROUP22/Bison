using System.Diagnostics;
using System.Net.Sockets;

public class EndToEndTests
{
    private static string GetRepositoryRoot()
    {
        DirectoryInfo? directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory != null && !Directory.Exists(Path.Combine(directory.FullName, "src")))
        {
            directory = directory.Parent;
        }

        if (directory == null)
        {
            throw new InvalidOperationException("Could not locate repository root from test output directory.");
        }

        return directory.FullName;
    }

#if DEBUG
    private const string Config = "Debug";
#else
    private const string Config = "Release";
#endif

    private static string GetDllPath(string projectName) =>
        Path.Combine(GetRepositoryRoot(), "src", projectName, "bin", Config, "net8.0", $"{projectName}.dll");

    private static int GetFreePort()
    {
        TcpListener listener = new TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        int port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static Process StartService(string workingDirectory, string url)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(GetDllPath("Bison.CSVDBService"));
        startInfo.Environment["ASPNETCORE_URLS"] = url;

        Process process = Process.Start(startInfo)!;
        WaitUntilReady(url);
        return process;
    }

    private static void WaitUntilReady(string url)
    {
        using HttpClient client = new() { Timeout = TimeSpan.FromSeconds(1) };
        DateTime deadline = DateTime.UtcNow.AddSeconds(30);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                client.GetAsync($"{url}/observations").GetAwaiter().GetResult();
                return;
            }
            catch
            {
                Thread.Sleep(100);
            }
        }

        throw new InvalidOperationException($"Service did not become ready at {url}.");
    }

    private static string RunCli(string workingDirectory, string serviceUrl, params string[] args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(GetDllPath("Bison.CLI"));
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        startInfo.Environment["BISON_SERVICE_URL"] = serviceUrl;

        using Process process = Process.Start(startInfo)!;
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    [Fact]
    public void Read_WithSeededData_PrintsObservation()
    {
        // Tests need to start with empty databases. The CLI saves its CSVs in whichever folder it runs from,
        // so running it in a temp folder makes sure it doesn't mess up our actual databases
        DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
        string url = $"http://localhost:{GetFreePort()}";
        Process? service = null;

        try
        {
            string dbFile = Path.Combine(tempDirectory.FullName, "bison_observe_cli_db.csv");
            SimpleDB.CSVDatabase<Observation> seedDatabase = new SimpleDB.CSVDatabase<Observation>(dbFile);
            seedDatabase.Store(new Observation(1, "Ted", "Who even reads these test messages", 6969));

            service = StartService(tempDirectory.FullName, url);

            string output = RunCli(tempDirectory.FullName, url, "read");

            Assert.Contains("ID: 1", output);
            Assert.Contains("Ted", output);
            Assert.Contains("Who even reads these test messages", output);
        }
        finally
        {
            if (service is { HasExited: false })
            {
                service.Kill(entireProcessTree: true);
                service.WaitForExit();
            }
            service?.Dispose();
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }

    [Fact]
    public void Observe_Penguin_StoresObservationInDatabase()
    {
        DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
        string url = $"http://localhost:{GetFreePort()}";
        Process? service = null;

        try
        {
            service = StartService(tempDirectory.FullName, url);

            RunCli(tempDirectory.FullName, url, "observe", "Penguin");

            string dbFile = Path.Combine(tempDirectory.FullName, "bison_observe_cli_db.csv");
            SimpleDB.CSVDatabase<Observation> database = new SimpleDB.CSVDatabase<Observation>(dbFile);
            Observation stored = Assert.Single(database.Read());

            Assert.Equal("Penguin", stored.Message);
        }
        finally
        {
            if (service is { HasExited: false })
            {
                service.Kill(entireProcessTree: true);
                service.WaitForExit();
            }
            service?.Dispose();
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }
}