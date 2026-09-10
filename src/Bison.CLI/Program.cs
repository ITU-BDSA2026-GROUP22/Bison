using SimpleDB;
using DocoptNet;

string observationFileName = "bison_observe_cli_db.csv";
string commentFileName = "bison_comment_cli_db.csv";

IDatabaseRepository<Observation> observationDatabase = new CSVDatabase<Observation>(observationFileName);
IDatabaseRepository<Comment> commentDatabase = new CSVDatabase<Comment>(commentFileName);

BisonService bisonService = new BisonService(observationDatabase, commentDatabase);


const string usage = @"Bison CLI.
Usage:
    Bison.CLI read
    Bison.CLI observe <message>
    Bison.CLI comment <message> <id>
    Bison.CLI discussion <id>
";

var arguments = new Docopt().Apply(usage, args, exit: true)!;

if (arguments["read"].IsTrue) {
    read_observations();

} else if (arguments["observe"].IsTrue) {
    string message = arguments["<message>"].ToString();
    add_observation(message);

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

void read_observations()
{
    IEnumerable<Observation> cheeps = bisonService.ReadObservations();

    UserInterface.PrintObservations(cheeps);
}

void add_observation(string message) {
    Observation observation = bisonService.AddObservation(message);

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