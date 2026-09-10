using SimpleDB;

namespace SimpleDB.Tests;

public record TestRecord(int Id, string Name);

public class CSVDatabaseTests
{
    [Fact]
    public void Store_ThenRead_ReturnsStoredRecord()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            CSVDatabase<TestRecord> database = new CSVDatabase<TestRecord>(tempFile);

            TestRecord record = new TestRecord(1, "Bob");
            database.Store(record);

            IEnumerable<TestRecord> result = database.Read();

            TestRecord retrieved = Assert.Single(result);
            Assert.Equal(record, retrieved);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void Store_MultipleRecords_AllComeBackOnRead()
    {
        string tempFile = Path.GetTempFileName();
        try
        {
            CSVDatabase<TestRecord> database = new CSVDatabase<TestRecord>(tempFile);

            TestRecord first = new TestRecord(1, "Bob");
            TestRecord second = new TestRecord(2, "Carl");
            database.Store(first);
            database.Store(second);

            List<TestRecord> result = database.Read().ToList();

            Assert.Equal(2, result.Count);
            Assert.Contains(first, result);
            Assert.Contains(second, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
