using UnityEngine;
using System.Collections.Generic;

public class ClydeController : MonoBehaviour
{
    [Header("Réglages")]
    public float speed = 4f;

    private Vector3 targetPosition;
    private Vector3 lastDirection;
    private bool isMoving = false;

    void Start()
    {
        // Alignement initial
        targetPosition = new Vector3(
            Mathf.Floor(transform.position.x) + LevelData.CellCenterOffset,
            LevelData.DefaultLayerY,
            Mathf.Floor(transform.position.z) + LevelData.CellCenterOffset
        );
        transform.position = targetPosition;
    }

    void Update()
    {
        if (!isMoving)
        {
            ChooseNextMove();
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if (Vector3.Distance(transform.position, targetPosition) < 0.001f)
            {
                isMoving = false;
            }
        }
    }

    void ChooseNextMove()
    {
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

    bool CanGhostMoveTo(Vector3 worldPos)
    {
        if (!LevelData.TryWorldToGrid(worldPos, out int x, out int z)) return false;
        return LevelData.IsWalkable(x, z, allowGhostHouseDoor: true);
    }

    public void ResetPosition()
    {
        //destroy itself
        Destroy(gameObject);
    }
}
