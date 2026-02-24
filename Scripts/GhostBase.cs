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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        this.gameObject.SetActive(true);
        this.frightened.Disable();
        this.chase.Disable();
        this.scatter.Enable();

        if (this.home!= this.initialBehavior)
        {
            this.home.Disable();
        }
        if(this.initialBehavior != null)
        {
            this.initialBehavior.Enable();
        }
    }


}
