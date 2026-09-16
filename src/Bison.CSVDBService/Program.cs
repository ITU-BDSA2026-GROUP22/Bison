using SimpleDB;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDatabaseRepository<Observation>>(new CSVDatabase<Observation>("observations.csv"));
builder.Services.AddSingleton<IDatabaseRepository<Comment>>(new CSVDatabase<Comment>("comments.csv"));

var taxonomy = TaxonomyLoader.Load();

var app = builder.Build();

// observations
app.MapGet("/observations", (IDatabaseRepository<Observation> db) => db.Read());
app.MapPost("/observation", (IDatabaseRepository<Observation> db, Observation observation) => db.Store(observation));

// comments
app.MapGet("/comments", (IDatabaseRepository<Comment> db) => db.Read());
app.MapPost("/comment", (IDatabaseRepository<Comment> db, Comment comment) => db.Store(comment));


app.Run();

public record Observation(string Author, string Message, long Timestamp);
public record Comment(int ObservationId, string Author, string Message, long Timestamp);