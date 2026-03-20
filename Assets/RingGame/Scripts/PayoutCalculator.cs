using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class PayoutCalculator
{
    public struct PayoutResult
    {
        public float totalPayout;
        public float multiplier;
        public List<SymbolConfig.SymbolType> winningSymbols;
        public string resultLabel;
        public bool isWin;
    }

    private static readonly Dictionary<SymbolConfig.SymbolType, float> symbolValues = new Dictionary<SymbolConfig.SymbolType, float>
    {
        { SymbolConfig.SymbolType.Wild,     5.0f },
        { SymbolConfig.SymbolType.Spades,   2.0f },
        { SymbolConfig.SymbolType.Hearts,   1.8f },
        { SymbolConfig.SymbolType.Diamonds, 2.2f },
        { SymbolConfig.SymbolType.Clubs,    1.5f },
    };

    public static PayoutResult Calculate(List<SymbolConfig.SymbolType?> captured, float betAmount)
    {
        var result = new PayoutResult();
        result.winningSymbols = new List<SymbolConfig.SymbolType>();

        var nonNull = captured.Where(s => s.HasValue).Select(s => s.Value).ToList();

        if (nonNull.Count == 0)
        {
            result.resultLabel = "NO CAPTURE";
            result.isWin = false;
            return result;
        }

        var groups = nonNull.GroupBy(s => s == SymbolConfig.SymbolType.Wild ? SymbolConfig.SymbolType.Wild : s)
                            .ToDictionary(g => g.Key, g => g.Count());

        int wildCount = groups.ContainsKey(SymbolConfig.SymbolType.Wild) ? groups[SymbolConfig.SymbolType.Wild] : 0;

        float bestMultiplier = 0f;
        SymbolConfig.SymbolType bestSymbol = SymbolConfig.SymbolType.Clubs;

        foreach (var sym in groups.Keys)
        {
            if (sym == SymbolConfig.SymbolType.Wild) continue;

            int count = groups[sym] + wildCount;
            float mult = GetMultiplier(sym, count);
            if (mult > bestMultiplier)
            {
                bestMultiplier = mult;
                bestSymbol = sym;
            }
        }

        if (wildCount > 0 && groups.Count == 1)
        {
            float wildMult = GetMultiplier(SymbolConfig.SymbolType.Wild, wildCount);
            if (wildMult > bestMultiplier)
            {
                bestMultiplier = wildMult;
                bestSymbol = SymbolConfig.SymbolType.Wild;
            }
        }

        if (bestMultiplier > 0f)
        {
            result.isWin = true;
            result.multiplier = bestMultiplier;
            result.totalPayout = betAmount * bestMultiplier;
            result.winningSymbols = nonNull.Where(s => s == bestSymbol || s == SymbolConfig.SymbolType.Wild).ToList();
            result.resultLabel = GetLabel(bestMultiplier);
        }
        else
        {
            result.isWin = false;
            result.multiplier = 0f;
            result.totalPayout = 0f;
            result.resultLabel = nonNull.Count > 0 ? "NO MATCH" : "NO CAPTURE";
        }

        return result;
    }

    private static float GetMultiplier(SymbolConfig.SymbolType sym, int count)
    {
        float baseValue = symbolValues.ContainsKey(sym) ? symbolValues[sym] : 1f;

        return count switch
        {
            1 => 0f,
            2 => baseValue * 0.167f,
            3 => baseValue * 0.5f,
            4 => baseValue * 1.0f,
            _ => baseValue * (count * 0.267f)
        };
    }

    private static string GetLabel(float multiplier)
    {
        if (multiplier >= 3.3f) return "JACKPOT!";
        if (multiplier >= 1.7f)  return "BIG WIN!";
        if (multiplier >= 0.67f) return "WIN!";
        return "SMALL WIN";
    }
}
