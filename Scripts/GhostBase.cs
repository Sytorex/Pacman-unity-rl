using TMPro;
using UnityEngine;

public class GhostBase : MonoBehaviour
{
    
    public int points = 200;

    public GhostHome home;
    public GhostScatter scatter;
    public GhostChase chase;
    public GhostFrightened frightened;

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
        chase.enabled = false;
        scatter.enabled = false;
        frightened.enabled = false;

        ResetPosition();

        if (initialBehavior != null)
        {
            Debug.Log("Enabling initial behavior: " + initialBehavior.GetType().Name);

            initialBehavior.Enable();
        }
    }

    public void ResetPosition() { 
        Vector2Int pos = LevelData.GhostStartPositions[this.tag];
        transform.position = LevelGenerator.GridToWorld(pos.x, -pos.y);
    }
}
