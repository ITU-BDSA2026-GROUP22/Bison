using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SimpleDB;
using Bison.Razor.DTOs;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly IPostRepository  database;
    private readonly IDatabaseRepository<Comment> commentDatabase;
    private readonly IDatabaseRepository<Proposal> proposalDatabase;

    public ObservationDTO? Observation { get; set; }
    public List<CommentDTO> Comments { get; set; } = new();
    public List<ProposalDTO> Proposals { get; set; } = new();

    public ObservationModel(
        IPostRepository database,
        IDatabaseRepository<Comment> commentDatabase,
        IDatabaseRepository<Proposal> proposalDatabase)
    {
        this.database = database;
        this.commentDatabase = commentDatabase;
        this.proposalDatabase = proposalDatabase;
    }

    public IActionResult OnGet(int id)
{
    Observation? observation = database.GetObservation(id);

    if (observation == null)
    {
        return NotFound();
    }

    Observation = new ObservationDTO
    {
        ID = observation.ID,
        Username = observation.Author,
        Text = observation.Message,
        PubDate = observation.Timestamp
    };

    foreach (Comment comment in commentDatabase.Read())
    {
        if (comment.ObservationId == id)
        {
            Comments.Add(new CommentDTO
            {
                Author = comment.Author,
                Message = comment.Message,
                Timestamp = comment.Timestamp
            });
        }
    }

    foreach (Proposal proposal in proposalDatabase.Read())
    {
        if (proposal.ObservationId == id)
        {
            Proposals.Add(new ProposalDTO
            {
                Author = proposal.Author,
                TaxonId = proposal.TaxonId,
                Timestamp = proposal.Timestamp
            });
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