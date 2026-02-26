using UnityEngine;

[CreateAssetMenu(fileName = "SymbolConfig", menuName = "RingGame/Symbol Config")]
public class SymbolConfig : ScriptableObject
{
    [Header("Suit Icons (assign sprites here)")]
    public Sprite spadesIcon;
    public Sprite heartsIcon;
    public Sprite diamondsIcon;
    public Sprite clubsIcon;
    public Sprite wildIcon;

    [Header("Suit Colors")]
    public Color spadesColor = new Color(0.85f, 0.9f, 1f);
    public Color heartsColor = new Color(0.95f, 0.2f, 0.2f);
    public Color diamondsColor = new Color(0.2f, 0.6f, 1f);
    public Color clubsColor = new Color(0.3f, 0.85f, 0.4f);
    public Color wildColor = new Color(1f, 0.82f, 0.1f);

    public enum SymbolType
    {
        Spades,
        Hearts,
        Diamonds,
        Clubs,
        Wild
    }

    public Sprite GetIcon(SymbolType type)
    {
        return type switch
        {
            SymbolType.Spades => spadesIcon,
            SymbolType.Hearts => heartsIcon,
            SymbolType.Diamonds => diamondsIcon,
            SymbolType.Clubs => clubsIcon,
            SymbolType.Wild => wildIcon,
            _ => null
        };
    }

    public Color GetColor(SymbolType type)
    {
        return type switch
        {
            SymbolType.Spades => spadesColor,
            SymbolType.Hearts => heartsColor,
            SymbolType.Diamonds => diamondsColor,
            SymbolType.Clubs => clubsColor,
            SymbolType.Wild => wildColor,
            _ => Color.white
        };
    }
}
