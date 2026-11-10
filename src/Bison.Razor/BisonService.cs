using SimpleDB;

public record ObservationViewModel(
    int ID,
    string Author,
    string Message,
    string Timestamp,
    int? PictureId = null);

public interface IObservationService
{
    int PageSize { get; }

    List<ObservationViewModel> GetObservations(int page = 1);

    List<ObservationViewModel> GetObservationsFromAuthor(
        string author, int page = 1);

    int CountObservations();

    int CountObservationsFromAuthor(string author);

    void CreateObservation(int authorId, string text);
}

public class ObservationService : IObservationService
{
    private readonly IPostRepository _db;

    public ObservationService(IPostRepository db)
    {
        _db = db;
    }

    public int PageSize
    {
        get
        {
            return _db.PageSize;
        }
    }

    public List<ObservationViewModel> GetObservations(int page = 1)
    {
        List<Observation> observations = _db.GetObservations(page);

        return ConvertToViewModels(observations);
    }

    public List<ObservationViewModel> GetObservationsFromAuthor(
        string author, int page = 1)
    {
        List<Observation> observations =
            _db.GetObservationsFromAuthor(author, page);

        return ConvertToViewModels(observations);
    }

    public int CountObservations()
    {
        return _db.CountObservations();
    }

    public int CountObservationsFromAuthor(string author)
    {
        return _db.CountObservationsFromAuthor(author);
    }

    private static List<ObservationViewModel> ConvertToViewModels(
        List<Observation> observations)
    {
        List<ObservationViewModel> viewModels =
            new List<ObservationViewModel>();

        foreach (Observation observation in observations)
        {
            string timestamp =
                UnixTimeStampToDateTimeString(observation.Timestamp);

            ObservationViewModel viewModel = new ObservationViewModel(
                observation.ID,
                observation.Author,
                observation.Message,
                timestamp);

            viewModels.Add(viewModel);
        }

        return viewModels;
    }

    private static string UnixTimeStampToDateTimeString(
        double unixTimeStamp)
    {
        DateTime dateTime = new DateTime(
            1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        dateTime = dateTime.AddSeconds(unixTimeStamp);

        return dateTime.ToString("MM/dd/yy H:mm:ss");
    }

    public void CreateObservation(int authorId, string text)
    {
        _db.CreateObservation(authorId, text);
    }

}