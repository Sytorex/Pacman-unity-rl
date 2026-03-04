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
    //private float lastNearestPelletDistance = 0f;
    private int stepsSinceLastPellet = 0;
    private const int MaxStepsWithoutPellet = 150;
    private const int maxStepCount = 1500;
    
    // private const int VisionRadius = 2; // 11x11 grid (2*2+1)
    // private const int VisionSize = VisionRadius * 2 + 1;
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
        //lastNearestPelletDistance = GetNearestPelletDistance(transform.localPosition);
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

   // ╔══════════════════════════════════════════════════════════════╗
    // ║  MODIF #2 — WriteDiscreteActionMask RÉACTIVÉ                ║
    // ║  Empêche l'agent de choisir une direction bloquée par un    ║
    // ║  mur. Réduit l'exploration inutile et accélère              ║
    // ║  l'apprentissage dès les premières itérations.              ║
    // ╚══════════════════════════════════════════════════════════════╝
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
        if (stepsSinceLastPellet > MaxStepsWithoutPellet)
        {
            AddReward(-5f);
            EndEpisode();
            return;
        }
        if (CountStep >= maxStepCount) // proche du Max Step = 3000
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
            // float angle = Mathf.Atan2(wantedDir.y, wantedDir.x) * Mathf.Rad2Deg;
            transform.eulerAngles = new Vector3(0, 0, rotation);
        }

        if (currentMoveDir != Vector3.zero)
        {
            if (!Physics2D.Raycast(transform.localPosition, currentMoveDir, 1f, wallLayer))
            {
                targetPosition = transform.localPosition + currentMoveDir;
                isMoving = true;

                 
                /*float newNearestPelletDistance = GetNearestPelletDistance(targetPosition);
                if (newNearestPelletDistance < lastNearestPelletDistance)
                {
                    AddReward(0.001f);
                }
                else if (newNearestPelletDistance > lastNearestPelletDistance)
                {
                    AddReward(-0.0002f);
                }*/

                //lastNearestPelletDistance = newNearestPelletDistance;
                
                // stepsSinceLastPellet++;
                // if (stepsSinceLastPellet > MaxStepsWithoutPellet)
                // {
                //     AddReward(-5f);
                //     EndEpisode();
                //     return;
                // }
                // if (CountStep >= maxStepCount) // proche du Max Step = 3000
                // {
                //     AddReward(-5f);
                //     EndEpisode();
                //     return;
                // }
            }
            else
            {
                currentMoveDir = Vector3.zero;
               // AddReward(-0.01f);
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

                // OPTIONNEL : Demander une décision immédiatement après être arrivé
                // RequestDecision(); 
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("pacman_pellet") == true)
        {
            //HandlePelletCollected(other.gameObject, 10f, 10);
            HandlePelletCollected(other.gameObject, 2f, 10);
        } else if (other.CompareTag("pacman_power_pellet") == true)
        {
            //if (HandlePelletCollected(other.gameObject, 50f, 50))
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
                //AddReward(150f);
                AddReward(16f);
                score += multiplierScore * 200;
                multiplierScore *= 2;
                other.GetComponent<GhostBehavior>().GetComponent<GhostFrightened>().Eaten();
            }
            else
            {
                //AddReward(-200f);
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
        // lastNearestPelletDistance = GetNearestPelletDistance(transform.localPosition);

        bool allEaten = pellets.TrueForAll(p => !p.activeSelf);
        if (allEaten)
        {
            Debug.Log($"All pellets eaten in {CountStep} steps!");
            //float WinReward = 1000f - (CountStep - 200);
            //AddReward(Mathf.Max(100f, WinReward)); 
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

    /// <summary>
    /// Converts a world position to map grid coordinates (x = column, y = row).
    /// </summary>
    private static Vector2Int WorldToMapCoords(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPos.x), -Mathf.FloorToInt(worldPos.y));
    }

// public override void CollectObservations(VectorSensor sensor)
//     {
//         int mapHeight = LevelData.MapHeight; // 11
//         int mapWidth = LevelData.MapWidth;   // 21

//         // 1. POSITION DE PAC-MAN (2 obs)
//         // CRUCIAL : Puisque la carte est globale, l'agent doit savoir où IL se trouve dessus
//         Vector2Int pacMap = WorldToMapCoords(transform.localPosition);
//         sensor.AddObservation((float)pacMap.x / mapWidth);
//         sensor.AddObservation((float)pacMap.y / mapHeight);

//         // 2. LA GRILLE ENTIÈRE (11 * 21 = 231 obs)
//         // On parcourt la grille de [0,0] à [mapHeight, mapWidth]
//         for (int y = 0; y < mapHeight; y++)
//         {
//             for (int x = 0; x < mapWidth; x++)
//             {
//                 TileType cell = (TileType)LevelData.Map[y, x];
//                 if (cell == TileType.Wall) 
//                 {
//                     sensor.AddObservation(0.33f);
//                 }
//                 else 
//                 {
//                     sensor.AddObservation(0f); // Vide ou déjà mangé
//                 }
//             }
//         }
//         // 3. POSITION DES PELLETS DISSOCIés DES MURS
//         // for (int y = 0; y < mapHeight; y++)
//         // {
//         //     for (int x = 0; x < mapWidth; x++)
//         //     {
//         //         TileType cell = (TileType)LevelData.Map[y, x];
//         //         if (cell == TileType.Pellet && IsPelletActive(x, y)) 
//         //         {
//         //             sensor.AddObservation(0.5f);
//         //         }
//         //         else if (cell == TileType.PowerPellet && IsPelletActive(x, y)) 
//         //         {
//         //             sensor.AddObservation(1f);
//         //         }
//         //         else 
//         //         {
//         //             sensor.AddObservation(0f); // Vide ou déjà mangé
//         //         }
//         //     }
//         // }


//         // 3. FANTÔMES (Position absolue normalisée + état de peur : 4 x 3 = 12 obs)
//         for (int i = 0; i < 4; i++)
//         {
//             if (i < ghosts.Count && ghosts[i] != null)
//             {
//                 GameObject ghost = ghosts[i];
//                 Vector2Int ghostMap = WorldToMapCoords(ghost.transform.localPosition);
                
//                 // Position absolue du fantôme sur la carte
//                 sensor.AddObservation((float)ghostMap.x / mapWidth);
//                 sensor.AddObservation((float)ghostMap.y / mapHeight);

//                 GhostFrightened frightened = ghost.GetComponent<GhostFrightened>();
//                 sensor.AddObservation((frightened != null && frightened.enabled) ? 1f : 0f);
//             }
//             else
//             {
//                 sensor.AddObservation(0f);
//                 sensor.AddObservation(0f);
//                 sensor.AddObservation(0f);
//             }
//         }

//         // 4. DIRECTION ACTUELLE (2 obs)
//         sensor.AddObservation(currentMoveDir.x); 
//         sensor.AddObservation(currentMoveDir.y);

//         // 5. INFOS GLOBALES (1 obs)
//         sensor.AddObservation(GetPowerUpObservation());

//         // 6. DIRECTION VERS LE PELLET LE PLUS PROCHE (2 obs) relative to pacman, normalized
//         Vector2 nearestPelletDir = GetDirectionToNearestPellet();
//         sensor.AddObservation(nearestPelletDir.x);
//         sensor.AddObservation(nearestPelletDir.y);
//     }

public override void CollectObservations(VectorSensor sensor)
    {
        int mapHeight = LevelData.MapHeight;
        int mapWidth  = LevelData.MapWidth;
        Vector2Int pacMap = WorldToMapCoords(transform.localPosition);

        // --- 1. Position normalisée de Pac-Man (2 obs) ---
        sensor.AddObservation((float)pacMap.x / mapWidth);
        sensor.AddObservation((float)pacMap.y / mapHeight);

        // --- 2. Grille locale 5×5 centrée sur Pac-Man (25 obs) ---
        // MODIF #9a : rayon 2 (5×5) au lieu de la carte entière (11×21)
        // MODIF #9b : les pellets actifs sont maintenant encodés (0.66 / 1.0)
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

        // --- 3. Fantômes : position relative normalisée + état peur (4 × 3 = 12 obs) ---
        // MODIF #9c : position RELATIVE (par rapport à Pac-Man) au lieu d'absolue
        // → l'agent raisonne plus facilement en termes de "le fantôme est à droite"
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

        // --- 4. Direction actuelle de Pac-Man (2 obs) ---
        sensor.AddObservation(currentMoveDir.x);
        sensor.AddObservation(currentMoveDir.y);

        // --- 5. Power-up restant normalisé (1 obs) ---
        sensor.AddObservation(GetPowerUpObservation());

        // 6. Direction + DISTANCE vers le pellet le plus proche (3 obs au lieu de 2)
        Vector2 nearestPelletDir = GetDirectionToNearestPellet();
        sensor.AddObservation(nearestPelletDir.x);
        sensor.AddObservation(nearestPelletDir.y);

        // NOUVEAU : distance normalisée — crucial pour savoir si le pellet est loin
        float nearestDist = GetNearestPelletDistance(transform.localPosition);
        float maxDist = Mathf.Sqrt(LevelData.MapWidth * LevelData.MapWidth 
                                + LevelData.MapHeight * LevelData.MapHeight);
        sensor.AddObservation(nearestDist / maxDist);

        // 7. Pellets restants normalisés (1 obs)
        int activePellets = pellets.FindAll(p => p != null && p.activeSelf).Count;
        sensor.AddObservation((float)activePellets / pellets.Count);

        // Total : 2 + 81 + 12 + 2 + 1 + 3 + 1 = 102 observations
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

    /*
    public override void CollectObservations(VectorSensor sensor)
    {
        int mapHeight = LevelData.MapHeight;
        int mapWidth = LevelData.MapWidth;

        // Pacman position in map coordinates
        Vector2Int pacMap = WorldToMapCoords(transform.localPosition);

        // === Local vision grid (11x11 = 121 observations) ===
        // Values: 0 = empty/path, 0.33 = wall, 0.66 = pellet, 1.0 = power pellet
        for (int dy = -VisionRadius; dy <= VisionRadius; dy++)
        {
            for (int dx = -VisionRadius; dx <= VisionRadius; dx++)
            {
                int mx = pacMap.x + dx;
                int my = pacMap.y + dy;

                if (mx < 0 || mx >= mapWidth || my < 0 || my >= mapHeight)
                {
                    sensor.AddObservation(0.33f); // Out of bounds → wall
                }
                else
                {
                    TileType cell = (TileType)LevelData.Map[my, mx];

                    if (cell == TileType.Wall)
                    {
                        sensor.AddObservation(0.33f);
                    }
                    else if (cell == TileType.Pellet || cell == TileType.PowerPellet)
                    {
                        if (pelletByGrid.TryGetValue(new Vector2Int(mx, my), out GameObject pellet)
                            && pellet != null && pellet.activeSelf)
                        {
                            sensor.AddObservation(cell == TileType.PowerPellet ? 1f : 0.66f);
                        }
                        else
                        {
                            sensor.AddObservation(0f); // Eaten
                        }
                    }
                    else
                    {
                        sensor.AddObservation(0f); // Empty / door
                    }
                }
            }
        }   

        // === Ghost observations: relative position + frightened (4 × 3 = 12 obs) ===
        for (int i = 0; i < 4; i++)
        {
            if (i >= ghosts.Count || ghosts[i] == null)
            {
                Debug.LogWarning($"Ghost index {i} is out of bounds or null.");
                sensor.AddObservation(0f); // relative x
                sensor.AddObservation(0f); // relative y
                sensor.AddObservation(0f); // frightened
            }
            else
            {
                GameObject ghost = ghosts[i];
                float relX = (ghost.transform.localPosition.x - transform.localPosition.x) / mapWidth;
                float relY = (ghost.transform.localPosition.y - transform.localPosition.y) / mapHeight;
                sensor.AddObservation(relX);
                sensor.AddObservation(relY);

                GhostFrightened frightened = ghost.GetComponent<GhostFrightened>();
                sensor.AddObservation((frightened != null && frightened.enabled) ? 1f : 0f);
            }
        }

        // === Power-up remaining time (1 obs) ===
        sensor.AddObservation(GetPowerUpObservation());

        // === Distance to nearest pellet (1 obs) ===
        float nearestPelletDistance = GetNearestPelletDistance(transform.localPosition) / (mapWidth + mapHeight); // Normalize by max possible distance
        sensor.AddObservation(nearestPelletDistance);

        // Pacman current movement direction (1 obs) - encoded as 0=up, 1=down, 2=left, 3=right
        int moveDir = 0;
        if (currentMoveDir == Vector3.up) moveDir = 0;
        else if (currentMoveDir == Vector3.down) moveDir = 1;
        else if (currentMoveDir == Vector3.left) moveDir = 2;
        else if (currentMoveDir == Vector3.right) moveDir = 3;
        sensor.AddObservation(moveDir / 3f); // Normalize to [0,1]
    }
    */

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
