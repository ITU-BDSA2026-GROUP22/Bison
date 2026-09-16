public record Taxon(
    string TaxonID,
    string? ParentNameUsageID,
    string? AcceptedNameUsageID,
    string TaxonomicStatus,
    string TaxonRank,
    string ScientificName,
    string? ScientificNameAuthorship,
    string Language,
    string? VernacularName,
    bool? Merged
);