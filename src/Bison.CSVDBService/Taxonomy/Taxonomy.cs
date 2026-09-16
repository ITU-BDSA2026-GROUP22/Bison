public class Taxonomy {
    private readonly Dictionary<string, Taxon> taxonsByID;
    private readonly Dictionary<string, Taxon> taxonsByVernacularName;
    private readonly Dictionary<string, List<Taxon>> subtaxonsByParentID;

    public Taxonomy(IEnumerable<Taxon> taxons) {

        var taxonList = taxons.ToList();

        taxonsByID = taxonList.ToDictionary(taxon => taxon.TaxonID);

        var taxonasThatHaverVernacularName = taxonList.Where(
            taxon => !string.IsNullOrEmpty(taxon.VernacularName)
        );

        taxonsByVernacularName = taxonasThatHaverVernacularName.ToDictionary(
            taxon => taxon.VernacularName!
        );

        subtaxonsByParentID = new Dictionary<string, List<Taxon>>();

        foreach (Taxon taxon in taxonList) {
            if (string.IsNullOrEmpty(taxon.ParentNameUsageID)) continue;
            if (!subtaxonsByParentID.ContainsKey(taxon.ParentNameUsageID)) {
                subtaxonsByParentID[taxon.ParentNameUsageID] = new List<Taxon>();
            }

            subtaxonsByParentID[taxon.ParentNameUsageID].Add(taxon);
        }
    }

    public Taxon? GetByID(string? id) {
        if (id == null) return null;

       return taxonsByID.GetValueOrDefault(id);
    }

    public Taxon? GetByVernacularName(string name) {
        return taxonsByVernacularName.GetValueOrDefault(name);
    }

    public Taxon? GetSupertaxon(Taxon taxon) {
        return GetByID(taxon.ParentNameUsageID);
    }

    public IEnumerable<Taxon> GetSubtaxons(Taxon taxon) {
        if (subtaxonsByParentID.TryGetValue(taxon.TaxonID, out List<Taxon>? subtaxons)) {
            return subtaxons;
        }

        return Enumerable.Empty<Taxon>();
    }
}

