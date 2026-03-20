using UnityEngine;
using System.Collections.Generic;

public enum SwipeDirection { Up, Down, Left, Right }

public class SwipeDirectionMap : MonoBehaviour
{
    public static SwipeDirectionMap Instance { get; private set; }

    private Dictionary<SymbolConfig.SymbolType, SwipeDirection> mapping = new Dictionary<SymbolConfig.SymbolType, SwipeDirection>();

    private static readonly SymbolConfig.SymbolType[] Suits =
    {
        SymbolConfig.SymbolType.Spades,
        SymbolConfig.SymbolType.Hearts,
        SymbolConfig.SymbolType.Diamonds,
        SymbolConfig.SymbolType.Clubs
    };

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void GenerateNewMapping()
    {
        mapping.Clear();
        var directions = new List<SwipeDirection>
        {
            SwipeDirection.Up,
            SwipeDirection.Down,
            SwipeDirection.Left,
            SwipeDirection.Right
        };

        for (int i = directions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (directions[i], directions[j]) = (directions[j], directions[i]);
        }

        for (int i = 0; i < Suits.Length; i++)
            mapping[Suits[i]] = directions[i];
    }

    public SwipeDirection GetDirection(SymbolConfig.SymbolType suit)
    {
        if (mapping.TryGetValue(suit, out var dir))
            return dir;
        Debug.LogWarning("SwipeDirectionMap: No mapping for " + suit + ", returning Up");
        return SwipeDirection.Up;
    }

    public bool IsCorrectSwipe(SymbolConfig.SymbolType symbol, SwipeDirection swipeDir)
    {
        if (symbol == SymbolConfig.SymbolType.Wild)
            return true;

        if (mapping.TryGetValue(symbol, out var required))
            return required == swipeDir;

        Debug.LogWarning("SwipeDirectionMap: No mapping for " + symbol);
        return false;
    }

    public Dictionary<SymbolConfig.SymbolType, SwipeDirection> GetMapping()
    {
        return new Dictionary<SymbolConfig.SymbolType, SwipeDirection>(mapping);
    }

    public static string GetArrowForDirection(SwipeDirection dir)
    {
        return dir switch
        {
            SwipeDirection.Up => "\u2191",
            SwipeDirection.Down => "\u2193",
            SwipeDirection.Left => "\u2190",
            SwipeDirection.Right => "\u2192",
            _ => "?"
        };
    }

    public static string GetNameForDirection(SwipeDirection dir)
    {
        return dir switch
        {
            SwipeDirection.Up => "UP",
            SwipeDirection.Down => "DOWN",
            SwipeDirection.Left => "LEFT",
            SwipeDirection.Right => "RIGHT",
            _ => "?"
        };
    }
}
