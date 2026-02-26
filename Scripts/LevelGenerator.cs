using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

 public class LevelGenerator : MonoBehaviour
{
    [Header("Configuration")]
    public Tilemap tilemap;
    public TileBase wallTile;
    public TileBase doorTile;
    public float tilemapLayerY = 0f;
    
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
        for (int z = 0; z < map.GetLength(0); z++) 
        {
            for (int x = 0; x < map.GetLength(1); x++)
            {
                // Récupère la valeur de la cellule et convertit en TileType
                int cellValue = map[z, x];
                TileType type = (TileType)cellValue;

                // Calcule la position de la tuile dans le Tilemap et la position du monde pour les objets
                Vector3Int tilePosition = LevelData.GridToTilePosition(x, z);
                Vector3 localPos = LevelData.GridToWorld(x, z);

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
        Vector3 localPos = LevelData.GridToWorld(startPosition.x, startPosition.y);
        Instantiate(pacmanPrefab, localPos, Quaternion.identity, transform);
    }

    void GenerateGhosts()
    {
        Vector2Int[] ghostPositions = LevelData.GhostStartPositions;

        for (int i = 0; i < ghostPositions.Length; i++)
        {
            if (i >= ghostPrefabs.Length) break;
            Vector2Int pos = ghostPositions[i];
            Vector3 localPos = LevelData.GridToWorld(pos.x, pos.y);
            GameObject ghost = Instantiate(ghostPrefabs[i], localPos, Quaternion.identity, transform);
            spawnedGhosts.Add(ghost);
        }
    }

    void CenterCamera(int width, int height)
    {
        Camera.main.transform.position = new Vector3(width / 2.0f, 20f, -height / 2.0f);
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
                Vector3 localPos = LevelData.GridToWorld(startPos.x, startPos.y);
                spawnedGhosts[i].transform.position = localPos;

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
