using UnityEngine;

public class GhostBehavior : MonoBehaviour
{
    [SerializeField] GhostBase ghost;
    [SerializeField] private float DefDuration = 5f;
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
    //Enable for a duration
    public virtual void Enable(float duration)
    {
        this.enabled= true;

        //If new duration is set, reset the timer
        CancelInvoke();
        Invoke(nameof(Disable), duration);
    }

    public virtual void  Disable()
    {
        this.enabled = false;
        CancelInvoke();
    }


    GhostBase GetGhostBase()
    {
        if (ghost == null)
        {
            ghost = GetComponent<GhostBase>();
        }
        return ghost;
    }

    private void set(GhostBase ghost)
    {
        this.ghost = ghost;
    }
}
