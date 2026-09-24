using SimpleDB;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
public class BisonService
{

    private readonly HttpClient client;

    public BisonService()
    {
        client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5257");

        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    }

    public async Task<IEnumerable<Observation>> ReadObservations()
    {
        var observations = await client.GetFromJsonAsync<List<Observation>>("observations");
        return observations ?? new List<Observation>();
    }

    public async Task<IEnumerable<Observation>> ReadObservationsFromAuthor(string author)
{
    var observations = await client.GetFromJsonAsync<List<Observation>>(
        $"observations/{author}"
    );

    return observations ?? new List<Observation>();
}

    public async Task<IEnumerable<Observation>> ReadObservationsAt(string location)
    {
        var observations = await client.GetFromJsonAsync<List<Observation>>("observations");
        return observations?.Where(observation => string.Equals(observation.Location, location, StringComparison.OrdinalIgnoreCase)) ?? new List<Observation>();
    }
    
    public async Task<Observation> AddObservation(string message, string location = "")
    {
        NewObservationRequest request = new NewObservationRequest(
        Environment.UserName,
        message,
        DateTimeOffset.UtcNow.ToUnixTimeSeconds()
    );

        HttpResponseMessage response = await client.PostAsJsonAsync("observation", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Observation>();
    }

    public async Task<bool> ObservationExists(int observationID)
    {
        IEnumerable<Observation> observations = await ReadObservations();
        return observations.Any(observation => observation.ID == observationID);
    }

    public async Task<bool> AddComment(string message, int observationID)
    {
        if (!await ObservationExists(observationID)) {
            return false;
        }

        Comment comment = new Comment(
            observationID,
            Environment.UserName,
            message,
            DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        );

        HttpResponseMessage response = await client.PostAsJsonAsync("comment", comment);
        response.EnsureSuccessStatusCode();

        return response.IsSuccessStatusCode;
    }

    public async Task<IEnumerable<Comment>> GetComments(int observationID)
    {   
        var comments = await client.GetFromJsonAsync<List<Comment>>("comments");
        return comments?.Where(comment => comment.ObservationID == observationID) ?? new List<Comment>();
    }
}
