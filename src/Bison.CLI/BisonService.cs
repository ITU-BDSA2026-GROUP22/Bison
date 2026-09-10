using SimpleDB;

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

    public Observation AddObservation(string message)
    {
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
