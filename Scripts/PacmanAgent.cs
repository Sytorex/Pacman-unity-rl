using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class PacmanAgent : Agent
{
    public float moveSpeed = 5f;
    public LevelGenerator levelGenerator;
    public LayerMask wallLayer;
    public GameObject[] ghostObjects; // Blinky, Pinky, Inky, Clyde

    private Vector3 targetPosition;
    private bool isMoving = false;
    private int nextAction = 0;
    private bool isPowerUpActive = false;
    private float powerUpEndTime = 0f;
    private const float PowerUpDuration = 8f;
    private Vector3 currentMoveDir = Vector3.zero;
    private bool isReady = false;
    public int score = 0;
    private int multiplierScore = 1;
    private List<GameObject> pellets = new List<GameObject>();
    private List<GameObject> ghosts = new List<GameObject>();
    private readonly Dictionary<Vector2Int, GameObject> pelletByGrid = new Dictionary<Vector2Int, GameObject>();
    private float lastNearestPelletDistance = 0f;
    private int stepsSinceLastPellet = 0;
    private const int MaxStepsWithoutPellet = 140;

    public override void Initialize()
    {
        pellets = levelGenerator.GenerateLevel();
        Debug.Log($"Generated {pellets.Count} pellets.");
        foreach (GameObject pellet in pellets)
        {
            Vector2Int gridPos = LevelGenerator.WorldToGrid(pellet.transform.localPosition);
            pelletByGrid[gridPos] = pellet;
        }

        foreach (GameObject ghost in ghostObjects)
        {
            if (ghost != null) {
                ghost.GetComponent<GhostBase>().ResetState();
                ghosts.Add(ghost);
            }
        }
    }

    public override void OnEpisodeBegin()
    {
        ResetGhostsAndPellets();        

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
        score = 0;
        multiplierScore = 1;
        lastNearestPelletDistance = GetNearestPelletDistance(transform.localPosition);
        stepsSinceLastPellet = 0;
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
        if (Input.GetKey(KeyCode.Z) || Input.GetKey(KeyCode.UpArrow)) nextAction = 0;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) nextAction = 1;
        else if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.LeftArrow)) nextAction = 2;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) nextAction = 3;

        discreteActions[0] = nextAction;
    }

