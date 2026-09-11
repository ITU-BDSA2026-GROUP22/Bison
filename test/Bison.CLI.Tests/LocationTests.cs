using SimpleDB;

public class LocationTests
{
    // Unit testing (this only tests the filtering in bisonService "-see slide 18")

    [Fact]
    public void ReadObservationsAtOnlyReturnsMatchingLocation()
    {
        // use fake database instead of our csv file, since bisonservice takes the interface -slide 11
        var service = new BisonService(new FakeDatabase<Observation>(),
            new FakeDatabase<Comment>());
        service.AddObservation("Heron", "Copenhagen");
        service.AddObservation("Penguin", "Oelstykke");
        
        // testing lowercase to see if case sensetivity fix slide 35 regression test
        var result = service.ReadObservationsAt("copenhagen");
        
        // real test for heron
        var observation = Assert.Single(result);
        Assert.Equal("Heron", observation.Message);
    }
    
    // Integration test (checks real CSVDatabase slide 19)
    [Fact]
    public void StoredLocationCanBeRetrieved()
    {
        // make temporary file so not to fuck with real data
        string file = Path.GetTempFileName();
        var database = new CSVDatabase<Observation>(file);
        var observation = new Observation(1, "Tony", "Heron",0, "Copenhagen");
        
        database.Store(observation);
        var result = database.Read().Single();
        
        // this test compares all fields and checks
        Assert.Equal(observation, result);
        
        File.Delete(file);
    }
}

// keeps everything in a list instead of a file
class FakeDatabase<T> : IDatabaseRepository<T>
{
    private readonly List<T> records = new();
    
    public IEnumerable<T> Read(int? limit) => records;
    public void Store(T record) => records.Add(record); 
}