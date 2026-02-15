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

    // Assigne le layer "Walls" dans l'inspecteur
    public LayerMask wallLayer;

    public override void Initialize()
    {
        // 1. Aligner Pac-Man parfaitement au centre de la case de départ
        // Ton LevelGenerator place les objets à x+0.5, -y+0.5
        transform.position = new Vector3(
            Mathf.Floor(transform.position.x) + 0.5f,
            Mathf.Floor(transform.position.y) + 0.5f,
            0
        );
        targetPosition = transform.position;
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
        if (isMoving) return;
        var discreteActions = actionsOut.DiscreteActions;
        // discreteActions[0] = Random.Range(1, 5); // 1=Up, 2=Down, 3=Left, 4=Right

        // Contrôle manuel pour les tests
        if (Input.GetKey(KeyCode.UpArrow)) { discreteActions[0] = 1; }
        else if (Input.GetKey(KeyCode.DownArrow)) { discreteActions[0] = 2; }
        else if (Input.GetKey(KeyCode.LeftArrow)) { discreteActions[0] = 3; }
        else if (Input.GetKey(KeyCode.RightArrow)) { discreteActions[0] = 4; }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady || actions.DiscreteActions[0] == 0) return;

        int action = actions.DiscreteActions[0];
        Vector3 dir = Vector3.zero;
        float rotation = 0f;

        if (action == 1) { dir = Vector3.up; rotation = 90f; }
        else if (action == 2) { dir = Vector3.down; rotation = -90f; }
        else if (action == 3) { dir = Vector3.left; rotation = 180f; }
        else if (action == 4) { dir = Vector3.right; rotation = 0f; }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, wallLayer);
        if (hit.collider == null)
        {
            targetPosition = transform.position + dir;
            transform.eulerAngles = new Vector3(0, 0, rotation);
            StartCoroutine(SmoothMove());
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
            AddScore(10);
            other.gameObject.SetActive(false);
        } else if (other.CompareTag("pacman_power_pellet") == true)
        {
            AddReward(50f);
            AddScore(50);
            other.gameObject.SetActive(false);
        }
         else if (other.CompareTag("pacman_ghost") == true)
        {
            AddReward(-100f);
            AddScore(-100);
        }
    }

    private void AddScore(int amount)
    {
        score += amount;
        if (scoreText != null) {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}