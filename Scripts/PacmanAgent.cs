using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Collections.Generic;

public class PacmanAgent : Agent
{
    public float moveSpeed = 8f;
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
    private int stepsSinceLastPellet = 0;
    private const int MaxStepsWithoutPellet = 150;
    private const int maxStepCount = 1500;
    
    private int CountStep = 0;


    public override void Initialize()
    {
        pellets = levelGenerator.GenerateLevel();
        Debug.Log($"Generated {pellets.Count} pellets.");
        foreach (GameObject pellet in pellets)
        {
            Vector2Int gridPos = WorldToMapCoords(pellet.transform.localPosition);
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
        CountStep =0;
        nextAction = 0;
        currentMoveDir = Vector3.zero;
        isPowerUpActive = false;
        powerUpEndTime = 0f;
        CancelInvoke(nameof(DeactivatePowerUp));

        Vector2Int startPos = LevelData.PacmanStartPosition;
        transform.localPosition = LevelGenerator.GridToWorld(startPos.x, -startPos.y, LevelGenerator.PacmanZLayer);
        targetPosition = transform.localPosition;
        transform.eulerAngles = Vector3.zero;

        ResetGhostsAndPellets();

        isMoving = false;
        score = 0;
        multiplierScore = 1;
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
    
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady) return;

        stepsSinceLastPellet++;
        bool isHeuristic = actions.DiscreteActions[0] == nextAction;
        if (stepsSinceLastPellet > MaxStepsWithoutPellet && !isHeuristic)
        {
            AddReward(-5f);
            EndEpisode();
            return;
        }
        if (CountStep >= maxStepCount && !isHeuristic) // proche du Max Step = 3000
        {
            AddReward(-5f);
            EndEpisode();
            return;
        }

         Vector2Int currentGrid = WorldToMapCoords(transform.localPosition);
        if (!IsPelletActive(currentGrid.x, currentGrid.y) && stepsSinceLastPellet > 5)
        {
            AddReward(-0.02f); // case vide, pousse à explorer ailleurs
        }

        CountStep++;
        int action = actions.DiscreteActions[0];
        Vector3 wantedDir = Vector3.zero;
        float rotation = 0f;
        if (action == 0) { wantedDir = Vector3.up; rotation = 90f; }
        else if (action == 1) { wantedDir = Vector3.down; rotation = -90f; }
        else if (action == 2) { wantedDir = Vector3.left; rotation = 180f; }
        else if (action == 3) { wantedDir = Vector3.right; rotation = 0f; }

        AddReward(-0.002f);


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
                isMoving = true;
            }
            else
            {
                currentMoveDir = Vector3.zero;
                Debug.Log("Move blocked by wall.");
            }
        }
    }

    

    void Update()
    {
        if (isMoving)
        {
            // On avance vers la cible
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetPosition, moveSpeed * Time.deltaTime);

            // Si on est arrivé (seuil très petit)
            if (Vector3.Distance(transform.localPosition, targetPosition) < 0.001f)
            {
                transform.localPosition = targetPosition; // Snap parfait
                isMoving = false; // On est prêt pour la prochaine Action
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("pacman_pellet") == true)
        {
            HandlePelletCollected(other.gameObject, 2f, 10);
        } else if (other.CompareTag("pacman_power_pellet") == true)
        {
            if (HandlePelletCollected(other.gameObject, 5f, 50))
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
         else if (other.CompareTag("pacman_ghost") == true)
        {
            if (isPowerUpActive)
            {
                AddReward(16f);
                score += multiplierScore * 200;
                multiplierScore *= 2;
                other.GetComponent<GhostBehavior>().GetComponent<GhostFrightened>().Eaten();
            }
            else
            {
                AddReward(-16f);
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

        bool allEaten = pellets.TrueForAll(p => !p.activeSelf);
        if (allEaten)
        {
            Debug.Log($"All pellets eaten in {CountStep} steps!");
            float speedBonus = Mathf.Max(0f, 5f - (CountStep / 500f));
            AddReward(15f + speedBonus);
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

    private static Vector2Int WorldToMapCoords(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), -Mathf.FloorToInt(worldPos.y));
    }

public override void CollectObservations(VectorSensor sensor)
    {
        int mapHeight = LevelData.MapHeight;
        int mapWidth  = LevelData.MapWidth;
        Vector2Int pacMap = WorldToMapCoords(transform.localPosition);

        // 1. Position normalisée de Pac-Man
        sensor.AddObservation((float)pacMap.x / mapWidth);
        sensor.AddObservation((float)pacMap.y / mapHeight);

        // 2. Grille locale
        const int radius = 4;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int mx = pacMap.x + dx;
                int my = pacMap.y + dy;

                if (mx < 0 || mx >= mapWidth || my < 0 || my >= mapHeight)
                {
                    sensor.AddObservation(0.33f); // hors carte → mur
                    continue;
                }

                TileType cell = (TileType)LevelData.Map[my, mx];

                if (cell == TileType.Wall)
                {
                    sensor.AddObservation(0.33f);
                }
                else if ((cell == TileType.Pellet || cell == TileType.PowerPellet)
                         && IsPelletActive(mx, my))
                {
                    sensor.AddObservation(cell == TileType.PowerPellet ? 1f : 0.66f);
                }
                else
                {
                    sensor.AddObservation(0f); // vide ou pellet déjà mangé
                }
            }
        }

        // 3. Fantômes : position relative normalisée + état peur
        for (int i = 0; i < 4; i++)
        {
            if (i < ghosts.Count && ghosts[i] != null)
            {
                float relX = (ghosts[i].transform.localPosition.x - transform.localPosition.x) / mapWidth;
                float relY = (ghosts[i].transform.localPosition.y - transform.localPosition.y) / mapHeight;
                sensor.AddObservation(relX);
                sensor.AddObservation(relY);
                GhostFrightened f = ghosts[i].GetComponent<GhostFrightened>();
                sensor.AddObservation((f != null && f.enabled) ? 1f : 0f);
            }
            else
            {
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
                sensor.AddObservation(0f);
            }
        }

        // 4. Direction actuelle de Pac-Man
        sensor.AddObservation(currentMoveDir.x);
        sensor.AddObservation(currentMoveDir.y);

        // 5. Power-up restant normalisé
        sensor.AddObservation(GetPowerUpObservation());

        // 6. Direction + DISTANCE vers le pellet le plus proche (non normalisée pour la distance, normalisée pour la direction)
        Vector2 nearestPelletDir = GetDirectionToNearestPellet();
        sensor.AddObservation(nearestPelletDir.x);
        sensor.AddObservation(nearestPelletDir.y);

        float nearestDist = GetNearestPelletDistance(transform.localPosition);
        float maxDist = Mathf.Sqrt(LevelData.MapWidth * LevelData.MapWidth 
                                + LevelData.MapHeight * LevelData.MapHeight);
        sensor.AddObservation(nearestDist / maxDist);

        // 7. Pellets restants normalisés (1 obs)
        int activePellets = pellets.FindAll(p => p != null && p.activeSelf).Count;
        sensor.AddObservation((float)activePellets / pellets.Count);
    }

    private bool IsPelletActive(int mx, int my)
    {
        if (pelletByGrid.TryGetValue(new Vector2Int(mx, my), out GameObject pellet))
        {
            return pellet != null && pellet.activeSelf;
        }
        return false;
    }

    private Vector2 GetDirectionToNearestPellet()
    {
        Vector2 nearestDir = Vector2.zero;
        float minDistance = float.MaxValue;

        foreach (GameObject pellet in pellets)
        {
            if (pellet == null || !pellet.activeSelf) continue;

            Vector2 dir = pellet.transform.localPosition - transform.localPosition;
            float distance = dir.magnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestDir = dir.normalized; // Direction normalisée vers le pellet
            }
        }

        return nearestDir;
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

    private GameObject GetNearestPellet(Vector3 fromPosition)
    {
        GameObject nearest = null;
        float minDistance = float.MaxValue;

        foreach (GameObject pellet in pellets)
        {
            if (pellet == null || !pellet.activeSelf) continue;

            float distance = Vector2.Distance(fromPosition, pellet.transform.localPosition);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = pellet;
            }
        }
        return nearest;
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
