using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Bison.Razor.Data;
using Bison.Razor.Models;

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
