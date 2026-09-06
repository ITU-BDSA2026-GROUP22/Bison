using SimpleDB;

string fileName = "bison_observe_cli_db.csv";

IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(fileName);

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
    IEnumerable<Cheep> cheeps = database.Read();

    foreach (Cheep cheep in cheeps) 
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(cheep.Timestamp).ToLocalTime();

        //Whn u write $ in front of string it make the string "Interpolated String" 
        //which makes it possible to put variables inside the strng using {...}
        Console.WriteLine($"{cheep.Author} @ {date:MM/dd/yy HH:mm:ss}: {cheep.Message}");
    }
}

void add_observation() {
    if (args.Length < 2) {
        Console.WriteLine("Missing second argument. Please write an observation.");
        return;
    }
    Cheep cheep = new Cheep(Environment.UserName, args[1], DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    database.Store(cheep);

    Console.WriteLine("Observation added.");
}