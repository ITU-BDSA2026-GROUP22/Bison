// Dafny program TaxonFilter.dfy compiled into C#
// To recompile, you will need the libraries
//     System.Runtime.Numerics.dll System.Collections.Immutable.dll
// but the 'dotnet' tool in .NET should pick those up automatically.
// Optionally, you may want to include compiler switches like
//     /debug /nowarn:162,164,168,183,219,436,1717,1718

using System;
using System.Numerics;
using System.Collections;
[assembly: DafnyAssembly.DafnySourceAttribute(@"// dafny 4.11.0.0
// Command-line arguments: translate cs src/Bison.Dafny/TaxonFilter.dfy --include-runtime false --output src/Bison.Razor/Verified/TaxonFilter.cs
// TaxonFilter.dfy


module {:extern ""Bison.Razor.Models""} Models {
  class {:extern} Taxon {
    function {:extern ""IsSubTaxon""} isSubTaxon(ancestor: Taxon): bool
      decreases ancestor
  }

  class {:extern} Observation {
    function {:extern ""GetTaxon""} {:axiom} getTaxon(): Taxon
  }
}

module TaxonFilter {
  predicate HasTaxonOrSubTaxon(root: Taxon, observation: Observation)
    decreases root, observation
  {
    var taxon: Taxon := observation.getTaxon();
    taxon.isSubTaxon(root)
  }

  function FilterBy(root: Taxon, observations: seq<Observation>): seq<Observation>
    ensures |FilterBy(root, observations)| <= |observations|
    ensures forall index: int {:trigger FilterBy(root, observations)[index]} :: 0 <= index < |FilterBy(root, observations)| ==> HasTaxonOrSubTaxon(root, FilterBy(root, observations)[index])
    ensures forall index: int {:trigger FilterBy(root, observations)[index]} :: 0 <= index < |FilterBy(root, observations)| ==> FilterBy(root, observations)[index] in observations
    ensures forall index: int {:trigger observations[index]} :: 0 <= index < |observations| ==> (observations[index] in FilterBy(root, observations) <==> HasTaxonOrSubTaxon(root, observations[index]))
    decreases root, observations
  {
    if |observations| == 0 then
      []
    else
      var lastIndex: int := |observations| - 1; var lastObservation: Observation := observations[lastIndex]; var observationsBeforeLast: seq<Observation> := observations[..lastIndex]; var filteredBeforeLast: seq<Observation> := FilterBy(root, observationsBeforeLast); if HasTaxonOrSubTaxon(root, lastObservation) then filteredBeforeLast + [lastObservation] else filteredBeforeLast
  } by method {
    var result: seq<Observation> := [];
    var index := 0;
    while index < |observations|
      invariant 0 <= index <= |observations|
      invariant result == FilterBy(root, observations[..index])
      decreases |observations| - index
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

  lemma /*{:_inductionTrigger FilterBy(root, first) + FilterBy(root, second)}*/ /*{:_inductionTrigger FilterBy(root, first + second)}*/ /*{:_induction root, first, second}*/ FilterByAppend(root: Taxon, first: seq<Observation>, second: seq<Observation>)
    ensures FilterBy(root, first + second) == FilterBy(root, first) + FilterBy(root, second)
    decreases |second|
  {
    if |second| == 0 {
      assert first + second == first;
    } else {
      ghost var combined := first + second;
      ghost var lastIndexOfSecond := |second| - 1;
      ghost var secondWithoutLast := second[..lastIndexOfSecond];
      ghost var combinedWithoutLast := combined[..|combined| - 1];
      assert combinedWithoutLast == first + secondWithoutLast;
      FilterByAppend(root, first, secondWithoutLast);
    }
  }

  lemma /*{:_inductionTrigger FilterBy(root, observations)}*/ /*{:_induction root, observations}*/ FilterByKeepsAll(root: Taxon, observations: seq<Observation>)
    requires forall index: int {:trigger observations[index]} :: 0 <= index < |observations| ==> HasTaxonOrSubTaxon(root, observations[index])
    ensures FilterBy(root, observations) == observations
    decreases root, observations
  {
    if |observations| > 0 {
      ghost var lastIndex := |observations| - 1;
      ghost var observationsBeforeLast := observations[..lastIndex];
      ghost var lastObservation := observations[lastIndex];
      FilterByKeepsAll(root, observationsBeforeLast);
      assert observationsBeforeLast + [lastObservation] == observations;
    }
  }

  import opened Models
}

module {:extern ""Bison.Verified""} VerifiedFilter {

  import opened Models

  import TaxonFilter
  class ObservationFilter {
    static method Filter(root: Taxon, observations: seq<Observation>) returns (result: seq<Observation>)
      ensures result == TaxonFilter.FilterBy(root, observations)
      decreases root, observations
    {
      result := TaxonFilter.FilterBy(root, observations);
    }
  }
}
")]

namespace Dafny {
  internal class ArrayHelpers {
    public static T[] InitNewArray1<T>(T z, BigInteger size0) {
      int s0 = (int)size0;
      T[] a = new T[s0];
      for (int i0 = 0; i0 < s0; i0++) {
        a[i0] = z;
      }
      return a;
    }
  }
} // end of namespace Dafny
internal static class FuncExtensions {
  public static Func<UResult> DowncastClone<TResult, UResult>(this Func<TResult> F, Func<TResult, UResult> ResConv) {
    return () => ResConv(F());
  }
  public static Func<U, UResult> DowncastClone<T, TResult, U, UResult>(this Func<T, TResult> F, Func<U, T> ArgConv, Func<TResult, UResult> ResConv) {
    return arg => ResConv(F(ArgConv(arg)));
  }
  public static Func<U1, U2, UResult> DowncastClone<T1, T2, TResult, U1, U2, UResult>(this Func<T1, T2, TResult> F, Func<U1, T1> ArgConv1, Func<U2, T2> ArgConv2, Func<TResult, UResult> ResConv) {
    return (arg1, arg2) => ResConv(F(ArgConv1(arg1), ArgConv2(arg2)));
  }
}
// end of class FuncExtensions
namespace Bison.Razor.Models {



} // end of namespace Bison.Razor.Models
namespace TaxonFilter {

  public partial class __default {
    public static bool HasTaxonOrSubTaxon(Bison.Razor.Models.Taxon root, Bison.Razor.Models.Observation observation)
    {
      Bison.Razor.Models.Taxon _0_taxon = (observation).GetTaxon();
      return (_0_taxon).IsSubTaxon(root);
    }
    public static Dafny.ISequence<Bison.Razor.Models.Observation> FilterBy(Bison.Razor.Models.Taxon root, Dafny.ISequence<Bison.Razor.Models.Observation> observations)
    {
      Dafny.ISequence<Bison.Razor.Models.Observation> _hresult = Dafny.Sequence<Bison.Razor.Models.Observation>.Empty;
      Dafny.ISequence<Bison.Razor.Models.Observation> _0_result;
      _0_result = Dafny.Sequence<Bison.Razor.Models.Observation>.FromElements();
      BigInteger _1_index;
      _1_index = BigInteger.Zero;
      while ((_1_index) < (new BigInteger((observations).Count))) {
        Dafny.ISequence<Bison.Razor.Models.Observation> _2_observationsUpToNext;
        _2_observationsUpToNext = (observations).Take((_1_index) + (BigInteger.One));
        Bison.Razor.Models.Observation _3_observation;
        _3_observation = (observations).Select(_1_index);
        if (TaxonFilter.__default.HasTaxonOrSubTaxon(root, _3_observation)) {
          _0_result = Dafny.Sequence<Bison.Razor.Models.Observation>.Concat(_0_result, Dafny.Sequence<Bison.Razor.Models.Observation>.FromElements(_3_observation));
        }
        _1_index = (_1_index) + (BigInteger.One);
      }
      _hresult = _0_result;
      return _hresult;
      return _hresult;
    }
  }
} // end of namespace TaxonFilter
namespace Bison.Verified {


  public partial class ObservationFilter {
    public ObservationFilter() {
    }
    public static Dafny.ISequence<Bison.Razor.Models.Observation> Filter(Bison.Razor.Models.Taxon root, Dafny.ISequence<Bison.Razor.Models.Observation> observations)
    {
      Dafny.ISequence<Bison.Razor.Models.Observation> result = Dafny.Sequence<Bison.Razor.Models.Observation>.Empty;
      result = TaxonFilter.__default.FilterBy(root, observations);
      return result;
    }
  }
} // end of namespace Bison.Verified
namespace _module {

} // end of namespace _module
