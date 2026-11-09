
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Bison.Razor.Data;
using Bison.Razor.DTOs;
using Bison.Razor.Models;

namespace Bison.Razor.Pages;

public class ObservationModel : PageModel
{
    private readonly BisonDBContext _db;

    [BindProperty]
    [StringLength(500)]
    public string CommentText { get; set; } = "";
    public List<CommentDTO> Comments { get; set; } = new();

    [BindProperty]
    public int ProposalTaxonId { get; set; }
    public List<Taxon> Taxons { get; set; } = new();
    public List<ProposalDTO> Proposals { get; set; } = new();

    public ObservationDTO? Observation { get; set; }
    public ObservationModel(BisonDBContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var observation = await _db.Observations
            .Include(o => o.Author)
            .Include(o => o.Comments)
                .ThenInclude(c => c.Author)
            .Include(o => o.Proposals)
                .ThenInclude(p => p.Author)
            .FirstOrDefaultAsync(o => o.PostId == id);

        if (observation == null)
        {
            return NotFound();
        }

        Observation = new ObservationDTO
        {
            ID = observation.PostId,
            Username = observation.Author.Name,
            Text = observation.Text,
            PubDate = new DateTimeOffset(observation.TimeStamp).ToUnixTimeSeconds()
        };

        Comments = observation.Comments.Select(comment => new CommentDTO
        {
            Author = comment.Author.Name,
            Message = comment.Text,
            Timestamp = new DateTimeOffset(comment.TimeStamp).ToUnixTimeSeconds()
        }).ToList();

        Proposals = observation.Proposals.Select(proposal => new ProposalDTO
        {
            Author = proposal.Author.Name,
            TaxonId = proposal.TaxonId.ToString(),
            Timestamp = new DateTimeOffset(proposal.TimeStamp).ToUnixTimeSeconds()
        }).ToList();

        Taxons = await _db.Taxons
        .Where(t => !string.IsNullOrWhiteSpace(t.VernacularName))
        .OrderBy(t => t.VernacularName)
        .ToListAsync();

        return Page();
    }

    public string FormatDate(long timestamp)
    {
        DateTimeOffset date = DateTimeOffset.FromUnixTimeSeconds(timestamp);
        return date.UtcDateTime.ToString("dd/MM/yyyy HH:mm");
    }

    public async Task<IActionResult> OnPostAsync(int id)
    {
        if (string.IsNullOrWhiteSpace(CommentText))
        {
            ModelState.AddModelError(nameof(CommentText), "A comment is required.");
            return await OnGetAsync(id);
        }

        if (User.Identity?.Name == null)
        {
            return Unauthorized();
        }

        Author? author = await _db.Authors.FirstOrDefaultAsync(a => a.Email == User.Identity.Name);

        if (author == null)
        {
            return Unauthorized();
        }

        Observation? observation = await _db.Observations.FirstOrDefaultAsync(o => o.PostId == id);

        if (observation == null)
        {
            return NotFound();
        }

        Comment comment = new Comment
        {
            AuthorId = author.AuthorId,
            ObservationId  = observation.PostId,
            Text = CommentText,
            TimeStamp = DateTime.UtcNow
        };

        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Observation", new { id });
    }

    public async Task<IActionResult> OnPostProposalAsync(int id)
    {
        if (ProposalTaxonId <= 0)
        {
            ModelState.AddModelError(nameof(ProposalTaxonId), "Please select a taxon.");
            return await OnGetAsync(id);
        }

        if (User.Identity?.Name == null)
        {
            return Unauthorized();
        }

        Author? author = await _db.Authors
            .FirstOrDefaultAsync(a => a.Email == User.Identity.Name);

        if (author == null)
        {
            return Unauthorized();
        }

        Observation? observation = await _db.Observations
            .FirstOrDefaultAsync(o => o.PostId == id);

        if (observation == null)
        {
            return NotFound();
        }

        Taxon? taxon = await _db.Taxons
            .FirstOrDefaultAsync(t => t.TaxonId == ProposalTaxonId);

        if (taxon == null || string.IsNullOrWhiteSpace(taxon.VernacularName))
        {
            return BadRequest();
        }

        Proposal proposal = new Proposal
        {
            AuthorId = author.AuthorId,
            ObservationId = observation.PostId,
            TaxonId = taxon.TaxonId,
            TimeStamp = DateTime.UtcNow
        };

        _db.Proposals.Add(proposal);
        await _db.SaveChangesAsync();

        return RedirectToPage("/Observation", new { id });
    }

}
