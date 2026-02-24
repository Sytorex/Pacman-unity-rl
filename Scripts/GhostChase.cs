using System.Collections.Generic;
using UnityEngine;

public class GhostChase : GhostBehavior
{
    string ghostTag;
    private Transform pacmanTransform;

    private void OnDisable()
    {
        Debug.Log("disabling Chase on " + this.tag);
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

        //Alignement initial sur la grille
        targetPosition = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f,
            0
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
        // Inky's behavior is more complex and depends on both Pacman's position and Blinky's position
        // Implementation would require tracking Blinky's position and calculating a target based on both
        ChooseNextMoveBlinky(); // Placeholder: You would replace this with the actual logic for Inky
    }

    void ChooseNextMoveClyde()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        List<Vector3> availableDirections = new List<Vector3>();

        foreach (Vector3 dir in directions)
        {
            // 1. Éviter le demi-tour immédiat
            if (dir == -lastDirection && directions.Length > 1) continue;

            // 2. Vérifier la grille de données au lieu de la physique
            if (CanGhostMoveTo(transform.position + dir))
            {
                availableDirections.Add(dir);
            }
        }

        // Cas d'impasse
        if (availableDirections.Count == 0 && lastDirection != Vector3.zero)
            availableDirections.Add(-lastDirection);

        if (availableDirections.Count > 0)
        {
            Vector3 chosenDir = availableDirections[Random.Range(0, availableDirections.Count)];
            lastDirection = chosenDir;
            targetPosition = transform.position + chosenDir;
            isMoving = true;
        }
    }
}
