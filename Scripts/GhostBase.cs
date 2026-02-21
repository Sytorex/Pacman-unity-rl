using UnityEngine;

public class GhostBase : MonoBehaviour
{
    
    public int points = 200;
    [SerializeField] private string GhostName;

    [SerializeField] GhostHome home;
    [SerializeField] GhostScatter scatter;
    [SerializeField] GhostChase chase;
    [SerializeField] GhostFrightened frightened;

    [SerializeField] GhostBehavior initialBehavior;
    [SerializeField] Transform Pacman;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        this.home = GetComponent<GhostHome>();
        this.scatter = GetComponent<GhostScatter>();
        this.chase = GetComponent<GhostChase>();
        this.frightened = GetComponent<GhostFrightened>();
        this.GhostName = gameObject.name;

        //color of the ghost based on its name
        //switch (GhostName.ToLower())
        //{
        //    case "blinky":
        //        GetComponent<SpriteRenderer>().color = Color.red;
        //        break;
        //    case "pinky":
        //        GetComponent<SpriteRenderer>().color = Color.magenta;
        //        break;
        //    case "inky":
        //        GetComponent<SpriteRenderer>().color = Color.cyan;
        //        break;
        //    case "clyde":
        //        GetComponent<SpriteRenderer>().color = new Color(1f, 0.5f, 0f); // Orange
        //        break;
        //     default:
        //        break;
        //}
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
