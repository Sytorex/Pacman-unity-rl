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
            Mathf.Floor(transform.position.x) + LevelData.CellCenterOffset,
            LevelData.DefaultLayerY,
            Mathf.Floor(transform.position.z) + LevelData.CellCenterOffset
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
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;

        Vector3 currentTargetGoal;

     
        if (pacmanTransform != null)
        {
            //on récupère la position de pacman
            Vector3 pacmanPos = pacmanTransform.position;

            //on récupère la direction de pacman
            Vector3 pacmanForward = pacmanTransform.forward;

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
        float z = transform.position.z;
        return (x > 10 && x < 18 && z < -11 && z > -18);
    }

    bool CanMoveTo(Vector3 worldPos)
    {
        if (!LevelData.TryWorldToGrid(worldPos, out int x, out int z)) return false;
        return LevelData.IsWalkable(x, z, allowGhostHouseDoor: true);
    }

    public void ResetPosition()
    {
        Destroy(gameObject);
    }
}
