var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AddPageRoute("/Public", "obs");
    options.Conventions.AddPageRoute("/UserTimeline", "obs/{author}");
});
builder.Services.AddSingleton<DBFacade>();
builder.Services.AddSingleton<IObservationService, ObservationService>();


var app = builder.Build();

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
