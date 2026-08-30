using System.Text.RegularExpressions;

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

void read_observations() {
    Regex CSVParser = new Regex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

    using (StreamReader reader = new StreamReader(fileName)) {
        string? line;

        reader.ReadLine();

        while ((line = reader.ReadLine()) != null) {
            string[] X = CSVParser.Split(line);

            string author = X[0];
            string observation = X[1].Trim('"');

            long timestamp = long.Parse(X[2]);

            DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(timestamp).ToLocalTime();

            Console.WriteLine($"{author} @ {date:MM/dd/yy HH:mm:ss}: {observation}");
        }
    }
}

void add_observation() {
    string observation = args[1];
    string author = Environment.UserName;
    long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        observation = observation.Replace("\"", "\"\"");

        using (StreamWriter writer = File.AppendText(fileName)) {
            writer.WriteLine($"{author},\"{observation}\",{timestamp}");
        }

        Console.WriteLine("Observation added.");
}