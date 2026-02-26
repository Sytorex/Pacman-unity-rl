using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    public GhostBase ghost;
    public float DefDuration = 5f;
    private float disableTime;

    public float speed = 4f;

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
        if (!LevelData.TryWorldToGrid(worldPos, out int x, out int z)) return false;

        return LevelData.IsWalkable(x, z, allowGhostHouseDoor: false);
    }
    public void AddDuration(float extra)
    {
        float remaining = disableTime - Time.time;

        Enable(remaining + extra);
    }
    protected List<Vector3> AvailableDirection(Vector3 worldPos){
        Vector3[] directions = { Vector3.forward, Vector3.back, Vector3.left, Vector3.right };
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
