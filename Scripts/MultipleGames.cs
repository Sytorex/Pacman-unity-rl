using UnityEngine;

public class MultipleGames : MonoBehaviour
{
    public GameObject gamePrefab;
    public int numberOfGamesX = 3;
    public int numberOfGamesY = 3;
    public float spacing = 2f;
    private GameObject[] games;

    void Start()
    {
        float gameXSize = LevelData.MapWidth; // Largeur de la grille de jeu
        float gameYSize = LevelData.MapHeight; // Hauteur de la grille de jeu

        gamePrefab.SetActive(false); // Désactive le prefab original

        games = new GameObject[numberOfGamesX * numberOfGamesY];
        for (int i = 0; i < numberOfGamesX * numberOfGamesY; i++)
        {
            int x = i % numberOfGamesX;
            int y = i / numberOfGamesX;
            Vector3 position = new Vector3(x * (gameXSize + spacing), y * (gameYSize + spacing), 0);
            games[i] = Instantiate(gamePrefab, position, Quaternion.identity);
            games[i].name = "Game_" + (i + 1);
            games[i].SetActive(true); // Active le jeu pour qu'il puisse être vu et joué
        }

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
