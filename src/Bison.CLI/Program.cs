using SimpleDB;
using DocoptNet;

string observationFileName = "bison_observe_cli_db.csv";
string commentFileName = "bison_comment_cli_db.csv";

IDatabaseRepository<Observation> observationDatabase = new CSVDatabase<Observation>(observationFileName);
IDatabaseRepository<Comment> commentDatabase = new CSVDatabase<Comment>(commentFileName);

BisonService bisonService = new BisonService(observationDatabase, commentDatabase);


const string usage = @"Bison CLI.
Usage:
    Bison.CLI read [<location>]
    Bison.CLI observe <message> [<location>]
    Bison.CLI comment <message> <id>
    Bison.CLI discussion <id>
";

var arguments = new Docopt().Apply(usage, args, exit: true)!;

if (arguments["read"].IsTrue) {
    string location = arguments["<location>"].Value?.ToString() ?? "";
    read_observations(location);

} else if (arguments["observe"].IsTrue) {
    string message = arguments["<message>"].ToString();
    string location = arguments["<location>"].Value?.ToString() ?? "";
    add_observation(message, location);

} else if (arguments["comment"].IsTrue) {
    string message = arguments["<message>"].ToString();
    if (int.TryParse(arguments["<id>"].ToString(), out int id)) {
        add_comment(message, id);
    }

} else if (arguments["discussion"].IsTrue) {
    if (int.TryParse(arguments["<id>"].ToString(), out int id)) {
        show_discussion(id);
    }
}

void read_observations(string location)
{
    IEnumerable<Observation> cheeps = bisonService.ReadObservations();
    
    if (location == "")
    {
        cheeps = bisonService.ReadObservations();
    } else {
        cheeps = bisonService.ReadObservationsAt(location);
    }
    UserInterface.PrintObservations(cheeps);
}

void add_observation(string message, string location) {
    Observation observation = bisonService.AddObservation(message, location);

    Console.WriteLine($"Observation with ID: {observation.ID} added.");
}

void add_comment(string message, int observationID) {
    bool commentWasAdded = bisonService.AddComment(message, observationID);

    if (commentWasAdded) {
        Console.WriteLine("Comment added.");
    } else {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
    }
}

void show_discussion(int observationID) {
    bool observationExists = bisonService.ObservationExists(observationID);

    if (!observationExists) {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
        return;
    }

    IEnumerable<Comment> comments = bisonService.GetComments(observationID);

    UserInterface.PrintComments(comments);
}