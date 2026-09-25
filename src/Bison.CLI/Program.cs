using SimpleDB;
using DocoptNet;


BisonService bisonService = new BisonService();


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
    await read_observations(location);

} else if (arguments["observe"].IsTrue) {
    string message = arguments["<message>"].ToString();
    string location = arguments["<location>"].Value?.ToString() ?? "";
    await add_observation(message, location);

} else if (arguments["comment"].IsTrue) {
    string message = arguments["<message>"].ToString();
    if (int.TryParse(arguments["<id>"].ToString(), out int id)) {
        await add_comment(message, id);
    }

} else if (arguments["discussion"].IsTrue) {
    if (int.TryParse(arguments["<id>"].ToString(), out int id)) {
        await show_discussion(id);
    }
}

async Task read_observations(string location)
{
    IEnumerable<Observation> cheeps;
    
    if (location == "")
    {
        cheeps = await bisonService.ReadObservations();
    } else {
        cheeps = await bisonService.ReadObservationsAt(location);
    }
    UserInterface.PrintObservations(cheeps);
}

async Task add_observation(string message, string location) {
    Observation observation = await bisonService.AddObservation(message, location);

    Console.WriteLine($"Observation with ID: {observation.ID} added.");
}

async Task add_comment(string message, int observationID) {
    bool commentWasAdded = await bisonService.AddComment(message, observationID);

    if (commentWasAdded) {
        Console.WriteLine("Comment added.");
    } else {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
    }
}

async Task show_discussion(int observationID) {
    bool observationExists = await bisonService.ObservationExists(observationID);

    if (!observationExists) {
        Console.WriteLine($"Observation with ID: {observationID} does not exist.");
        return;
    }

    IEnumerable<Comment> comments = await bisonService.GetComments(observationID);

    UserInterface.PrintComments(comments);
}