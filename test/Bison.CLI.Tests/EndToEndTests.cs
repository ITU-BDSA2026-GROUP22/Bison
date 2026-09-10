using System.Diagnostics;

public class EndToEndTests
{
    private static string GetCliDllPath()
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

#if DEBUG
        string config = "Debug";
#else
        string config = "Release";
#endif

        return Path.Combine(directory.FullName, "src", "Bison.CLI", "bin", config, "net8.0", "Bison.CLI.dll");
    }

    private static string RunCli(string workingDirectory, params string[] args)
    {
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            UseShellExecute = false,
        };

        startInfo.ArgumentList.Add(GetCliDllPath());
        foreach (string arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        using Process process = Process.Start(startInfo)!;
        string output = process.StandardOutput.ReadToEnd();
        process.WaitForExit();

        return output;
    }

    [Fact]
    public void Read_WithSeededData_PrintsObservation()
    {
        DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            string dbFile = Path.Combine(tempDirectory.FullName, "bison_observe_cli_db.csv");
            SimpleDB.CSVDatabase<Observation> seedDatabase = new SimpleDB.CSVDatabase<Observation>(dbFile);
            seedDatabase.Store(new Observation(1, "Ted", "Who even reads these test messages", 6969));

            string output = RunCli(tempDirectory.FullName, "read");

            Assert.Contains("ID: 1", output);
            Assert.Contains("Ted", output);
            Assert.Contains("Who even reads these test messages", output);
        }
        finally
        {
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }

    [Fact]
    public void Observe_Penguin_StoresObservationInDatabase()
    {
        DirectoryInfo tempDirectory = Directory.CreateTempSubdirectory();
        try
        {
            RunCli(tempDirectory.FullName, "observe", "Penguin");

            string dbFile = Path.Combine(tempDirectory.FullName, "bison_observe_cli_db.csv");
            SimpleDB.CSVDatabase<Observation> database = new SimpleDB.CSVDatabase<Observation>(dbFile);
            Observation stored = Assert.Single(database.Read());

            Assert.Equal("Penguin", stored.Message);
        }
        finally
        {
            Directory.Delete(tempDirectory.FullName, recursive: true);
        }
    }
}
