using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using SimpleDB;

public class ProposalFuzzTests : IDisposable
{
    // Arbitrarily chosen Taxon IDs. This is the pool of IDs that our fuzz tests will pick from
    private static readonly List<string> ValidTaxonIds = new List<string>
    {
        "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:495067e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:a15367e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:025767e4-f785-ea11-aa77-501ac539d1ea",
        "MSTSNM:Arter:2a5767e4-f785-ea11-aa77-501ac539d1ea"
    };

    private const long FixedTimestamp = 1700000000;
    private const int NumberOfValidObservationsToCreate = 5;
    private const int NumberOfFuzzIterations = 500;

    // Separate file names so they don't get mixed up with ProposalEndpointTests. Allows xUnit to safely run them in parallel
    private const string ObservationsFileName = "proposalFuzzTests_observations.csv";
    private const string ProposalsFileName = "proposalFuzzTests_proposals.csv";

    private readonly WebApplicationFactory<Program> webApplicationFactory;
    private readonly HttpClient httpClient;
    private readonly Random random;

    public ProposalFuzzTests()
    {
        DeleteFileIfItExists(ObservationsFileName);
        DeleteFileIfItExists(ProposalsFileName);

        // Replaces the default databases with the files used for the tests
        webApplicationFactory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(webHostBuilder =>
            {
                webHostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton<IDatabaseRepository<Observation>>(new CSVDatabase<Observation>(ObservationsFileName));
                    services.AddSingleton<IDatabaseRepository<Proposal>>(new CSVDatabase<Proposal>(ProposalsFileName));
                });
            });

        httpClient = webApplicationFactory.CreateClient();

        // Using fixed seed so failures are reproducible
        random = new Random(88888888);
    }

    private void DeleteFileIfItExists(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }

    public void Dispose()
    {
        httpClient.Dispose();
        webApplicationFactory.Dispose();
    }

    private async Task<List<int>> CreateValidObservationsAsync(int numberOfObservationsToCreate)
    {
        List<int> validObservationIds = new List<int>();

        for (int i = 0; i < numberOfObservationsToCreate; i++)
        {
            NewObservationRequest newObservationRequest = new NewObservationRequest(
                "FuzzTestAuthor",
                "This observation exists only so the fuzz tests have valid ObservationIds to use.",
                FixedTimestamp
            );

            HttpResponseMessage observationResponse = await httpClient.PostAsJsonAsync("/observation", newObservationRequest);
            Observation? createdObservation = await observationResponse.Content.ReadFromJsonAsync<Observation>();

            Assert.NotNull(createdObservation);

            validObservationIds.Add(createdObservation.ID);
        }

        return validObservationIds;
    }

    private string CreateRandomText(int minimumLength, int maximumLength)
    {
        // Intentionally includes characters that can brick CSV files to test that they get stored correctly
        string characterPool = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ,;:'\"!@#$%^&*()[]{}\n";

        int length = random.Next(minimumLength, maximumLength + 1);

        string randomText = "";

        for (int i = 0; i < length; i++)
        {
            int randomCharacterIndex = random.Next(0, characterPool.Length);
            char randomCharacter = characterPool[randomCharacterIndex];

            randomText = randomText + randomCharacter;
        }

        return randomText;
    }

    private int CreateRandomObservationId(List<int> validObservationIds)
    {
        int percentChanceOfBeingValid = 80;
        int roll = random.Next(0, 100);

        bool shouldBeValid = roll < percentChanceOfBeingValid;

        if (shouldBeValid)
        {
            int randomIndex = random.Next(0, validObservationIds.Count);
            return validObservationIds[randomIndex];
        }
        else
        {
            // Ensures invalid ID since all IDs are positive and this creates a negative ID
            return -random.Next(1, 1000000);
        }
    }

    private string CreateRandomTaxonId()
    {
        int percentChanceOfBeingValid = 80;
        int roll = random.Next(0, 100);

        bool shouldBeValid = roll < percentChanceOfBeingValid;

        if (shouldBeValid)
        {
            int randomIndex = random.Next(0, ValidTaxonIds.Count);
            return ValidTaxonIds[randomIndex];
        }
        else
        {
            return CreateRandomText(0, 50);
        }
    }

    // Creates 500 random (but weighted) valid or invalid proposals. The test figures out by itself whether each one
    // should be valid and checks that the service accepts or rejects it accordingly.
    // It thereafter checks that exactly the valid proposals were stored
    [Fact]
    public async Task PostProposal_MatchesOracle_ForManyRandomlyGeneratedProposals()
    {
        List<int> validObservationIds = await CreateValidObservationsAsync(NumberOfValidObservationsToCreate);
        List<Proposal> expectedProposals = new List<Proposal>();

        for (int i = 0; i < NumberOfFuzzIterations; i++)
        {
            int randomObservationId = CreateRandomObservationId(validObservationIds);
            string randomAuthor = CreateRandomText(5, 20);
            string randomTaxonId = CreateRandomTaxonId();

            Proposal randomProposal = new Proposal(randomObservationId, randomAuthor, randomTaxonId, FixedTimestamp);

            bool observationIdIsValid = validObservationIds.Contains(randomObservationId);
            bool taxonIdIsValid = ValidTaxonIds.Contains(randomTaxonId);
            bool proposalShouldBeAccepted = observationIdIsValid && taxonIdIsValid;

            HttpResponseMessage proposalResponse = await httpClient.PostAsJsonAsync("/proposal", randomProposal);

            if (proposalShouldBeAccepted)
            {
                Assert.Equal(HttpStatusCode.OK, proposalResponse.StatusCode);
                expectedProposals.Add(randomProposal);
            }
            else
            {
                Assert.Equal(HttpStatusCode.BadRequest, proposalResponse.StatusCode);
            }
        }

        HttpResponseMessage getProposalsResponse = await httpClient.GetAsync("/proposals");
        Assert.Equal(HttpStatusCode.OK, getProposalsResponse.StatusCode);

        List<Proposal>? actualProposals = await getProposalsResponse.Content.ReadFromJsonAsync<List<Proposal>>();
        Assert.NotNull(actualProposals);

        Assert.Equal(expectedProposals.Count, actualProposals.Count);

        foreach (Proposal expectedProposal in expectedProposals)
        {
            Assert.Contains(expectedProposal, actualProposals);
        }
    }
}
