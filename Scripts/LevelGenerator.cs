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
    void Start()
    {
        GenerateLevel();
        GeneratePacman();
        GenerateGhosts();
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
                Vector3 localPos = new Vector3(x + 0.5f, -y + 0.5f, 0);

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

    void GeneratePacman()
    {
        Vector2Int startPosition = LevelData.PacmanStartPosition;
        Vector3 localPos = new Vector3(startPosition.x + 0.5f, -startPosition.y + 0.5f, 0);
        Instantiate(pacmanPrefab, localPos, Quaternion.identity, transform);
    }

    void GenerateGhosts()
    {
        Vector2Int[] ghostPositions = LevelData.GhostStartPositions;

        for (int i = 0; i < ghostPositions.Length; i++)
        {
            if (i >= ghostPrefabs.Length) break;
            Vector2Int pos = ghostPositions[i];
            Vector3 localPos = new Vector3(pos.x + 0.5f, -pos.y + 0.5f, 0);
            GameObject ghost = Instantiate(ghostPrefabs[i], localPos, Quaternion.identity, transform);
            spawnedGhosts.Add(ghost);
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

        Vector2Int[] ghostPositions = LevelData.GhostStartPositions;
        for (int i = 0; i < spawnedGhosts.Count; i++)
        {
            if (spawnedGhosts[i] != null)
            {
                // Replacer à la position de départ
                Vector2Int startPos = ghostPositions[i];
                spawnedGhosts[i].transform.position = new Vector3(startPos.x + 0.5f, -startPos.y + 0.5f, 0);

                // Optionnel : Réinitialiser les scripts de comportement
                // Si tes fantômes ont un script de base "GhostBase", appelle une fonction Reset dessus
                GhostBase ghostBase = spawnedGhosts[i].GetComponent<GhostBase>();
                if (ghostBase != null)
                {
                    ghostBase.ResetState(); 
                }
            }
        }

    }
}
