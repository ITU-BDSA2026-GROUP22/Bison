using SimpleDB;

// Test database that keeps records as a list. Records are public so they can be used for tests
public class TestDatabaseRepository<T> : IDatabaseRepository<T>
{
    public List<T> Records { get; } = new List<T>();

    public IEnumerable<T> Read(int? limit = null)
    {
        if (limit == null) {
            return Records;
        } else {
            return Records.Take(limit.Value);
        }
    }

    public void Store(T record)
    {
        Records.Add(record);
    }
}