namespace OpenBarters.Pricing;

// Independent of the game objects so quantity allocation and rounding can be tested directly.
internal static class BarterPricing {
    // A zero/missing minimum level means any dogtag level, not an item worth zero.
    public static double GetRequiredUnitValue(double handbookPrice, double currencyRate, int minimumDogtagLevel = 1) =>
        handbookPrice / currencyRate * Math.Max(1, minimumDogtagLevel);

    internal readonly struct Requirement(int count, double unitValue) {
        public int Count { get; } = count;
        public double UnitValue { get; } = unitValue;
    }

    internal readonly struct Offer(int count, int sellValue) {
        public int Count { get; } = count;
        public int SellValue { get; } = sellValue;
    }

    internal readonly struct Result(int requiredValue, int basketValue, bool isValid) {
        public int RequiredValue { get; } = requiredValue;
        public int BasketValue { get; } = basketValue;
        public bool IsValid { get; } = isValid;
        public bool CanTrade => IsValid && BasketValue >= RequiredValue;
    }

    public static Result Calculate(IReadOnlyList<Requirement> requirements, IReadOnlyList<Offer> offers,
                                   bool[,] matches, float multiplier) {
        if (float.IsNaN(multiplier) || float.IsInfinity(multiplier)) {
            multiplier = 1.5f;
        }
        multiplier = Math.Max(1f, multiplier);
        // Avoid float artifacts such as 1.1000000238 rounding a cost of 110 up to 111.
        decimal priceMultiplier = (decimal)multiplier;

        var (allocated, available) = AllocateExactItems(requirements, offers, matches);

        decimal baseValue = 0;
        decimal requiredValue = 0;
        for (int requirement = 0; requirement < requirements.Count; requirement++) {
            Requirement entry = requirements[requirement];
            if (entry.Count <= 0) {
                continue;
            }

            // Keep the original per-requisite handbook/currency rounding.
            decimal value = (decimal)Math.Floor(entry.UnitValue * entry.Count);
            int matched = 0;
            for (int offer = 0; offer < offers.Count; offer++) {
                matched += allocated[requirement, offer];
            }
            baseValue += value;
            // Multiply before division and round only the final missing cost. Rounding exact
            // credit and total cost independently could reject an otherwise sufficient basket.
            requiredValue += value * (entry.Count - matched) * priceMultiplier / entry.Count;
        }

        decimal substituteValue = 0;
        for (int offer = 0; offer < offers.Count; offer++) {
            if (offers[offer].Count > 0) {
                // Matched units are already credited at the requirement's value; only the
                // remainder contributes sell value. This also handles partially used stacks.
                substituteValue += (decimal)Math.Max(0, offers[offer].SellValue) * available[offer] / offers[offer].Count;
            }
        }

        return new Result(ToInt(Math.Ceiling(requiredValue)), ToInt(Math.Floor(substituteValue)),
                          baseValue > 0 && requiredValue <= int.MaxValue);
    }

    private static (int[,] Allocated, int[] Available) AllocateExactItems(
        IReadOnlyList<Requirement> requirements, IReadOnlyList<Offer> offers, bool[,] matches) {
        var allocated = new int[requirements.Count, offers.Count];
        int[] available = offers.Select(offer => Math.Max(0, offer.Count)).ToArray();

        // Prefer the most valuable requirement (e.g. the highest dogtag level). Augmenting paths
        // can move earlier matches so a flexible requirement cannot steal a unique match.
        foreach (int requirement in Enumerable.Range(0, requirements.Count)
                                              .OrderByDescending(index => requirements[index].Count > 0
                                                  ? Math.Floor(requirements[index].UnitValue * requirements[index].Count) / requirements[index].Count
                                                  : 0)) {
            int remaining = Math.Max(0, requirements[requirement].Count);
            while (remaining > 0) {
                int added = Allocate(requirement, remaining);
                if (added == 0) {
                    break;
                }
                remaining -= added;
            }
        }

        return (allocated, available);

        int Allocate(int requirement, int count) {
            int[] fromRequirement = Enumerable.Repeat(-1, offers.Count).ToArray();
            int[] fromOffer = Enumerable.Repeat(-1, requirements.Count).ToArray();
            var pending = new Queue<int>();
            fromOffer[requirement] = -2;
            pending.Enqueue(requirement);
            int cheapest = -1;

            // Find every reachable free stack, including paths that reassign earlier matches.
            // Use the cheapest unit for exact credit, leaving expensive surplus as substitutes.
            while (pending.Count > 0) {
                int current = pending.Dequeue();
                for (int offer = 0; offer < offers.Count; offer++) {
                    if (!matches[current, offer] || fromRequirement[offer] != -1) {
                        continue;
                    }
                    fromRequirement[offer] = current;
                    if (available[offer] > 0 && (cheapest == -1 ||
                        (decimal)offers[offer].SellValue / offers[offer].Count <
                        (decimal)offers[cheapest].SellValue / offers[cheapest].Count)) {
                        cheapest = offer;
                    }
                    for (int previous = 0; previous < requirements.Count; previous++) {
                        if (allocated[previous, offer] > 0 && fromOffer[previous] == -1) {
                            fromOffer[previous] = offer;
                            pending.Enqueue(previous);
                        }
                    }
                }
            }
            if (cheapest == -1) {
                return 0;
            }

            int added = Math.Min(count, available[cheapest]);
            for (int offer = cheapest; fromRequirement[offer] != requirement;) {
                int previous = fromRequirement[offer];
                offer = fromOffer[previous];
                added = Math.Min(added, allocated[previous, offer]);
            }
            available[cheapest] -= added;
            for (int offer = cheapest;;) {
                int previous = fromRequirement[offer];
                allocated[previous, offer] += added;
                if (previous == requirement) {
                    return added;
                }
                offer = fromOffer[previous];
                allocated[previous, offer] -= added;
            }
        }
    }

    private static int ToInt(decimal value) => (int)Math.Min(int.MaxValue, Math.Max(0, value));
}
