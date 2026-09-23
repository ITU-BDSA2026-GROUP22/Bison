using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;

public class ProposalEndpointTests : IDisposable
{
    private const string ValidTaxonId = "MSTSNM:Arter:3e4e67e4-f785-ea11-aa77-501ac539d1ea";

    private readonly WebApplicationFactory<Program> webApplicationFactory;
    private readonly HttpClient httpClient;

    public ProposalEndpointTests()
    {
        // Deletes existing test CSVs to ensure that new tests always start with empty databases
        DeleteFileIfItExists("observations.csv");
        DeleteFileIfItExists("proposals.csv");

        webApplicationFactory = new WebApplicationFactory<Program>();
        httpClient = webApplicationFactory.CreateClient();
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

    [Fact]
    public async Task PostProposal_WithValidObservationIdAndValidTaxonId_ReturnsOk()
    {
        NewObservationRequest newObservationRequest = new NewObservationRequest(
            "TestAuthor",
            "Test observation for proposal tests",
            1700000000
        );

        HttpResponseMessage observationResponse = await httpClient.PostAsJsonAsync("/observation", newObservationRequest);
        Observation? createdObservation = await observationResponse.Content.ReadFromJsonAsync<Observation>();

        Assert.NotNull(createdObservation);

        Proposal proposal = new Proposal(
            createdObservation.ID,
            "TestAuthor",
            ValidTaxonId,
            1700000001
        );

        HttpResponseMessage proposalResponse = await httpClient.PostAsJsonAsync("/proposal", proposal);

        Assert.Equal(HttpStatusCode.OK, proposalResponse.StatusCode);
    }

    [Fact]
    public async Task PostProposal_WithInvalidObservationId_ReturnsBadRequest()
    {
        int observationIdThatDoesNotExist = -1;

        Proposal proposal = new Proposal(
            observationIdThatDoesNotExist,
            "TestAuthor",
            ValidTaxonId,
            1700000002
        );

        HttpResponseMessage proposalResponse = await httpClient.PostAsJsonAsync("/proposal", proposal);

        Assert.Equal(HttpStatusCode.BadRequest, proposalResponse.StatusCode);
    }

    [Fact]
    public async Task PostProposal_WithInvalidTaxonId_ReturnsBadRequest()
    {
        NewObservationRequest newObservationRequest = new NewObservationRequest(
            "TestAuthor",
            "Test observation for proposal tests",
            1700000003
        );

        HttpResponseMessage observationResponse = await httpClient.PostAsJsonAsync("/observation", newObservationRequest);
        Observation? createdObservation = await observationResponse.Content.ReadFromJsonAsync<Observation>();

        Assert.NotNull(createdObservation);

        string taxonIdThatDoesNotExist = "this-taxon-id-does-not-exist";

        Proposal proposal = new Proposal(
            createdObservation.ID,
            "TestAuthor",
            taxonIdThatDoesNotExist,
            1700000004
        );

        HttpResponseMessage proposalResponse = await httpClient.PostAsJsonAsync("/proposal", proposal);

        Assert.Equal(HttpStatusCode.BadRequest, proposalResponse.StatusCode);
    }

    [Fact]
    public async Task GetProposals_ReturnsStoredProposal()
    {
        NewObservationRequest newObservationRequest = new NewObservationRequest(
            "TestAuthor",
            "Test observation for proposal tests",
            1700000005
        );

        HttpResponseMessage observationResponse = await httpClient.PostAsJsonAsync("/observation", newObservationRequest);
        Observation? createdObservation = await observationResponse.Content.ReadFromJsonAsync<Observation>();

        Assert.NotNull(createdObservation);

        Proposal proposalToStore = new Proposal(
            createdObservation.ID,
            "TestAuthor",
            ValidTaxonId,
            1700000006
        );

        await httpClient.PostAsJsonAsync("/proposal", proposalToStore);

        HttpResponseMessage getProposalsResponse = await httpClient.GetAsync("/proposals");
        List<Proposal>? allProposals = await getProposalsResponse.Content.ReadFromJsonAsync<List<Proposal>>();

        Assert.NotNull(allProposals);

        bool storedProposalWasFound = false;
        foreach (Proposal existingProposal in allProposals)
        {
            if (existingProposal.ObservationId == proposalToStore.ObservationId && existingProposal.TaxonId == proposalToStore.TaxonId)
            {
                storedProposalWasFound = true;
            }
        }

        Assert.True(storedProposalWasFound);
    }
}
