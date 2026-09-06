using SimpleDB;
using DocoptNet;

string fileName = "bison_observe_cli_db.csv";

IDatabaseRepository<Cheep> database = new CSVDatabase<Cheep>(fileName);

const string usage = @"Bison CLI.
Usage:
    Bison.CLI read
    Bison.CLI observe <message>
";

var arguments = new Docopt().Apply(usage, args, exit: true)!;

if (arguments["read"].IsTrue) {
    read_observations();
} else if (arguments["observe"].IsTrue) {
    string message = arguments["<message>"].ToString();
    add_observation(message);
}

void read_observations() 
{
    IEnumerable<Cheep> cheeps = database.Read();

    UserInterface.PrintObservations(cheeps);
}

void add_observation(string message) {
    Cheep cheep = new Cheep(Environment.UserName, message, DateTimeOffset.UtcNow.ToUnixTimeSeconds());

    database.Store(cheep);

    Console.WriteLine("Observation added.");
}