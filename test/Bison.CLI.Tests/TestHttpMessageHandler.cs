using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SimpleDB;

public class TestHttpMessageHandler : HttpMessageHandler
{
    public List<Observation> Observations { get; } = new();
    public List<Comment> Comments { get; } = new();

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string path = request.RequestUri!.AbsolutePath.Trim('/');

        if (request.Method == HttpMethod.Get)
        {
            return path switch
            {
                "observations" => JsonResponse(Observations),
                "comments" => JsonResponse(Comments),
                _ => new HttpResponseMessage(HttpStatusCode.NotFound)
            };
        }

        if (request.Method == HttpMethod.Post)
        {
            string body = await request.Content!.ReadAsStringAsync(cancellationToken);

            switch (path)
            {
                case "observation":
                    Observations.Add(JsonSerializer.Deserialize<Observation>(body, JsonOptions)!);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                case "comment":
                    Comments.Add(JsonSerializer.Deserialize<Comment>(body, JsonOptions)!);
                    return new HttpResponseMessage(HttpStatusCode.OK);
                default:
                    return new HttpResponseMessage(HttpStatusCode.NotFound);
            }
        }

        return new HttpResponseMessage(HttpStatusCode.MethodNotAllowed);
    }

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private static HttpResponseMessage JsonResponse<T>(T value) =>
        new(HttpStatusCode.OK) { Content = JsonContent.Create(value) };

    public HttpClient CreateClient() =>
        new(this) { BaseAddress = new Uri("http://localhost") };
}