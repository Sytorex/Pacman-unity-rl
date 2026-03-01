using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class LevelGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase doorTile;
    
    [Header("Prefabs")]
    public GameObject pelletPrefab;
    public GameObject powerPelletPrefab;
    public GameObject pacmanPrefab;
    public GameObject[] ghostPrefabs; // Blinky, Pinky, Inky, Clyde

    public List<GameObject> allPellets = new List<GameObject>();
    public List<GameObject> spawnedGhosts = new List<GameObject>();

    public static Vector3 GridToWorld(float x, float y, float z = 0f)
    {
        return new Vector3(Mathf.Floor(x) + 0.5f, Mathf.Floor(y) + 0.5f, z);
    }

    void Start()
    {
        GenerateLevel();
    }

    void GenerateLevel()
    {
        int[,] map = LevelData.Map;
        GameObject pellet;
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
                        pellet=Instantiate(pelletPrefab, localPos, Quaternion.identity, transform);
                        allPellets.Add(pellet);
                        break;

                    case TileType.PowerPellet:
                        pellet=Instantiate(powerPelletPrefab, localPos, Quaternion.identity, transform);
                        allPellets.Add(pellet);
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
    }

    void CenterCamera(int width, int height)
    {
        Camera.main.transform.position = new Vector3(width / 2.0f, -height / 2.0f, -10);
    }

    public void ResetLevel()
    {
        foreach (GameObject pellet in allPellets)
        {
            if (pellet != null) pellet.SetActive(true);
        }

        for (int i = 0; i < spawnedGhosts.Count; i++)
        {
            if (spawnedGhosts[i] != null)
            {
                GhostBase ghostBase = spawnedGhosts[i].GetComponent<GhostBase>();
                if (ghostBase != null)
                {
                    ghostBase.ResetState(); 
                }
            }
        }

    }
}
