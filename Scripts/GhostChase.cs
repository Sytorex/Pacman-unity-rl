using System.Collections.Generic;
using UnityEngine;

public class GhostChase : GhostBehavior
{
    string ghostTag;
    private Transform pacmanTransform;
    private Transform blinkyTransform;

    private void OnDisable()
    {
        Debug.Log("disabling Chase on " + this.tag);
        if(this.ghost.frightened.enabled) return; // If the ghost is frightened, we don't want to enable scatter or chase
        this.ghost.scatter.Enable();
    }

    private void OnEnable()
    {
        // set the ghost's chase behavior based on its tag
        ghostTag = this.tag;
        Debug.Log("Chase enabled for " + ghost);

        //We will need to always know where pacman is
        GameObject pacman = GameObject.FindGameObjectWithTag("pacman_player");
        if (pacman != null) pacmanTransform = pacman.transform;

        GameObject blinky = GameObject.FindGameObjectWithTag("Blinky");
        if (blinky != null) blinkyTransform = blinky.transform;

        targetPosition = LevelGenerator.GridToWorld(
            transform.position.x,
            transform.position.y
        );
        transform.position = targetPosition;

    }

    private void Update()
    {
        if (!isMoving)
        {
            if (ghostTag == "Blinky")
                ChooseNextMoveBlinky();
            else if (ghostTag == "Pinky")
                ChooseNextMovePinky();
            else if (ghostTag == "Inky")
                ChooseNextMoveInky();
            else if (ghostTag == "Clyde")
                ChooseNextMoveClyde();
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

    void ChooseNextMoveBlinky()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
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
            if (CanGhostMoveTo(potentialStep))
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

    void ChooseNextMovePinky()
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
            currentTargetGoal = pacmanPos + (pacmanForward * 4);
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
            if (CanGhostMoveTo(potentialStep))
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

    void ChooseNextMoveInky()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;
        Vector3 targetGoal;

        if (pacmanTransform != null && blinkyTransform != null)
        {
            
            Vector3 pacmanDir = pacmanTransform.right;

            Vector3 pivotPoint = pacmanTransform.position + (pacmanDir * 2);

            Vector3 blinkyToPivot = pivotPoint - blinkyTransform.position;

            targetGoal = blinkyTransform.position + (blinkyToPivot * 2);

            Debug.DrawLine(blinkyTransform.position, targetGoal, Color.cyan);
            Debug.DrawLine(pacmanTransform.position, pivotPoint, Color.yellow);
        }
        else
        {
            targetGoal = transform.position;
        }

        foreach (Vector3 dir in directions)
        {
            if (dir == -lastDirection) continue;

            Vector3 potentialStep = transform.position + dir;

            if (CanGhostMoveTo(potentialStep))
            {
                if (Vector3.Distance(potentialStep, Vector3.zero) < 0.5f) continue;

                float dist = Vector3.Distance(potentialStep, targetGoal);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    bestDirection = dir;
                }
            }
        }

        if (bestDirection == Vector3.zero) bestDirection = -lastDirection;

        lastDirection = bestDirection;
        targetPosition = transform.position + bestDirection;
        isMoving = true;
    }

    void ChooseNextMoveClyde()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;
        Vector3 targetGoal;

        if (pacmanTransform != null)
        {
            // 1. Calculer la distance actuelle entre Clyde et Pac-Man
            float distanceToPacman = Vector3.Distance(transform.position, pacmanTransform.position);

            // 2. Déterminer la cible selon la distance (8 cases est le standard)
            if (distanceToPacman > 8f)
            {
                // Clyde est loin : il cible directement Pac-Man
                targetGoal = pacmanTransform.position;
            }
            else
            {
                
                targetGoal = new Vector3(1.5f, -25.5f, 0);
            }
        }
        else
        {
            targetGoal = transform.position;
        }

        foreach (Vector3 dir in directions)
        {
            if (dir == -lastDirection) continue;

            Vector3 potentialStep = transform.position + dir;

            if (CanGhostMoveTo(potentialStep))
            {
                float dist = Vector3.Distance(potentialStep, targetGoal);
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
}

