using SimpleDB;

public interface IPostRepository
{
    int PageSize { get; }

    List<Observation> GetObservations(int page = 1);

    List<Observation> GetObservationsFromAuthor(string author, int page = 1);

    Observation? GetObservation(int id);

    int CountObservations();

    int CountObservationsFromAuthor(string author);
}