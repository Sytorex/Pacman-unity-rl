using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class PacmanAgent : Agent
{
    public float moveSpeed = 5f;
    public LevelGenerator levelGenerator;
    public LayerMask wallLayer;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private int nextAction = 0;
    private bool isPowerUpActive = false;
    private Vector3 currentMoveDir = Vector3.zero;
    private bool isReady = false;


    public override void OnEpisodeBegin()
    {
        // Reset le level
        levelGenerator.ResetLevel();

        nextAction = 0;
        currentMoveDir = Vector3.zero;

        Vector2Int startPos = LevelData.PacmanStartPosition;
        transform.position = LevelGenerator.GridToWorld(startPos.x, -startPos.y);
        targetPosition = transform.position;

        isMoving = false;
    }

    void Start()
    {
        Invoke("SetReady", 0.1f);
    }
    void SetReady() { isReady = true; }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActions = actionsOut.DiscreteActions;

        // On capture la nouvelle direction, mais on ne l'applique pas encore forcément
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) nextAction = 1;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) nextAction = 2;
        else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) nextAction = 3;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) nextAction = 4;

        discreteActions[0] = nextAction;
    }
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady) return;

        int action = actions.DiscreteActions[0];
        Vector3 wantedDir = Vector3.zero;
        float rotation = 0f;
        if (action == 1) { wantedDir = Vector3.up; rotation = 90f; }
        else if (action == 2) { wantedDir = Vector3.down; rotation = -90f; }
        else if (action == 3) { wantedDir = Vector3.left; rotation = 180f; }
        else if (action == 4) { wantedDir = Vector3.right; rotation = 0f; }

        
        if (wantedDir != Vector3.zero && !Physics2D.Raycast(transform.position, wantedDir, 1f, wallLayer))
        {
            currentMoveDir = wantedDir;
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        if (currentMoveDir != Vector3.zero)
        {
            if (!Physics2D.Raycast(transform.position, currentMoveDir, 1f, wallLayer))
            {
                targetPosition = transform.position + currentMoveDir;
                StartCoroutine(SmoothMove());
            }
            else
            {
                currentMoveDir = Vector3.zero;
                AddReward(-0.01f);
            }
        }
    }

    System.Collections.IEnumerator SmoothMove()
    {
        isMoving = true;
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPosition;
        isMoving = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("pacman_pellet") == true)
        {
            AddReward(10f);
            other.gameObject.SetActive(false);
        } else if (other.CompareTag("pacman_power_pellet") == true)
        {
            AddReward(50f);
            other.gameObject.SetActive(false);
            isPowerUpActive = true;
            Invoke("DeactivatePowerUp", 8f);

            GhostBase[] ghosts = FindObjectsByType<GhostBase>(FindObjectsSortMode.None);

            foreach (GhostBase ghost in ghosts)
            {
                if (!ghost.home.enabled)
                {
                    ghost.frightened.Enable(8f);
                }
            }
        }
         else if (other.CompareTag("Clyde")==true || other.CompareTag("Blinky")==true || other.CompareTag("Inky")==true || other.CompareTag("Pinky")==true)
        {
            if (isPowerUpActive)
            {
                AddReward(200f);
                other.GetComponent<GhostBehavior>().GetComponent<GhostFrightened>().Eaten();
            }
            else
            {
                Debug.Log("Collision avec un fantôme !");
                AddReward(-100f);
                EndEpisode();
            }
        }
    }

    private void DeactivatePowerUp()
    {
        isPowerUpActive = false;
    }

/*
    public override void CollectObservations(VectorSensor sensor)
    {
       
        sensor.AddObservation(transform.position.x / LevelData.Map.GetLength(1));
        sensor.AddObservation(Mathf.Abs(transform.position.y) / LevelData.Map.GetLength(0));

        
        for (int y = 0; y < LevelData.Map.GetLength(0); y++)
        {
            for (int x = 0; x < LevelData.Map.GetLength(1); x++)
            {
                sensor.AddObservation(LevelData.Map[y, x] / 4f); // Normalisé (0 à 1)
            }
        }

        
        foreach (GameObject pellet in levelGenerator.allPellets)
        {
            sensor.AddObservation(pellet.activeSelf ? 1f : 0f);
        }

        foreach (GameObject ghost in levelGenerator.spawnedGhosts)
        {
            sensor.AddObservation(ghost.transform.position.x / LevelData.Map.GetLength(1));
            sensor.AddObservation(Mathf.Abs(ghost.transform.position.y) / LevelData.Map.GetLength(0));
        }
    }
    */
}
