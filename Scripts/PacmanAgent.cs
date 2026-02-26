using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using TMPro;

public class PacmanAgent : Agent
{
    public float moveSpeed = 5f;
    public int score = 0;
    [SerializeField] private TextMeshProUGUI scoreText;
    private Vector3 targetPosition;
    private bool isMoving = false;
    private int nextAction = 0;
    private bool isPowerUpActive = false;

    public override void OnEpisodeBegin()
    {
        FindObjectOfType<LevelGenerator>().ResetLevel();

        nextAction = 0;
        currentMoveDir = Vector3.zero;

        Vector2Int startPos = LevelData.PacmanStartPosition;
        transform.position = LevelData.GridToWorld(startPos.x, startPos.y);

        targetPosition = transform.position;
        isMoving = false;
    }

    // Optionnel : Désactive l'agent pendant 0.1s au réveil pour laisser la physique s'installer
    private bool isReady = false;
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
    private Vector3 currentMoveDir = Vector3.zero;
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady) return;

        int action = actions.DiscreteActions[0];
        Vector3 wantedDir = Vector3.zero;
        float rotation = 0f;
        if (action == 1) { wantedDir = Vector3.forward; rotation = 0f; }
        else if (action == 2) { wantedDir = Vector3.back; rotation = 180f; }
        else if (action == 3) { wantedDir = Vector3.left; rotation = 270f; }
        else if (action == 4) { wantedDir = Vector3.right; rotation = 90f; }

        
        if (wantedDir != Vector3.zero && CanMoveTo(transform.position + wantedDir))
        {
            currentMoveDir = wantedDir;
            transform.eulerAngles = new Vector3(0, rotation, 0);
        }

        if (currentMoveDir != Vector3.zero)
        {
            if (CanMoveTo(transform.position + currentMoveDir))
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

    private bool CanMoveTo(Vector3 worldPos)
    {
        if (!LevelData.TryWorldToGrid(worldPos, out int x, out int z)) return false;

        return LevelData.IsWalkable(x, z, allowGhostHouseDoor: false);
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
        HandlePickupsAndGhostCollisions();
        isMoving = false;
    }

    private void HandlePickupsAndGhostCollisions()
    {
        LevelGenerator levelGenerator = FindObjectOfType<LevelGenerator>();
        if (levelGenerator != null)
        {
            foreach (GameObject pellet in levelGenerator.allPellets)
            {
                if (pellet == null || !pellet.activeSelf) continue;

                if (Vector3.Distance(transform.position, pellet.transform.position) > 0.2f) continue;

                if (pellet.CompareTag("pacman_pellet"))
                {
                    AddReward(10f);
                    AddScore(10);
                    pellet.SetActive(false);
                }
                else if (pellet.CompareTag("pacman_power_pellet"))
                {
                    AddReward(50f);
                    AddScore(50);
                    pellet.SetActive(false);
                    isPowerUpActive = true;
                    Invoke("DeactivatePowerUp", 8f);

                    GhostBase[] allGhosts = FindObjectsOfType<GhostBase>();

                    foreach (GhostBase ghost in allGhosts)
                    {
                        if (!ghost.home.enabled)
                        {
                            ghost.frightened.Enable(8f);
                        }
                    }
                }
            }
        }

        GhostBehavior[] ghostBehaviors = FindObjectsOfType<GhostBehavior>();
        foreach (GhostBehavior ghostBehavior in ghostBehaviors)
        {
            if (!ghostBehavior.enabled) continue;
            if (Vector3.Distance(transform.position, ghostBehavior.transform.position) > 0.35f) continue;

            GhostFrightened frightened = ghostBehavior.GetComponent<GhostFrightened>();
            if (isPowerUpActive && frightened != null && frightened.enabled)
            {
                AddReward(200f);
                AddScore(200);
                frightened.Eaten();
            }
            else
            {
                Debug.Log("Collision avec un fantôme !");
                AddReward(-100f);
                AddScore(-100);
                EndEpisode();
            }

            break;
        }
    }

    private void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null) {
            scoreText.text = "Score: " + score.ToString();
        }
    }

    private void DeactivatePowerUp()
    {
        isPowerUpActive = false;
    }
}
