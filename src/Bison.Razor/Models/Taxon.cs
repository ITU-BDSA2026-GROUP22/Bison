namespace Bison.Razor.Models;

public class Taxon
{
    public int TaxonId { get; set; }
    public string dwc_TaxonID { get; set; } = "";

    // Danish vernacular name. Null for taxons that have no Danish name.
    public string? VernacularName { get; set; }

    public int? ParentId { get; set; }
    public Taxon? Parent { get; set; }

    public List<Taxon> Children { get; set; } = new List<Taxon>();
}
