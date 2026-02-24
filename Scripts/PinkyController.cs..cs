using UnityEngine;
using System.Collections.Generic;

public class PinkyController : MonoBehaviour
{
    [Header("Réglages")]
    public float speed = 4f;
    public int predictionSteps = 4; //nb de case devant PacMan que Pinky vise

    private Transform pacmanTransform;
    private Vector3 targetPosition;
    private Vector3 lastDirection;
    private bool isMoving = false;

    void Start()
    {
        GameObject pacman = GameObject.FindGameObjectWithTag("pacman_player");
        if(pacman == null)
        {
            Debug.Log("PACMAN NON CHARGE");
        }
        if (pacman != null) pacmanTransform = pacman.transform;

        // Alignement initial sur la grille
        targetPosition = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f,
            0
        );
        transform.position = targetPosition;
    }

    void Update()
    {
        if (!isMoving)
        {
            ChooseNextMove();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                transform.position = targetPosition;
                isMoving = false;
            }
        }
    }

    void ChooseNextMove()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;

        Vector3 currentTargetGoal;

     
        if (pacmanTransform != null)
        {
            //on récupère la position de pacman
            Vector3 pacmanPos = pacmanTransform.position;

            //on récupère la direction de pacman
            Vector3 pacmanForward = pacmanTransform.right;

            // La cible de Pinky est 4 cases devant Pac-Man
            currentTargetGoal = pacmanPos + (pacmanForward * predictionSteps);
        }
        else
        {
            currentTargetGoal = transform.position;
        }
        // On évalue les 4 directions possibles
        foreach (Vector3 dir in directions)
        {
            // Interdiction de faire demi-tour
            if (dir == -lastDirection) continue;

            Vector3 potentialStep = transform.position + dir;
            if (CanMoveTo(potentialStep))
            {
                float dist = Vector3.Distance(potentialStep, currentTargetGoal);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestDirection = dir;
                }
            }
        }

        if (bestDirection == Vector3.zero) bestDirection = -lastDirection;

        if (bestDirection != Vector3.zero)
        {
            lastDirection = bestDirection;
            targetPosition = transform.position + bestDirection;
            isMoving = true;
        }
    }

    bool IsInGhostHouse()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        return (x > 10 && x < 18 && y < -11 && y > -18);
    }

    bool CanMoveTo(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.Abs(Mathf.FloorToInt(worldPos.y));

        if (y >= 0 && y < LevelData.Map.GetLength(0) && x >= 0 && x < LevelData.Map.GetLength(1))
        {
            int cellValue = LevelData.Map[y, x];
            //Blinky can not pass if it's a wall 
            return cellValue != (int)TileType.Wall;
        }
        return false;
    }

    public void ResetPosition()
    {
        Destroy(gameObject);
    }
}
