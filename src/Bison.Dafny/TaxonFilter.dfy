module {:extern "Bison.Razor.Models"} Models {

  class {:extern} Taxon {
    function {:extern "IsSubTaxon"} isSubTaxon(ancestor: Taxon): bool
  }

  class {:extern} Observation {
    function {:extern "GetTaxon"} {:axiom} getTaxon(): Taxon
  }
}

module TaxonFilter {
  import opened Models

  predicate HasTaxonOrSubTaxon(root: Taxon, observation: Observation)
  {
    var taxon := observation.getTaxon();
    taxon.isSubTaxon(root)
  }

  function FilterBy(root: Taxon, observations: seq<Observation>): seq<Observation>
    ensures |FilterBy(root, observations)| <= |observations|
    ensures forall index :: 0 <= index < |FilterBy(root, observations)| ==>
              HasTaxonOrSubTaxon(root, FilterBy(root, observations)[index])
    ensures forall index :: 0 <= index < |FilterBy(root, observations)| ==>
              FilterBy(root, observations)[index] in observations
    ensures forall index :: 0 <= index < |observations| ==>
              (observations[index] in FilterBy(root, observations) <==> HasTaxonOrSubTaxon(root, observations[index]))
  {
    if |observations| == 0 then
      []
    else
      var lastIndex := |observations| - 1;
      var lastObservation := observations[lastIndex];
      var observationsBeforeLast := observations[..lastIndex];
      var filteredBeforeLast := FilterBy(root, observationsBeforeLast);
      if HasTaxonOrSubTaxon(root, lastObservation) then
        filteredBeforeLast + [lastObservation]
      else
        filteredBeforeLast
  }
  by method
  {
    var result: seq<Observation> := [];
    var index := 0;
    while index < |observations|
      invariant 0 <= index <= |observations|
      invariant result == FilterBy(root, observations[..index])
    {
      var observationsUpToNext := observations[..index + 1];
      assert observationsUpToNext[..index] == observations[..index];

      var observation := observations[index];
      if HasTaxonOrSubTaxon(root, observation) {
        result := result + [observation];
      }
      index := index + 1;
    }
    assert observations[..|observations|] == observations;
    return result;
  }

  lemma FilterByAppend(root: Taxon, first: seq<Observation>, second: seq<Observation>)
    ensures FilterBy(root, first + second) == FilterBy(root, first) + FilterBy(root, second)
    decreases |second|
  {
    if |second| == 0 {
      assert first + second == first;
    } else {
      var combined := first + second;
      var lastIndexOfSecond := |second| - 1;
      var secondWithoutLast := second[..lastIndexOfSecond];
      var combinedWithoutLast := combined[..|combined| - 1];
      assert combinedWithoutLast == first + secondWithoutLast;
      FilterByAppend(root, first, secondWithoutLast);
    }
  }

  lemma FilterByKeepsAll(root: Taxon, observations: seq<Observation>)
    requires forall index :: 0 <= index < |observations| ==> HasTaxonOrSubTaxon(root, observations[index])
    ensures FilterBy(root, observations) == observations
  {
    if |observations| > 0 {
      var lastIndex := |observations| - 1;
      var observationsBeforeLast := observations[..lastIndex];
      var lastObservation := observations[lastIndex];
      FilterByKeepsAll(root, observationsBeforeLast);
      assert observationsBeforeLast + [lastObservation] == observations;
    }
  }
}

module {:extern "Bison.Verified"} VerifiedFilter {
  import opened Models
  import TaxonFilter

  class ObservationFilter {
    static method Filter(root: Taxon, observations: seq<Observation>) returns (result: seq<Observation>)
      ensures result == TaxonFilter.FilterBy(root, observations)
    {
      result := TaxonFilter.FilterBy(root, observations);
    }
  }
}
