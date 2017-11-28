using System.Collections.Generic;

public static class Constants {
    public const int Pawn = 1;
    public const int Rook = 2;
    public const int Knight = 3;
    public const int Bishop = 4;
    public const int Queen = 5;
    public const int King = 6;
    public const int PawnInitial = 7;
    public const int MoveToFree = 0;
    public const int MoveToTaken = 1;
    public const int MoveAny = 2;
    public const int White = 1;
    public const int Black = 2;
    public static string[] letters = new string[8] { "A", "B", "C", "D", "E", "F", "G", "H" };
    public static Dictionary<int, int> weights = new Dictionary<int, int>() {
        { Pawn, 1},
        { Rook, 2},
        { Knight, 2},
        { Bishop, 2},
        { Queen, 5},
        { King, 8},
        { PawnInitial, 2},
    };
}
