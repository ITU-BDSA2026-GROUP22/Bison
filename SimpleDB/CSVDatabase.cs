using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace SimpleDB;

public sealed class CSVDatabase<T> : IDatabaseRepository<T>
{
    private readonly string _fileName;

    public CSVDatabase(string fileName)
    {
        this._fileName = fileName;
    }

    public IEnumerable<T> Read(int? limit = null)
    {
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
        using (StreamWriter writer = new StreamWriter(_fileName, append: true))
        using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
        {
            csv.WriteRecord(record);
            csv.NextRecord();
        }
    }
}