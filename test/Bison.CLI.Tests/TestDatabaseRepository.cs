using SimpleDB;

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