/*
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        if (!isReady) return;

        Vector3[] dirs = { Vector3.up, Vector3.down, Vector3.left, Vector3.right };
        for (int action = 0; action < dirs.Length; action++)
        {
            bool blocked = Physics2D.Raycast(transform.localPosition, dirs[action], 1f, wallLayer);
            if (blocked)
            {
                actionMask.SetActionEnabled(0, action, false);
            }
        }
    }
*/
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady) return;

        int action = actions.DiscreteActions[0];
        Vector3 wantedDir = Vector3.zero;
        float rotation = 0f;
        if (action == 0) { wantedDir = Vector3.up; rotation = 90f; }
        else if (action == 1) { wantedDir = Vector3.down; rotation = -90f; }
        else if (action == 2) { wantedDir = Vector3.left; rotation = 180f; }
        else if (action == 3) { wantedDir = Vector3.right; rotation = 0f; }

        
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
                
                /*float newNearestPelletDistance = GetNearestPelletDistance(targetPosition);
                if (newNearestPelletDistance < lastNearestPelletDistance)
                {
                    AddReward(0.01f);
                }
                else if (newNearestPelletDistance > lastNearestPelletDistance)
                {
                    AddReward(-0.002f);
                }*/

                // lastNearestPelletDistance = newNearestPelletDistance;
                stepsSinceLastPellet++;
                if (stepsSinceLastPellet > MaxStepsWithoutPellet)
                {
                    AddReward(-1f);
                    EndEpisode();
                    return;
                }
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
            HandlePelletCollected(other.gameObject, 10f, 10);
        } else if (other.CompareTag("pacman_power_pellet") == true)
        {
            if (HandlePelletCollected(other.gameObject, 50f, 50))
            {
                return;
            }

            isPowerUpActive = true;
            powerUpEndTime = Time.time + PowerUpDuration;
            CancelInvoke(nameof(DeactivatePowerUp));
            Invoke(nameof(DeactivatePowerUp), PowerUpDuration);

            foreach (GameObject ghostObject in ghosts)
            {
                if (ghostObject == null) continue;

                GhostBase ghost = ghostObject.GetComponent<GhostBase>();
                if (ghost == null) continue;

                if (!ghost.home.enabled)
                {
                    ghost.frightened.Enable(PowerUpDuration);
                }
                else
                {
                    ghost.home.AddDuration(PowerUpDuration);
                }
            }
        }
         else if (other.CompareTag("Clyde") == true ||
                other.CompareTag("Blinky") == true ||
                other.CompareTag("Inky") == true ||
                other.CompareTag("Pinky") == true)
        {
            if (isPowerUpActive)
            {
                AddReward(200f);
                score += multiplierScore * 200;
                multiplierScore *= 2;
                other.GetComponent<GhostBehavior>().GetComponent<GhostFrightened>().Eaten();
            }
            else
            {
                AddReward(-100f);
                EndEpisode();
            }
        }
    }

    private bool HandlePelletCollected(GameObject pelletObject, float rewardValue, int scoreValue)
    {
        AddReward(rewardValue);
        score += scoreValue;

        stepsSinceLastPellet = 0;
        pelletObject.SetActive(false);
        lastNearestPelletDistance = GetNearestPelletDistance(transform.localPosition);

        bool allEaten = pellets.TrueForAll(p => !p.activeSelf);
        if (allEaten)
        {
            AddReward(500f);
            EndEpisode();
            return true;
        }

        return false;
    }

    private void DeactivatePowerUp()
    {
        multiplierScore = 1;
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
        int mapHeight = LevelData.Map.GetLength(0);
        int mapWidth = LevelData.Map.GetLength(1);

        // Grille de murs (28x31 = 868 observations)
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                sensor.AddObservation((TileType)LevelData.Map[y, x] == TileType.Wall ? 1f : 0f);

            }
        }

        // Grille des pellets actifs
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                TileType cellValue = (TileType)LevelData.Map[y, x];
                if (cellValue == TileType.Pellet || cellValue == TileType.PowerPellet)
                {
                    if (pelletByGrid.TryGetValue(new Vector2Int(x, y), out GameObject pellet) && pellet != null && pellet.activeSelf)
                    {
                        sensor.AddObservation(GetPelletObs(cellValue));
                    }
                    else
                    {
                        sensor.AddObservation(0f);
                    }
                }
                else
                {
                    sensor.AddObservation(0f);
                }
            }
        }
        
        // Position et état des fantômes (4 max, format fixe)
        for (int i = 0; i < 4; i++)
        {
            if (i >= ghosts.Count || ghosts[i] == null)
            {
                Debug.LogWarning($"Ghost index {i} is out of bounds or null. Total ghosts: {ghosts.Count}");
                sensor.AddObservation(0f); // x
                sensor.AddObservation(0f); // y
                sensor.AddObservation(0f); // frightened
            }
            else
            {
                GameObject ghost = ghosts[i];
                sensor.AddObservation(ghost.transform.localPosition.x / mapWidth);
                sensor.AddObservation(Mathf.Abs(ghost.transform.localPosition.y) / mapHeight);
                
                GhostFrightened frightened = ghost.GetComponent<GhostFrightened>();
                sensor.AddObservation((frightened != null && frightened.enabled) ? 1f : 0f);
            }
        }

        // Position de Pacman normalisée
        sensor.AddObservation(transform.localPosition.x / mapWidth);
        sensor.AddObservation(Mathf.Abs(transform.localPosition.y) / mapHeight);

        sensor.AddObservation(GetPowerUpObservation());
    }

    private float GetNearestPelletDistance(Vector3 fromPosition)
    {
        float minDistance = float.MaxValue;

        foreach (GameObject pellet in pellets)
        {
            if (pellet == null || !pellet.activeSelf) continue;

            float distance = Vector2.Distance(fromPosition, pellet.transform.localPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance == float.MaxValue ? 0f : minDistance;
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

    private void ResetGhostsAndPellets()
    {
        // Reset pellets
        foreach (GameObject pellet in pellets)
        {
            if (pellet != null) pellet.SetActive(true);
        }

        // Reset ghosts
        for (int i = 0; i < ghosts.Count; i++)
        {
            if (ghosts[i] != null)
            {
                GhostBase ghostBase = ghosts[i].GetComponent<GhostBase>();
                if (ghostBase != null)
                {
                    ghostBase.ResetState(); 
                }
            }
        }
    }
}
