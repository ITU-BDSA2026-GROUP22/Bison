using System.Reflection;
using Microsoft.Extensions.FileProviders;
using CsvHelper;
using System.Globalization;

public static class TaxonomyLoader {
    public static Taxonomy Load() {
        var embeddedProvider =
            new EmbeddedFileProvider(Assembly.GetExecutingAssembly());

        using var reader = embeddedProvider.GetFileInfo("data/taxons.csv").CreateReadStream();

        using var sr = new StreamReader(reader);

        using var csv = new CsvReader(sr, CultureInfo.InvariantCulture);

        csv.Read();
        csv.ReadHeader();

        var taxons = new List<Taxon>();

        while (csv.Read()) {
            var taxon = new Taxon(
                csv.GetField<string>("dwc:taxonID")!,
                csv.GetField<string>("dwc:parentNameUsageID"),
                csv.GetField<string>("dwc:acceptedNameUsageID"),
                csv.GetField<string>("dwc:taxonomicStatus")!,
                csv.GetField<string>("dwc:taxonRank")!,
                csv.GetField<string>("dwc:scientificName")!,
                csv.GetField<string>("dwc:scientificNameAuthorship"),
                csv.GetField<string>("dcterms:language")!,
                csv.GetField<string>("dwc:vernacularName"),
                csv.GetField<bool?>("clb:merged")
            );

            taxons.Add(taxon);
        }

        return new Taxonomy(taxons);
    }
}