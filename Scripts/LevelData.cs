// We can select between LevelDataLight, LevelDataMedium, and LevelDataHard by changing the inheritance of LevelData
public class LevelData : LevelDataMedium {}

public enum TileType
{
    Wall = -1,
    Empty = 0,
    Pellet = 1,
    PowerPellet = 2,
    GhostHouseDoor = 3
}