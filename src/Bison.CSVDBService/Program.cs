using SimpleDB;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<DBFacade>();
builder.Services.AddSingleton<IDatabaseRepository<Observation>>(new CSVDatabase<Observation>("observations.csv"));
builder.Services.AddSingleton<IDatabaseRepository<Comment>>(new CSVDatabase<Comment>("comments.csv"));
builder.Services.AddSingleton<IDatabaseRepository<Proposal>>(new CSVDatabase<Proposal>("proposals.csv"));

var taxonomy = TaxonomyLoader.Load();
// Required for proposals to check for valid TaxonIDs
builder.Services.AddSingleton(taxonomy);

var app = builder.Build();


// observations
//old method app.MapGet("/observations", (IDatabaseRepository<Observation> db) => db.Read());
app.MapGet("/observations", ([FromServices] DBFacade db) => db.GetObservations());

app.MapGet("/observations/{author}", ([FromServices] DBFacade db, string author) =>
    db.GetObservationsFromAuthor(author));

app.MapPost("/observation", (IDatabaseRepository<Observation> db, NewObservationRequest request) =>
{
    IEnumerable<Observation> allObservations = db.Read();

    int highestExistingId = 0;
    foreach (Observation existingObservation in allObservations)
    {
        if (existingObservation.ID > highestExistingId)
        {
            highestExistingId = existingObservation.ID;
        }
    }

    int newObservationId = highestExistingId + 1;

    Observation newObservation = new Observation(
        newObservationId,
        request.Author,
        request.Message,
        request.Timestamp,
        request.Location
    );

    db.Store(newObservation);

    return newObservation;
});

// comments
app.MapGet("/comments", (IDatabaseRepository<Comment> db) => db.Read());
app.MapPost("/comment", (IDatabaseRepository<Observation> observationDb, IDatabaseRepository<Comment> commentDb, Comment comment) =>
{
    IEnumerable<Observation> allObservations = observationDb.Read();

    bool observationWithThisIdExists = false;
    foreach (Observation existingObservation in allObservations)
    {
        if (existingObservation.ID == comment.ObservationId)
        {
            observationWithThisIdExists = true;
        }
    }

    if (!observationWithThisIdExists)
    {
        return Results.BadRequest("Invalid ObservationId");
    }

    commentDb.Store(comment);
    return Results.Ok();
});

// proposals
app.MapGet("/proposals", (IDatabaseRepository<Proposal> db) => db.Read());
app.MapPost("/proposal", (IDatabaseRepository<Observation> observationDb, IDatabaseRepository<Proposal> proposalDb, Taxonomy taxonomy, Proposal proposal) =>
{
    IEnumerable<Observation> allObservations = observationDb.Read();

    bool observationWithThisIdExists = false;
    foreach (Observation existingObservation in allObservations)
    {
        if (existingObservation.ID == proposal.ObservationId)
        {
            observationWithThisIdExists = true;
        }
    }

    if (!observationWithThisIdExists)
    {
        return Results.BadRequest("Invalid ObservationId");
    }

    Taxon? matchingTaxon = taxonomy.GetByID(proposal.TaxonId);
    bool taxonWithThisIdExists = matchingTaxon != null;

    if (!taxonWithThisIdExists)
    {
        return Results.BadRequest("Invalid TaxonId");
    }

    proposalDb.Store(proposal);
    return Results.Ok();
});


app.Run();



// Makes Program class public, which allows it to be used for tests
public partial class Program { }