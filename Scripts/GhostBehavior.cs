using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public GhostBase ghost;
    public float DefDuration = 5f; // Default duration for behaviors that have a time limit (like Frightened or Home)

    public float speed = 4f;
    private float disableTime;

    protected Vector3 targetPosition;
    protected Vector3 lastDirection;
    protected bool isMoving = false;
    private void Awake()
    {
        if (ghost == null)
        {
            this.ghost = GetComponent<GhostBase>();
            this.enabled = false;
        }
    }

    //Enable Behavior
    public void Enable()
    {
        Enable(DefDuration);
    }
    ////Enable for a duration
    public virtual void Enable(float duration)
    {
        if (this is not GhostHome && ghost.home.enabled) return;
        this.enabled = true;
        disableTime = Time.time + duration;
        //If new duration is set, reset the timer
        CancelInvoke(nameof(Disable));
        CancelInvoke();
        Invoke(nameof(Disable), duration);
    }
    
    public virtual void Disable()
    {
        this.enabled = false;
        CancelInvoke();
    }

    public bool CanGhostMoveTo(Vector3 worldPos)
    {
        // Conversion de la position monde en coordonnées tableau (Inversion du Y comme dans ton Generator)
        int x = Mathf.FloorToInt(worldPos.x);
        int y = Mathf.Abs(Mathf.FloorToInt(worldPos.y));

        if (y >= 0 && y < LevelData.Map.GetLength(0) && x >= 0 && x < LevelData.Map.GetLength(1))
        {
            int cellValue = LevelData.Map[y, x];

            // Clyde peut passer si ce n'est PAS un mur (0) ou la PORTE (4)
            // Il PEUT passer si c'est du vide (1), une pastille (2/3) 
            return cellValue != -1 && cellValue!=3; // 0 représente un mur, 4 représente la porte, les autres sont des espaces traversables
        }
        return false;
    }
    public void AddDuration(float extra)
    {
        float remaining = disableTime - Time.time;

        Enable(remaining + extra);
    }
    protected List<Vector3> AvailableDirection(Vector3 worldPos){
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

        return availableDirections;
    }


}
