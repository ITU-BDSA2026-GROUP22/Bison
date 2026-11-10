using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using Bison.Razor.Data;
using Bison.Razor.Models;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{

    [BindProperty]
    [Required]
    [StringLength(500)] // Limit the observation text to 500 characters
    public string ObservationText { get; set; } = "";
    [BindProperty]
    public int TaxonId { get; set; }
    public List<Taxon> Taxons { get; set; } = new();
    private readonly IObservationService _service;
    private readonly BisonDBContext _db;
    private readonly ITaxonFilterService _taxonFilterService;
    public List<ObservationViewModel> Observations { get; set; } = new List<ObservationViewModel>();
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; } = false;

    public PublicModel(IObservationService service, BisonDBContext db, ITaxonFilterService taxonFilterService)
    {
        _service = service;
        _db = db;
        _taxonFilterService = taxonFilterService;

    }

    public async Task<IActionResult> OnGet([FromQuery] int page = 1, [FromQuery] int? taxon = null)
{
    if (page < 1)
    {
        page = 1;
    }

    CurrentPage = page;

    List<Bison.Razor.Models.Observation> observations =
        _db.Observations
            .Include(o => o.Author)
            .Include(o => o.Taxon)
            .ThenInclude(t => t.Parent)
            .ToList();

    if (taxon.HasValue)
    {
        Taxon? selectedTaxon = _db.Taxons
            .FirstOrDefault(t => t.TaxonId == taxon.Value);

        if (selectedTaxon == null)
        {
            return NotFound();
        }

        observations = _taxonFilterService.Filter(
            selectedTaxon,
            observations);
    }

    int totalObservations = observations.Count;
    int observationsToSkip = (page - 1) * _service.PageSize;

    observations = observations
        .OrderByDescending(o => o.TimeStamp)
        .Skip(observationsToSkip)
        .Take(_service.PageSize)
        .ToList();

    Observations = observations
        .Select(o => new ObservationViewModel(
            o.PostId,
            o.Author.Name,
            o.Text,
            o.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
        .ToList();

    int observationsShownSoFar =
        CurrentPage * _service.PageSize;

    HasNextPage = observationsShownSoFar < totalObservations;

    Taxons = await _db.Taxons.Where(t => !string.IsNullOrWhiteSpace(t.VernacularName)).OrderBy(t => t.VernacularName).ToListAsync();

    return Page();
}

public async Task<IActionResult> OnPostAsync()
{
    // Prevent empty observations from being submitted and can't be more than set limit
    if (!ModelState.IsValid)
{
    Taxons = await _db.Taxons.Where(t => !string.IsNullOrWhiteSpace(t.VernacularName)).OrderBy(t => t.VernacularName).ToListAsync();
    return Page();
}

    // Check that the user is logged in
    if (User.Identity?.Name == null)
    {
        return Unauthorized();
    }

    // Find the author
    Author? author = await _db.Authors .FirstOrDefaultAsync(a => a.Email == User.Identity.Name);

    if (author == null)
    {
        return Unauthorized();
    }

    // Find the selected taxon
    Taxon? taxon = await _db.Taxons.FirstOrDefaultAsync(t => t.TaxonId == TaxonId);

    if (taxon == null)
    {
        return BadRequest();
    }

    // Create the observation
    Observation observation = new Observation
    {
        AuthorId = author.AuthorId,
        TaxonId = taxon.TaxonId,
        Text = ObservationText,
        TimeStamp = DateTime.UtcNow
    };

    _db.Observations.Add(observation);
    await _db.SaveChangesAsync();

    return RedirectToPage();
}
}
