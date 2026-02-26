using UnityEngine;

public class GhostScatter : GhostBehavior
{
    private Vector3[] topLeftCorner = { new Vector3(1.5f, -0.5f, 0), new Vector3(1.5f, -5, 0), new Vector3(6.5f,-5,0), new Vector3(6.5f,-0.5f,0)};
    private Vector3[] topRightCorner = { new Vector3(26.5f, -0.5f, 0) , new Vector3(26.5f, -5, 0), new Vector3(21.5f,-5,0), new Vector3(21.5f,-0.5f,0)};
    private Vector3[] bottomLeftCorner = { new Vector3(1.5f, -25.5f, 0), new Vector3(1.5f, 18, 0), new Vector3(12.5f, -25.5f, 0), new Vector3(12.5f, -28.5f, 0) };
    private Vector3[] bottomRightCorner = { new Vector3(26.5f, -28.5f, 0), new Vector3(15.5f, -28.5f, 0), new Vector3(15.5f, -25.5f, 0), new Vector3(26.5f, -25.5f, 0) };

    private Vector3[] scatterTargetList;
    private Vector3 scatterTarget;

    void OnEnable()
    {
        this.ghost.home.Disable();
        Debug.Log("scatter mode on " + this.tag);
        ChooseCorner();
        isMoving = false;
    }

    void Update()
    {
        if (!isMoving)
        {
            ChooseNextMoveScatter();
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                speed * Time.deltaTime
            );

            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                isMoving = false;
                //change target in assigned corner
                scatterTarget = scatterTargetList[Random.Range(0, scatterTargetList.Length)];
            }
        }
    }

    void ChooseCorner()
    {
        if (this.tag == "Blinky")
        {
            scatterTargetList = topRightCorner;
            Debug.Log("Going to top right corner");
        }


        else if (this.tag == "Pinky")
            scatterTargetList = topLeftCorner;

        else if (this.tag == "Inky")
            scatterTargetList = bottomRightCorner;

        else if (this.tag == "Clyde")
            scatterTargetList = bottomLeftCorner;
    }

    void ChooseNextMoveScatter()
    {
        Vector3[] directions = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        Vector3 bestDirection = Vector3.zero;
        float minDistance = float.MaxValue;

        //if we dont see where pacman is, we will just stay afk
        

        foreach (Vector3 dir in directions)
        {
            //A ghost cant do a 180° turn
            if (dir == -lastDirection) continue;

            //check if we can move in this direction by looking at the level data
            Vector3 potentialStep = transform.position + dir;
            if (CanGhostMoveTo(potentialStep))
            {
                //we calculaate the distance from this potential step to pacman, and we want to minimize it
                float dist = Vector3.Distance(potentialStep, scatterTarget);
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
    private void OnDisable()
    {
        Debug.Log("Disable Scatter on "+ this.tag);
        if(this.ghost.frightened.enabled) return; // If the ghost is frightened, we don't want to enable scatter or chase
        this.ghost.chase.Enable();
    }

}
