using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Bison.Razor.Data;
using Bison.Razor.Models;

namespace Bison.Razor.Pages;

public class UserTimelineModel : PageModel
{
    private readonly BisonDBContext _db;
    private readonly int _pageSize = 32;
    public List<ObservationViewModel> Observations { get; set; } = new List<ObservationViewModel>();
    public int CurrentPage { get; set; } = 1;
    public bool HasNextPage { get; set; } = false;

    public UserTimelineModel(BisonDBContext db)
    {
        _db = db;
    }

    
    public ActionResult OnGet(string author, [FromQuery] int page = 1)
    {
        if (page < 1)
        {
            page = 1;
        }

        CurrentPage = page;

        var observations = _db.Observations
            .Include(o => o.Author)
            .Where(o => o.Author.Name == author)
            .OrderByDescending(o => o.TimeStamp);

        int totalObservations = observations.Count();

        Observations = observations
            .Skip((page - 1) * _pageSize)
            .Take(_pageSize)
            .ToList()
            .Select(o => new ObservationViewModel(
                o.PostId,
                o.Author.Name,
                o.Text,
                o.TimeStamp.ToString("MM/dd/yy H:mm:ss"),
                o.PictureId))
            .ToList();

        HasNextPage = page * _pageSize < totalObservations;

        return Page();
    }

}
