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
    [SerializeField] Transform Pacman;

    void Awake()
    {
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<GhostChase>();
        this.frightened = GetComponent<GhostFrightened>();
    }    

    // Update is called once per frame
    void Start()
    {
        ResetState();
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
            transform.position = LevelGenerator.GridToWorld(LevelData.HomeDoorPosition.x, -LevelData.HomeDoorPosition.y, -0.1f);
        }
        else
        {
            ResetPosition();
        }

        if (initialBehavior != null)
        {
            Debug.Log("Enabling initial behavior: " + initialBehavior.GetType().Name);

            initialBehavior.Enable();
        }
    }

    public void ResetPosition() { 
        Vector2Int pos = LevelData.GhostHomePositions[this.tag];
        transform.position = LevelGenerator.GridToWorld(pos.x, -pos.y, -0.1f);
    }
}
