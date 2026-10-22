using OpenBarters.Pricing;
using static OpenBarters.Pricing.BarterPricing;

int passed = 0;

Check("empty basket", [new(2, 5000)], [], new bool[1, 0], 1.5f, 15000, 0, false);
Check("all substitutes", [new(2, 5000)], [new(1, 15000)], new bool[1, 1], 1.5f, 15000, 15000, true);
Check("insufficient substitutes", [new(2, 5000)], [new(1, 14999)], new bool[1, 1], 1.5f, 15000, 14999, false);
Check("all exact at full handbook credit", [new(2, 5000)], [new(2, 6000)], new[,] {{true}}, 1.5f, 0, 0, true);
Check("exact item with no sell price", [new(1, 5000)], [new(1, 0)], new[,] {{true}}, 1.5f, 0, 0, true);
Check("partial barter: 4000 exact plus 9000 substitutes", [new(1, 4000), new(1, 6000)],
      [new(1, 2000), new(1, 9000)], new[,] {{true, false}, {false, false}}, 1.5f, 9000, 9000, true);
Check("same template surplus only gets sell value", [new(2, 100)], [new(5, 250)], new[,] {{true}}, 1.5f, 0, 150, true);
Check("part of a stack plus substitutes", [new(5, 100)], [new(2, 100), new(1, 450)],
      new[,] {{true, false}}, 1.5f, 450, 450, true);
Check("multiplier one still gives exact credit", [new(2, 100)], [new(1, 50), new(1, 100)],
      new[,] {{true, false}}, 1f, 100, 100, true);
Check("changed multiplier revalues the same basket", [new(2, 100)], [new(1, 50), new(1, 100)],
      new[,] {{true, false}}, 2f, 200, 100, false);
Check("selected purchase quantity", [new(6, 100)], [new(2, 100), new(1, 600)],
      new[,] {{true, false}}, 1.5f, 600, 600, true);
Check("foreign currency and fractional handbook value", [new(3, 10.5)], [new(3, 15)],
      new[,] {{true}}, 1.5f, 0, 0, true);
Check("fractional exact credit does not add a currency unit", [new(3, 10.5)], [new(1, 5), new(1, 31)],
      new[,] {{true, false}}, 1.5f, 31, 31, true);
Check("decimal multiplier has no float rounding surcharge", [new(1, 100)], [new(1, 110)],
      new bool[1, 1], 1.1f, 110, 110, true);
Check("wrong dogtag side/level/encoded state is only a substitute", [new(1, 100)], [new(1, 100)],
      new bool[1, 1], 1.5f, 150, 100, false);
Check("one item cannot satisfy duplicate requisites", [new(1, 100), new(1, 100)], [new(1, 50)],
      new[,] {{true}, {true}}, 1.5f, 150, 0, false);
Check("high level dogtag requirement gets priority", [new(1, 100), new(1, 300)], [new(1, 50)],
      new[,] {{true}, {true}}, 1.5f, 150, 0, false);
Check("reassign flexible dogtag to preserve a unique side match", [new(1, 300), new(1, 100)],
      [new(1, 50), new(1, 50)], new[,] {{true, true}, {true, false}}, 1.5f, 0, 0, true);
Check("empty or money-only requisites cannot authorize a deal", [], [new(1, 100)],
      new bool[0, 1], 1.5f, 0, 100, false);
Check("invalid multiplier uses the default", [new(1, 100)], [new(1, 150)],
      new bool[1, 1], float.NaN, 150, 150, true);
Check("fractional substitute stack is rounded down", [new(1, 100), new(1, 1)], [new(3, 100)],
      new[,] {{true}, {false}}, 1.5f, 2, 66, true);
Check("all exact with a larger multiplier", [new(3, 100)], [new(3, 20)], new[,] {{true}}, 10f, 0, 0, true);
Check("unpriced exact requirements cannot authorize a deal", [new(1, 0)], [new(1, 0)], new[,] {{true}}, 1.5f, 0, 0, false);
Check("unrepresentable required cost cannot authorize a deal", [new(1, int.MaxValue)], [new(1, int.MaxValue)],
      new bool[1, 1], 10f, int.MaxValue, int.MaxValue, false);
Check("expensive matching surplus remains available as a substitute", [new(1, 100), new(1, 100)],
      [new(1, 1000), new(1, 10)], new[,] {{true, true}, {false, false}}, 1.5f, 150, 1000, true);
