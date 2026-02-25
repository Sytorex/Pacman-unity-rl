using System.Collections.Generic;
using UnityEngine;

public class GhostFrightened : GhostBehavior
{
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private SpriteRenderer eyes;
    [SerializeField] private SpriteRenderer blue;
    [SerializeField] private SpriteRenderer white;

    bool isChase = false;
    public bool eaten { get; private set; }

    public override void Enable(float duration)
    {
        base.Enable(duration);
        eaten = false;
        body.enabled = false;
        eyes.enabled = false;
        blue.enabled = true;
        white.enabled = false;
        //if (this.ghost.chase.enabled) isChase = true;
        //this.ghost.chase.Disable();
        //this.ghost.scatter.Disable();
        if(this.ghost.frightened.enabled)this.ghost.home.Enable(duration + 1f); // Ensure if ghost is home it stays home for the entire frightened duration
        Invoke(nameof(Flash), duration * 0.5f); // Flash halfway through the frightened duration
    }

    public override void Disable()
    {
        base.Disable();
        eaten = false;
        body.enabled = true;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;

        //if (isChase) this.ghost.chase.Enable();
        //else this.ghost.scatter.Enable();
    }

    private void Flash()
    {
        if (!eaten)
        {
            blue.enabled = false;
            white.enabled = true;
        }
    }

    private void OnEnable()
    {
        speed = 2f; // Reduce speed when frightened
        this.eaten = false; // Reset eaten state when frightened mode starts
        if (this.ghost.chase.enabled) isChase = true;
        else isChase = false;
        //this.ghost.chase.Disable(); // Ensure chase mode is off when frightened
        //this.ghost.scatter.Disable(); // Ensure scatter mode is off when frightened
    }

    private void OnDisable()
    {
        speed = 4f; // Reset speed when not frightened
        this.eaten = false;
        if (isChase) this.ghost.chase.Enable();
        else this.ghost.scatter.Enable();
    }

    public void Eaten()
    {
        eaten = true;
        body.enabled = false;
        eyes.enabled = true;
        blue.enabled = false;
        white.enabled = false;
        this.ghost.ResetPosition(); // Teleport back to the ghost house
        this.ghost.home.Enable(8f);
    }

    private void Update()
    {
        if (!enabled || eaten) return;
        if (!isMoving)
        {
            ChooseNextMove();
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

    void ChooseNextMove()
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
