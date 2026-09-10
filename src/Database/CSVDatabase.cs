using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _fileName;

    private static CSVDatabase<T>? _instance;

    private CSVDatabase(string fileName)
    {
        _fileName = fileName;
    }

    public static CSVDatabase<T> Instance(string fileName)
    {
        if (_instance == null)
        {
            _instance = new CSVDatabase<T>(fileName);
        }

        return _instance;
    }

    public IEnumerable<T> Read(int? limit = null)
{
    if (!File.Exists(_fileName) ||
        new FileInfo(_fileName).Length == 0)
    {
        return Enumerable.Empty<T>();
    }

    using (StreamReader reader = new StreamReader(_fileName))
    using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        IEnumerable<T> records = csv.GetRecords<T>().ToList();

        if (limit != null)
        {
            records = records.Take(limit.Value);
        }

        return records;
    }
}

    public void Store(T record)
{
    bool fileIsNew = !File.Exists(_fileName) || new FileInfo(_fileName).Length == 0;

    using (StreamWriter writer = new StreamWriter(_fileName, append: true))
    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        if (fileIsNew)
        {
            csv.WriteHeader<T>();
            csv.NextRecord();
        }

        csv.WriteRecord(record);
        csv.NextRecord();
    }
}
}