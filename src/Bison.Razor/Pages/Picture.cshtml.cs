using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Infrastructure.Data;
using Bison.Infrastructure.Models;

namespace Bison.Razor.Pages;

public class PictureModel : PageModel {
    private readonly BisonDBContext _db;

    public PictureModel(BisonDBContext db) {
        _db = db;
    }

    public async Task<IActionResult> OnGetAsync(int id) {
        Picture? picture = await _db.Pictures.FindAsync(id);

        if (picture == null) {
            return NotFound();
        }

        return File(picture.Data, picture.ContentType);
    }
}
