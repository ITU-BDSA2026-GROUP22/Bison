using SimpleDB;

// Holds the CLI's logic and methods. Has database as parameter so it can be used for fake test databases
public class BisonService
{
    private readonly IDatabaseRepository<Observation> observationDatabase;
    private readonly IDatabaseRepository<Comment> commentDatabase;

    public BisonService(IDatabaseRepository<Observation> observationDatabase, IDatabaseRepository<Comment> commentDatabase)
    {
        this.observationDatabase = observationDatabase;
        this.commentDatabase = commentDatabase;
    }

    public IEnumerable<Observation> ReadObservations()
    {
        return observationDatabase.Read();
    }

    public IEnumerable<Observation> ReadObservationsAt(string location)
    {
        return observationDatabase.Read().Where(observation => string.Equals(observation.Location, location, StringComparison.OrdinalIgnoreCase));
    }
    
    public Observation AddObservation(string message, string location = "")
    {
        IEnumerable<Observation> observations = observationDatabase.Read();

        int nextID;

        var enumerable = observations as Observation[] ?? observations.ToArray();
        if (enumerable.Any()) {
            nextID = enumerable.Max(observation => observation.ID) + 1;
        } else {
            nextID = 1;
        }

        Observation observation = new Observation(
            nextID,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            location
        );

        observationDatabase.Store(observation);

        return observation;
    }

    public bool ObservationExists(int observationID)
    {
        IEnumerable<Observation> observations = observationDatabase.Read();
        return observations.Any(observation => observation.ID == observationID);
    }

    public bool AddComment(string message, int observationID)
    {
        if (!ObservationExists(observationID)) {
            return false;
        }

        Comment comment = new Comment(
            observationID,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        commentDatabase.Store(comment);

        return true;
    }

    public IEnumerable<Comment> GetComments(int observationID)
    {
        IEnumerable<Comment> comments = commentDatabase.Read();
        return comments.Where(comment => comment.ObservationID == observationID);
    }
}
