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
    private float powerUpEndTime = 0f;
    private const float PowerUpDuration = 8f;
    private Vector3 currentMoveDir = Vector3.zero;
    private bool isReady = false;


    public override void OnEpisodeBegin()
    {
        // Reset le level
        levelGenerator.ResetLevel();

        nextAction = 0;
        currentMoveDir = Vector3.zero;
        isPowerUpActive = false;
        powerUpEndTime = 0f;
        CancelInvoke(nameof(DeactivatePowerUp));

        Vector2Int startPos = LevelData.PacmanStartPosition;
        transform.localPosition = LevelGenerator.GridToWorld(startPos.x, -startPos.y, LevelGenerator.PacmanZLayer);
        targetPosition = transform.localPosition;

        transform.eulerAngles = Vector3.zero;

        isMoving = false;
    }

    void Start()
    {
        Invoke("SetReady", 0.5f);
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

        
        if (wantedDir != Vector3.zero && !Physics2D.Raycast(transform.localPosition, wantedDir, 1f, wallLayer))
        {
            currentMoveDir = wantedDir;
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        if (currentMoveDir != Vector3.zero)
        {
            if (!Physics2D.Raycast(transform.localPosition, currentMoveDir, 1f, wallLayer))
            {
                targetPosition = transform.localPosition + currentMoveDir;
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
        while (Vector3.Distance(transform.localPosition, targetPosition) > 0.01f)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.localPosition = targetPosition;
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
            powerUpEndTime = Time.time + PowerUpDuration;
            CancelInvoke(nameof(DeactivatePowerUp));
            Invoke(nameof(DeactivatePowerUp), PowerUpDuration);

            foreach (GameObject ghostObject in levelGenerator.GetSpawnedGhosts())
            {
                if (ghostObject == null) continue;

                GhostBase ghost = ghostObject.GetComponent<GhostBase>();
                if (ghost == null) continue;

                if (!ghost.home.enabled)
                {
                    ghost.frightened.Enable(PowerUpDuration);
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
                AddReward(-100f);
                EndEpisode();
            }
        }
    }

    private void DeactivatePowerUp()
    {
        isPowerUpActive = false;
        powerUpEndTime = 0f;
    }

    private float GetPelletObs(TileType cellValue)
    {
        if (cellValue == TileType.Pellet) return 0.5f;
        else if (cellValue == TileType.PowerPellet) return 1f;
        else return 0f;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Grille de murs (28x31 = 868 observations)
        for (int y = 0; y < LevelData.Map.GetLength(0); y++)
        {
            for (int x = 0; x < LevelData.Map.GetLength(1); x++)
            {
                sensor.AddObservation((TileType)LevelData.Map[y, x] == TileType.Wall ? 1f : 0f);
            }
        }

        // Grille des pellets actifs
        for (int y = 0; y < LevelData.Map.GetLength(0); y++)
        {
            for (int x = 0; x < LevelData.Map.GetLength(1); x++)
            {
                TileType cellValue = (TileType)LevelData.Map[y, x];
                if (cellValue == TileType.Pellet || cellValue == TileType.PowerPellet)
                {
                    // Trouve le pellet correspondant dans la liste des pellets actifs
                    GameObject pellet = levelGenerator.GetAllPellets().Find(p => p.transform.localPosition == LevelGenerator.GridToWorld(x, -y));
                    sensor.AddObservation((pellet != null && pellet.activeSelf) ? GetPelletObs(cellValue) : 0f);
                }
                else
                {
                    sensor.AddObservation(0f);
                }
            }
        }
        
        // Position et état des fantômes
        foreach (GameObject ghost in levelGenerator.GetSpawnedGhosts())
        {
            sensor.AddObservation(ghost.transform.localPosition.x / LevelData.Map.GetLength(1));
            sensor.AddObservation(Mathf.Abs(ghost.transform.localPosition.y) / LevelData.Map.GetLength(0));
            
            GhostFrightened frightened = ghost.GetComponent<GhostFrightened>();
            sensor.AddObservation((frightened != null && frightened.enabled) ? 1f : 0f);
        }

        // Position de Pacman normalisée
        sensor.AddObservation(transform.localPosition.x / LevelData.Map.GetLength(1));
        sensor.AddObservation(Mathf.Abs(transform.localPosition.y) / LevelData.Map.GetLength(0));

        sensor.AddObservation(GetPowerUpObservation());
    }

    private float GetPowerUpObservation()
    {
        if (isPowerUpActive)
        {
            float remainingPowerUpTime = Mathf.Max(0f, powerUpEndTime - Time.time);
            float normalizedRemainingPowerUpTime = remainingPowerUpTime / PowerUpDuration;
            return normalizedRemainingPowerUpTime;
        }
        else
        {
            return 0f;
        }
    }
}
