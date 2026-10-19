using Bison.Razor.Models;
using Bison.Verified;
namespace Bison.Razor;

public interface ITaxonFilterService
{
    List<Observation> Filter(
        Taxon taxon,
        List<Observation> observations);
}

public class TaxonFilterService : ITaxonFilterService
{
    public List<Observation> Filter(
        Taxon taxon,
        List<Observation> observations)
    {
        Dafny.ISequence<Observation> dafnyObservations =
            Dafny.Sequence<Observation>.FromArray(observations.ToArray());

        Dafny.ISequence<Observation> filtered =
            ObservationFilter.Filter(taxon, dafnyObservations);

        List<Observation> result = new List<Observation>();

        for (int i = 0; i < filtered.Count; i++)
        {
            result.Add(filtered.Select(i));
        }

        return result;
    }
}