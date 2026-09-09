using SimpleDB;
using DocoptNet;

string observationFileName = "bison_observe_cli_db.csv";
string commentFileName = "bison_comment_cli_db.csv";

IDatabaseRepository<Observation> observationDatabase = new CSVDatabase<Observation>(observationFileName);
IDatabaseRepository<Comment> commentDatabase = new CSVDatabase<Comment>(commentFileName);


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
    IEnumerable<Observation> cheeps = observationDatabase.Read();

    UserInterface.PrintObservations(cheeps);
}

void add_observation(string message) {
    IEnumerable<Observation> observations = observationDatabase.Read();
    
    int nextID;
   
    if (observations.Any()) {
        nextID = observations.Max(observation => observation.ID) + 1;
    } else {
        nextID = 1;
    }

    Observation observation = new Observation(
        nextID,
        Environment.UserName, 
        message, 
        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    );

    observationDatabase.Store(observation);
    Console.WriteLine($"Observation with ID: {nextID} added.");
}

void add_comment(string message, int observationID) {
    IEnumerable<Observation> observations = observationDatabase.Read();

    bool observationExists = observations.Any(observation => observation.ID == observationID);

    if (!observationExists) {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
        return;
    }

    Comment comment = new Comment(
        observationID,
        Environment.UserName, 
        message, 
        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    );

    commentDatabase.Store(comment);
    Console.WriteLine("Comment added.");
}

void show_discussion(int observationID) {
    IEnumerable<Observation> observations = observationDatabase.Read();
    bool observationExists = observations.Any(observation => observation.ID == observationID);

    if (!observationExists) {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
        return;
    }

    IEnumerable<Comment> comments = commentDatabase.Read().Where(comment => comment.ObservationID == observationID);

    UserInterface.PrintComments(comments);
}