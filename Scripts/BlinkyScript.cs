using UnityEngine;
using System.Collections.Generic;

public class BlinkyController : MonoBehaviour
{
    [Header("Réglages")]
    public float speed = 4f;

    private Transform pacmanTransform;
    private Vector3 targetPosition;
    private Vector3 lastDirection;
    private bool isMoving = false;

    void Start()
    {
        //We will need to always know where pacman is
        GameObject pacman = GameObject.FindGameObjectWithTag("pacman_player");
        if (pacman != null) pacmanTransform = pacman.transform;

        //Alignement initial sur la grille
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
                transform.position = targetPosition; // Snap to grid
                isMoving = false;
            }
        }
    }

    void ChooseNextMove()
    {
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;

        //if we dont see where pacman is, we will just stay afk
        Vector3 targetGoal = (pacmanTransform != null) ? pacmanTransform.position : transform.position;

        foreach (Vector3 dir in directions)
        {
            //A ghost cant do a 180° turn
            if (dir == -lastDirection) continue;

            //check if we can move in this direction by looking at the level data
            Vector3 potentialStep = transform.position + dir;
            if (CanMoveTo(potentialStep))
            {
                //we calculaate the distance from this potential step to pacman, and we want to minimize it
                float dist = Vector3.Distance(potentialStep, targetGoal);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestDirection = dir;
                }
            }
        }

        //if we have no valid direction, we will try to do a 180° turn as a last resort
        if (bestDirection == Vector3.zero)
        {
            bestDirection = -lastDirection;
        }

        if (bestDirection != Vector3.zero)
        {
            lastDirection = bestDirection;
            targetPosition = transform.position + bestDirection;
            isMoving = true;
        }
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
