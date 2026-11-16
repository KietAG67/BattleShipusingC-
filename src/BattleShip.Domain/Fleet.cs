namespace BattleShip.Domain;

public static class Fleet
{
    public static readonly IReadOnlyList<(string Name, int Length)> Default =
    [
        ("Carrier", 5),
        ("Battleship", 4),
        ("Cruiser", 3),
        ("Submarine", 3),
        ("Destroyer", 2),
    ];
}
