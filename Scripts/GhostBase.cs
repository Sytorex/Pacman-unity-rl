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

    private Vector3 BlinkyInitialPosition = new Vector3(12.5f, LevelData.DefaultLayerY, -11.5f); // Blinky
    private Vector3 PinkyInitialPosition = new Vector3(13.5f, LevelData.DefaultLayerY, -13.5f); // Pinky
    private Vector3 InkyInitialPosition = new Vector3(14.5f, LevelData.DefaultLayerY, -13.5f); // Inky
    private Vector3 ClydeInitialPosition = new Vector3(15.5f, LevelData.DefaultLayerY, -13.5f);  // Clyde


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
        if (this.tag =="Blinky")
        {
            transform.position = BlinkyInitialPosition;
        }
        else if (this.tag == "Pinky")
        {
            transform.position = PinkyInitialPosition;
        }
        else if (this.tag == "Inky")
        {
            transform.position = InkyInitialPosition;
        }
        else if (this.tag =="Clyde")
        {
            transform.position = ClydeInitialPosition;
        }
    }
}
