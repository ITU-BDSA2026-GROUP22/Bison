using SimpleDB;

public interface IPostRepository
{
    int PageSize { get; }

    List<Observation> GetObservations(int page = 1 );

    List<Observation> GetObservationsFromAuthor(string author, int page = 1);

    Observation? GetObservation(int observationID);

    int countObservations();

    int countObservationsFromAuthor(string author);
}