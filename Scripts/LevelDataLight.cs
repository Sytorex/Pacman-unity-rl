using UnityEngine;
using System.Collections.Generic;

public class LevelDataLight
{
    public static int MapWidth => Map.GetLength(1);
    public static int MapHeight => Map.GetLength(0);

    // Nouvelle map simplifiée (21 colonnes x 11 lignes)
    public static readonly int[,] Map = 
    {
        // 0   1   2   3   4   5   6   7   8   9  10  11  12  13  14  15  16  17  18  19  20
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}, // 0: Mur supérieur
        {-1,  2,  1,  1,  1,  1,  1, -1,  1,  1,  1,  1,  1, -1,  1,  1,  1,  1,  1,  1, -1}, // 1: Couloir haut (Super Pac-Gomme à gauche)
        {-1,  1, -1, -1, -1, -1,  1, -1,  1, -1, -1, -1,  1, -1,  1, -1, -1, -1, -1,  1, -1}, // 2: Piliers haut
        {-1,  1, -1, -1, -1, -1,  1,  1,  1,  0,  0,  0,  1,  1,  1, -1, -1, -1, -1,  1, -1}, // 3: Couloir central (vide au-dessus de la porte)
        {-1,  1, -1, -1, -1, -1,  1, -1, -1, -1,  3, -1, -1, -1,  1, -1, -1, -1, -1,  1, -1}, // 4: Porte Ghost House
        {-1,  1, -1, -1, -1, -1,  1, -1, -1,  0,  0,  0, -1, -1,  1, -1, -1, -1, -1,  1, -1}, // 5: Intérieur Ghost House
        {-1,  1, -1, -1, -1, -1,  1, -1, -1, -1, -1, -1, -1, -1,  1, -1, -1, -1, -1,  1, -1}, // 6: Mur bas Ghost House
        {-1,  1, -1, -1, -1, -1,  1,  1,  1,  1,  1,  1,  1,  1,  1, -1, -1, -1, -1,  1, -1}, // 7: Couloir sous la Ghost House
        {-1,  1, -1, -1, -1, -1,  1, -1,  1, -1, -1, -1,  1, -1,  1, -1, -1, -1, -1,  1, -1}, // 8: Piliers bas
        {-1,  1,  1,  1,  1,  1,  1, -1,  1,  1,  0,  1,  1, -1,  1,  1,  1,  1,  1,  2, -1}, // 9: Couloir bas (Super Pac-Gomme à droite)
        {-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1}  // 10: Mur inférieur
    };

    // Nouveau point de départ centré en bas
    public static readonly Vector2Int PacmanStartPosition = new(10, 9);

    // Nouvelles positions adaptées à la petite Ghost House
    public static readonly Dictionary<string, Vector2Int> GhostHomePositions = new()
    {
        { "Blinky", new(10, 3) }, // À l'extérieur, juste devant la porte
        { "Pinky", new(10, 5) },  // Au centre, à l'intérieur
        { "Inky", new(9,  5) },   // À gauche, à l'intérieur
        { "Clyde", new(11, 5) }   // À droite, à l'intérieur
    };

    public static readonly Vector2Int HomeDoorPosition = new(10, 3);

    // Cibles de dispersion recalibrées sur les 4 coins de la nouvelle map
    public static readonly Dictionary<string, Vector3[]> GhostScatterTargets = new()
    {
        {
            "Blinky", // Coin Haut-Droite
            new[]
            {
                new Vector3(19.5f, -0.5f, LevelGenerator.GhostZLayer),
                new Vector3(19.5f, -2.5f, LevelGenerator.GhostZLayer),
                new Vector3(14.5f, -2.5f, LevelGenerator.GhostZLayer),
                new Vector3(14.5f, -0.5f, LevelGenerator.GhostZLayer)
            }
        },
        {
            "Pinky", // Coin Haut-Gauche
            new[]
            {
                new Vector3(1.5f, -0.5f, LevelGenerator.GhostZLayer),
                new Vector3(1.5f, -2.5f, LevelGenerator.GhostZLayer),
                new Vector3(6.5f, -2.5f, LevelGenerator.GhostZLayer),
                new Vector3(6.5f, -0.5f, LevelGenerator.GhostZLayer)
            }
        },
        {
            "Inky", // Coin Bas-Droite
            new[]
            {
                new Vector3(19.5f, -8.5f, LevelGenerator.GhostZLayer),
                new Vector3(19.5f, -6.5f, LevelGenerator.GhostZLayer),
                new Vector3(14.5f, -6.5f, LevelGenerator.GhostZLayer),
                new Vector3(14.5f, -8.5f, LevelGenerator.GhostZLayer)
            }
        },
        {
            "Clyde", // Coin Bas-Gauche
            new[]
            {
                new Vector3(1.5f, -8.5f, LevelGenerator.GhostZLayer),
                new Vector3(1.5f, -6.5f, LevelGenerator.GhostZLayer),
                new Vector3(6.5f, -6.5f, LevelGenerator.GhostZLayer),
                new Vector3(6.5f, -8.5f, LevelGenerator.GhostZLayer)
            }
        }
    };

    // Ajusté au coin inférieur gauche de la nouvelle grille
    public static readonly Vector3 ClydeNearPacmanFallbackTarget = new(1.5f, -8.5f, LevelGenerator.GhostZLayer);
    
    public static readonly Vector3 InkyAvoidWorldPoint = Vector3.zero;
    public const float InkyAvoidRadius = 0.5f;
    public static readonly HashSet<string> ActiveGhosts = new() { "Blinky", "Clyde" };    

    public enum TileType
    {
        Wall = -1,
        Empty = 0,
        Pellet = 1,
        PowerPellet = 2,
        GhostHouseDoor = 3
    }
}