Check("basket insertion order does not change surplus credit", [new(1, 100), new(1, 100)],
      [new(1, 10), new(1, 1000)], new[,] {{true, true}, {false, false}}, 1.5f, 150, 1000, true);
Check("reassign earlier match instead of consuming expensive surplus", [new(1, 300), new(1, 100)],
      [new(1, 1), new(1, 100), new(1, 3)], new[,] {{true, false, true}, {true, true, false}}, 1.5f, 0, 100, true);
Check("zero-level dogtag remains payable with substitutes", [new(2, GetRequiredUnitValue(900, 1, 0))],
      [new(1, 2700)], new bool[1, 1], 1.5f, 2700, 2700, true);
Check("zero-level dogtag exact item closes its requirement", [new(1, GetRequiredUnitValue(900, 1, 0))],
      [new(1, 500)], new[,] {{true}}, 1.5f, 0, 0, true);
Check("level-qualified dogtag retains its original valuation", [new(1, GetRequiredUnitValue(900, 1, 50))],
      [new(1, 67500)], new bool[1, 1], 1.5f, 67500, 67500, true);
Check("zero-level dogtag currency conversion", [new(2, GetRequiredUnitValue(900, 150, 0))],
      [new(1, 18)], new bool[1, 1], 1.5f, 18, 18, true);

// Compare the allocation against exhaustive enumeration, including overlapping requirements
// and stacks. This catches double credit and order-dependent greedy matching failures.
var random = new Random(7416);
for (int trial = 0; trial < 600; trial++) {
    Requirement[] requirements = Enumerable.Range(0, 3).Select(_ => new Requirement(random.Next(1, 3), random.Next(1, 8) * 100)).ToArray();
    Offer[] offers = Enumerable.Range(0, 3).Select(_ => {
        int count = random.Next(1, 3);
        return new Offer(count, count * random.Next(1, 10));
    }).ToArray();
    var matches = new bool[3, 3];
    for (int req = 0; req < 3; req++) {
        for (int offer = 0; offer < 3; offer++) {
            matches[req, offer] = random.Next(2) == 0;
        }
    }
    int total = (int)requirements.Sum(req => req.Count * req.UnitValue);
    Result result = Calculate(requirements, offers, matches, 2f);
    var expected = BestExactValue(requirements, offers, matches);
    int expectedBasket = offers.Sum(offer => offer.SellValue) - expected.Cost;
    if (result.BasketValue != expectedBasket || result.RequiredValue != 2 * (total - expected.Credit)) {
        throw new Exception($"Allocation trial {trial}: expected substitute cost {2 * (total - expected.Credit)}, " +
                            $"basket {expectedBasket}; got {result.RequiredValue}, {result.BasketValue}.");
    }
    passed++;
}
Console.WriteLine($"PASS: {passed} pricing checks (31 scenarios and 600 exhaustive allocation comparisons).");

void Check(string name, Requirement[] requirements, Offer[] offers, bool[,] matches, float multiplier,
           int required, int basket, bool canTrade) {
    Result result = Calculate(requirements, offers, matches, multiplier);
    if (result.RequiredValue != required || result.BasketValue != basket || result.CanTrade != canTrade) {
        throw new Exception($"{name}: expected required={required}, basket={basket}, canTrade={canTrade}; " +
                            $"got required={result.RequiredValue}, basket={result.BasketValue}, canTrade={result.CanTrade}.");
    }
    passed++;
}

static (int Credit, int Cost) BestExactValue(Requirement[] requirements, Offer[] offers, bool[,] matches) {
    int[] units = offers.SelectMany((offer, index) => Enumerable.Repeat(index, offer.Count)).ToArray();
    int[] remaining = requirements.Select(req => req.Count).ToArray();
    return Search(0);

    (int Credit, int Cost) Search(int unit) {
        if (unit == units.Length) {
            return (0, 0);
        }
        var best = Search(unit + 1);
        for (int req = 0; req < requirements.Length; req++) {
            if (remaining[req] > 0 && matches[req, units[unit]]) {
                remaining[req]--;
                var next = Search(unit + 1);
                var candidate = (Credit: (int)requirements[req].UnitValue + next.Credit,
                                 Cost: offers[units[unit]].SellValue / offers[units[unit]].Count + next.Cost);
                if (candidate.Credit > best.Credit || candidate.Credit == best.Credit && candidate.Cost < best.Cost) {
                    best = candidate;
                }
                remaining[req]++;
            }
        }
        return best;
    }
}
