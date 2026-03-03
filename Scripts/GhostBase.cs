using TMPro;
using UnityEngine;

public class GhostBase : MonoBehaviour
{
    public GhostHome home;
    public GhostScatter scatter;
    public GhostChase chase;
    public GhostFrightened frightened;
    public bool isBlinky = false;

    public GhostBehavior initialBehavior;

    void Awake()
    {
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<GhostChase>();
        this.frightened = GetComponent<GhostFrightened>();
    }    

    public void ResetState()
    {
        gameObject.SetActive(true);

        home.enabled = false;
        scatter.enabled = false;
        chase.enabled = false;
        frightened.enabled = false;

        if (isBlinky)
        {
            transform.localPosition = LevelGenerator.GridToWorld(LevelData.HomeDoorPosition.x, -LevelData.HomeDoorPosition.y, LevelGenerator.GhostZLayer);
        }
        else
        {
            ResetPosition();
        }

        if (initialBehavior != null)
        {
            initialBehavior.Enable();
        }
    }

    public void ResetPosition() { 
        Vector2Int pos = LevelData.GhostHomePositions[this.tag];
        transform.localPosition = LevelGenerator.GridToWorld(pos.x, -pos.y, LevelGenerator.GhostZLayer);
    }
}
