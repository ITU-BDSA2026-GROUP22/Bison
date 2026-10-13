using Bison.Razor.Data;
using Microsoft.EntityFrameworkCore;
using SimpleDB;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Public", "obs");
    options.Conventions.AddPageRoute("/Public", "ob");
    options.Conventions.AddPageRoute("/UserTimeline", "obs/{author}");
});
builder.Services.AddScoped<IPostRepository, PostRepository>();
builder.Services.AddScoped<IObservationService, ObservationService>();

string dataFolder = Path.GetFullPath(
    Path.Combine(builder.Environment.ContentRootPath, "../Bison.CSVDBService")
);

builder.Services.AddSingleton<IDatabaseRepository<Comment>>(
    new CSVDatabase<Comment>(Path.Combine(dataFolder, "comments.csv"))
);

builder.Services.AddSingleton<IDatabaseRepository<Proposal>>(
    new CSVDatabase<Proposal>(Path.Combine(dataFolder, "proposals.csv"))
);

string databasePath = Path.Combine(builder.Environment.ContentRootPath, "bison.db");
string connectionString = "Data Source=" + databasePath;

builder.Services.AddDbContext<BisonDBContext>(options =>
{
    options.UseSqlite(connectionString);
});

var app = builder.Build();

// Create the database if it does not exist, and seed it with the example data.
using (IServiceScope scope = app.Services.CreateScope())
{
    BisonDBContext context = scope.ServiceProvider.GetRequiredService<BisonDBContext>();
    context.Database.EnsureCreated();
    DbInitializer.SeedDatabase(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();

app.Run();

// Makes Program visible to the test project, so tests can start the app with WebApplicationFactory
public partial class Program { }
