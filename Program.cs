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

    UserInterface.PrintObservations(cheeps);
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