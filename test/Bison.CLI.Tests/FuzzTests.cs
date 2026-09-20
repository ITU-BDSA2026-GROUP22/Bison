using System.Net;
using System.Net.Http.Json;
using SimpleDB;

public class FuzzE2ETests
{
    private const string BaseUrl = "http://localhost:5257";

    [Fact]
    public async Task FuzzTest_ObservationsCommentsAndProposals()
    {
        using HttpClient client = new HttpClient
        {
            BaseAddress = new Uri(BaseUrl)
        };

        Random random = new Random(12345);

        // Keep track of what we expect to be in the database

        List<Observation> expectedObservations =
            (await client.GetFromJsonAsync<List<Observation>>("observations"))
            ?? new List<Observation>();

        List<Comment> expectedComments =
            (await client.GetFromJsonAsync<List<Comment>>("comments"))
            ?? new List<Comment>();

        // FUZZ OBSERVATIONS

        for (int i = 0; i < 20; i++)
        {
            NewObservationRequest observationRequest =
                new NewObservationRequest(
                    RandomString(random),
                    RandomString(random),
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    RandomString(random)
                );

            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "observation",
                    observationRequest
                );

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            Observation? createdObservation =
                await response.Content.ReadFromJsonAsync<Observation>();

            Assert.NotNull(createdObservation);

            // The server generated the ID, so we use
            // the returned observation in our oracle.
            expectedObservations.Add(createdObservation!);
        }

        // FUZZ COMMENTS

        for (int i = 0; i < 20; i++)
        {
            // Pick an observation that we know exists.
            Observation observation =
                expectedObservations[
                    random.Next(expectedObservations.Count)
                ];

            Comment comment =
                new Comment(
                    observation.ID,
                    RandomString(random),
                    RandomString(random),
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );

            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "comment",
                    comment
                );

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // The comment was successfully stored,
            // so add it to our expected result.
            expectedComments.Add(comment);
        }

        // FUZZ PROPOSALS

        for (int i = 0; i < 20; i++)
        {
            Observation observation =
                expectedObservations[
                    random.Next(expectedObservations.Count)
                ];

            Proposal proposal =
                new Proposal(
                    observation.ID,
                    RandomString(random),
                    RandomTaxonId(random),
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                );

            HttpResponseMessage response =
                await client.PostAsJsonAsync(
                    "proposal",
                    proposal
                );

            // A proposal can either succeed or fail because
            // the randomly generated TaxonId may not exist.
            Assert.True(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        // GET /observations

        List<Observation> actualObservations =
            (await client.GetFromJsonAsync<List<Observation>>(
                "observations"
            )) ?? new List<Observation>();

        foreach (Observation expected in expectedObservations)
        {
            Assert.Contains(
                actualObservations,
                actual =>
                    actual.ID == expected.ID &&
                    actual.Author == expected.Author &&
                    actual.Message == expected.Message &&
                    actual.Timestamp == expected.Timestamp &&
                    actual.Location == expected.Location
            );
        }


        // GET /comments

        List<Comment> actualComments =
            (await client.GetFromJsonAsync<List<Comment>>(
                "comments"
            )) ?? new List<Comment>();

        foreach (Comment expected in expectedComments)
        {
            Assert.Contains(
                actualComments,
                actual =>
                    actual.ObservationID == expected.ObservationID &&
                    actual.Author == expected.Author &&
                    actual.Message == expected.Message &&
                    actual.Timestamp == expected.Timestamp
            );
        }
    }

    // Generate random strings

    private static string RandomString(Random random)
    {
        const string characters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "abcdefghijklmnopqrstuvwxyz" +
            "0123456789";

        int length = random.Next(5, 30);

        return new string(
            Enumerable
                .Range(0, length)
                .Select(_ => characters[random.Next(characters.Length)])
                .ToArray()
        );
    }

    // Generate random taxon IDs

    private static string RandomTaxonId(Random random)
    {
        string[] taxonIds =
        {
            "1",
            "2",
            "3",
            "4",
            "5"
        };

        return taxonIds[random.Next(taxonIds.Length)];
    }

    // Local request/response models used by the E2E test

    private record NewObservationRequest(
        string Author,
        string Message,
        long Timestamp,
        string Location
    );

    private record Proposal(
        int ObservationId,
        string Author,
        string TaxonId,
        long Timestamp
    );
}