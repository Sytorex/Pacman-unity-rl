using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase doorTile;

    [Header("Elements")]
    public GameObject pelletContainer;

    [Header("Prefabs")]
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;

    public const float DefaultZLayer = 0f;
    public const float GhostZLayer = -0.1f;
    public const float PacmanZLayer = -0.5f;

    public static Vector3 GridToWorld(float x, float y, float z = DefaultZLayer)
    {
        return new Vector3(Mathf.Floor(x) + 0.5f, Mathf.Floor(y) + 0.5f, z);
    }

    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), Mathf.FloorToInt(worldPos.y));
    }

    public List<GameObject> GenerateLevel()
    {
        List<GameObject> pellets = new List<GameObject>();
        int[,] map = LevelData.Map;
        GameObject pellet;

        tilemap.ClearAllTiles();

        for (int y = 0; y < map.GetLength(0); y++) 
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // Récupère la valeur de la cellule et convertit en TileType
                int cellValue = map[y, x];
                TileType type = (TileType)cellValue;

                // Calcule la position de la tuile dans le Tilemap et la position du monde pour les objets
                Vector3Int tilePosition = new Vector3Int(x, -y, 0);
                Vector3 localPos = GridToWorld(x, -y);

                switch (type)
                {
                    case TileType.Wall:
                        tilemap.SetTile(tilePosition, wallTile);
                        break;

                    case TileType.Pellet:
                        pellet = Instantiate(pelletPrefab, pelletContainer.transform);
                        pellet.transform.localPosition = localPos;
                        pellets.Add(pellet);
                        break;

                    case TileType.PowerPellet:
                        pellet = Instantiate(powerPelletPrefab, pelletContainer.transform);
                        pellet.transform.localPosition = localPos;
                        pellets.Add(pellet);
                        break;
                    
                    case TileType.GhostHouseDoor:
                        tilemap.SetTile(tilePosition, doorTile);
                        break;

                    case TileType.Empty:
                    default:
                        // Do nothing
                        break;
                }
            }
        }

        return pellets;
    }
}
