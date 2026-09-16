public class TaxonomyTests {
    private Taxonomy CreateTestTaxonomy() {
        var taxons = new List<Taxon> {
            new Taxon(
                "order1",
                null,
                null,
                "accepted",
                "order",
                "Pelecaniformes",
                null,
                "dan",
                "Årefodede",
                false
            ),

            new Taxon(
                "family1",
                "order1",
                null,
                "accepted",
                "family",
                "Ardeidae",
                null,
                "dan",
                "Hejrer",
                false
            ),

            new Taxon(
                "family2",
                "order1",
                null,
                "accepted",
                "family",
                "Pelecanidae",
                null,
                "dan",
                "Pelikaner",
                false
            ),

            new Taxon(
                "genus1",
                "family1",
                null,
                "accepted",
                "genus",
                "Ardea",
                null,
                "dan",
                null,
                null
            )
        };

        return new Taxonomy(taxons);
    }


    [Fact]
    public void GetByID_ReturnsCorrectTaxon() {
        var taxonomy = CreateTestTaxonomy();

        var taxon = taxonomy.GetByID("family1");

        Assert.NotNull(taxon);
        Assert.Equal("Ardeidae", taxon.ScientificName);
    }


    [Fact]
    public void GetByVernacularName_ReturnsCorrectTaxon() {
        var taxonomy = CreateTestTaxonomy();

        var taxon = taxonomy.GetByVernacularName("Hejrer");

        Assert.NotNull(taxon);
        Assert.Equal("Ardeidae", taxon.ScientificName);
    }


    [Fact]
    public void GetSupertaxon_ReturnsDirectParent() {
        var taxonomy = CreateTestTaxonomy();

        var ardeidae = taxonomy.GetByID("family1");

        Assert.NotNull(ardeidae);

        var parent = taxonomy.GetSupertaxon(ardeidae);

        Assert.NotNull(parent);
        Assert.Equal("Pelecaniformes", parent.ScientificName);
    }


    [Fact]
    public void GetSubtaxons_ReturnsDirectChildren() {
        var taxonomy = CreateTestTaxonomy();

        var pelecaniformes = taxonomy.GetByID("order1");

        Assert.NotNull(pelecaniformes);

        var subtaxons = taxonomy.GetSubtaxons(pelecaniformes).ToList();

        Assert.Equal(2, subtaxons.Count);

        Assert.Contains(
            subtaxons,
            taxon => taxon.ScientificName == "Ardeidae"
        );

        Assert.Contains(
            subtaxons,
            taxon => taxon.ScientificName == "Pelecanidae"
        );
    }


    [Fact]
    public void GetByID_ReturnsNull_WhenTaxonDoesNotExist() {
        var taxonomy = CreateTestTaxonomy();

        var taxon = taxonomy.GetByID("does-not-exist");

        Assert.Null(taxon);
    }
}