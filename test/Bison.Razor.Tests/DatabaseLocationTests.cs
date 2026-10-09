namespace Bison.Razor.Tests;

//all tests that change BISONDBPATH have to be in this collection. The environment variable is shared
//by every test, and xUnit doesnt run tests from the same collection at the same time
[CollectionDefinition(Name)]
public class EnvironmentVariableCollection {
    public const string Name = "BISONDBPATH environment variable";
}

[Collection(EnvironmentVariableCollection.Name)]
public class DatabaseLocationTests : IDisposable {
    private readonly string? _originalValue = Environment.GetEnvironmentVariable("BISONDBPATH");

    public void Dispose() {
        Environment.SetEnvironmentVariable("BISONDBPATH", _originalValue);
    }

    [Fact]
    public void DatabasePath_ComesFromBISONDBPATH_WhenItIsSet() {
        Environment.SetEnvironmentVariable("BISONDBPATH", "./mybison.db");

        PostRepository db = new PostRepository();

        Assert.Equal("./mybison.db", db.DatabasePath);
    }

    [Fact]
    public void DatabasePath_IsBisonDbInTempDirectory_WhenBISONDBPATHIsNotSet() {
        Environment.SetEnvironmentVariable("BISONDBPATH", null);

        PostRepository db = new PostRepository();

        Assert.Equal(Path.Combine(Path.GetTempPath(), "bison.db"), db.DatabasePath);
    }
}
