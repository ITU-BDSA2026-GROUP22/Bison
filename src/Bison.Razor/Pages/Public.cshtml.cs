using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bison.Razor.Data;
using Bison.Razor.Models;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
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

    public ActionResult OnGet([FromQuery] int page = 1, [FromQuery] int? taxon = null)
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
            o.Author.Name,
            o.Text,
            o.TimeStamp.ToString("MM/dd/yy H:mm:ss")))
        .ToList();

    int observationsShownSoFar =
        CurrentPage * _service.PageSize;

    HasNextPage = observationsShownSoFar < totalObservations;

    return Page();
}
}
