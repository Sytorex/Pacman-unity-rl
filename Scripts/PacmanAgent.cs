using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class PacmanAgent : Agent
{
    public float moveSpeed = 5f;
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
        var discreteActions = actionsOut.DiscreteActions;
        // On force une direction aléatoire constante tant qu'on ne bouge pas
        if (!isMoving)
        {
            discreteActions[0] = Random.Range(0, 4); // 0=Up, 1=Down, 2=Left, 3=Right
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (isMoving || !isReady ) return;

        int action = actions.DiscreteActions[0];
        Vector3 dir = Vector3.zero;
        float rotation = 0f;

        if (action == 0) { dir = Vector3.up; rotation = 90f; }
        else if (action == 1) { dir = Vector3.down; rotation = -90f; }
        else if (action == 2) { dir = Vector3.left; rotation = 180f; }
        else if (action == 3) { dir = Vector3.right; rotation = 0f; }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, wallLayer);

        if (hit.collider == null)
        {
            targetPosition = transform.position + dir;
            transform.eulerAngles = new Vector3(0, 0, rotation);
            StartCoroutine(SmoothMove());
        }
        else
        {
            Debug.Log("Mur détecté en : " + dir);
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
}
