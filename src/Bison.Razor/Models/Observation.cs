namespace Bison.Razor.Models;

public class Observation : Post
{
    public int TaxonId { get; set; }
    public Taxon Taxon { get; set; } = null!;
    
    public int? PictureId { get; set; }
    public Picture? Picture { get; set; }

    public List<Comment> Comments { get; set; } = new List<Comment>();
    public List<Proposal> Proposals { get; set; } = new List<Proposal>();

    public Taxon GetTaxon()
    {
        return Taxon;
    }
}
