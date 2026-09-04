using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

string fileName = "bison_observe_cli_db.csv";


if (args.Length == 0) {
    Console.WriteLine("Invalid command");
    return;
}

string command = args[0];
if (command == "read") {
    read_observations(); 
} else if (command == "observe") {
    add_observation();
} else {
    Console.WriteLine("Invalid command");
}

void read_observations() 
{
    using (StreamReader reader = new StreamReader(fileName)) 
    using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        IEnumerable<Cheep> cheeps = csv.GetRecords<Cheep>();

        foreach (Cheep cheep in cheeps) 
        {
            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

            //Whn u write $ in front of string it make the string "Interpolated String" 
            //which makes it possible to put variables inside the strng using {...}
            Console.WriteLine($"{cheep.Author} @ {date:MM/dd/yy HH:mm:ss}: {cheep.Message}");
        }

    }
}

void add_observation() {
    Cheep cheep = new Cheep(Environment.UserName, args[1], DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    using (StreamWriter writer = new StreamWriter(fileName, append: true))
    using (CsvWriter csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
    {
        csv.WriteRecord(cheep);
        csv.NextRecord();
    }

    Console.WriteLine("Observation added.");
}