using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Bison.Razor.Pages;

public class PublicModel : PageModel
{
    private readonly IObservationService _service;
    public List<ObservationViewModel> Observations { get; set; } = new List<ObservationViewModel>();
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; } = false;

    public PublicModel(IObservationService service)
    {
        _service = service;
    }

    public ActionResult OnGet([FromQuery] int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        CurrentPage = page;
        Observations = _service.GetObservations(page);

        int totalObservations = _service.CountObservations();
        int observationsShownSoFar = CurrentPage * _service.PageSize;
        HasNextPage = observationsShownSoFar < totalObservations;

        return Page();
    }
}
