using UnityEngine;

public class MultipleGames : MonoBehaviour
{
    public GameObject gamePrefab;
    public int numberOfGamesX = 3;
    public int numberOfGamesY = 3;
    public float spacing = 2f;
    public float gameXSize = 28f; // Largeur de la grille de jeu
    public float gameYSize = 31f; // Hauteur de la grille de jeu
    private GameObject[] pacmanAgents;

    void Start()
    {
        pacmanAgents = new GameObject[numberOfGamesX * numberOfGamesY];
        for (int i = 0; i < numberOfGamesX * numberOfGamesY; i++)
        {
            int x = i % numberOfGamesX;
            int y = i / numberOfGamesX;
            Vector3 position = new Vector3(x * (gameXSize + spacing), y * (gameYSize + spacing), 0);
            pacmanAgents[i] = Instantiate(gamePrefab, position, Quaternion.identity);
            pacmanAgents[i].name = "Game_" + (i + 1);
        }

        gamePrefab.SetActive(false); // Désactive le prefab original

        // Centrer la caméra
        float totalWidth = numberOfGamesX * (gameXSize + spacing) - spacing;
        float totalHeight = numberOfGamesY * (gameYSize + spacing) - spacing;
        Camera.main.transform.position = new Vector3(totalWidth / 2, totalHeight / 2 - gameYSize, -15);

        // Ajuster la taille de la caméra pour que tous les jeux soient visibles
        float aspectRatio = (float)Screen.width / Screen.height;
        float cameraHeight = totalHeight / 2 + spacing;
        float cameraWidth = cameraHeight * aspectRatio;
        Camera.main.orthographicSize = Mathf.Max(cameraHeight, cameraWidth / aspectRatio);
    }
}
