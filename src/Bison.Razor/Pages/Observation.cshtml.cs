using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleDB;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly DBFacade database;
    private readonly IDatabaseRepository<Comment> commentDatabase;
    private readonly IDatabaseRepository<Proposal> proposalDatabase;

    public Observation? Observation { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<Proposal> Proposals { get; set; } = new List<Proposal>();

    public ObservationModel(
        DBFacade database,
        IDatabaseRepository<Comment> commentDatabase,
        IDatabaseRepository<Proposal> proposalDatabase)
    {
        this.database = database;
        this.commentDatabase = commentDatabase;
        this.proposalDatabase = proposalDatabase;
    }

    public IActionResult OnGet(int id)
    {
        Observation = database.GetObservation(id);

        if (Observation == null)
        {
            return NotFound();
        }

        foreach (Comment comment in commentDatabase.Read())
        {
            if (comment.ObservationId == id)
            {
                Comments.Add(comment);
            }
        }

        foreach (Proposal proposal in proposalDatabase.Read())
        {
            if (proposal.ObservationId == id)
            {
                Proposals.Add(proposal);
            }
        }

        return Page();
    }

    public string FormatDate(long timestamp)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return date.UtcDateTime.ToString("dd/MM/yyyy HH:mm");
    }
}