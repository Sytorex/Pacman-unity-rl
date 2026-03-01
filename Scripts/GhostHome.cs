using UnityEngine;

public class GhostHome : GhostBehavior
{
    private void OnEnable()
    {
        this.ghost.ResetPosition();
        this.ghost.scatter.Disable();
        this.ghost.chase.Disable();
        this.ghost.frightened.Disable();
        speed = 0f; // Ghosts in the home do not move
    }

    private void OnDisable()
    {
        speed = 4f; // Reset speed when leaving home

        //Move Ghost outside of Home
        transform.position = LevelGenerator.GridToWorld(LevelData.HomeDoorPosition.x, -LevelData.HomeDoorPosition.y, -0.1f);
        this.ghost.scatter.Enable(); // Enable scatter mode when leaving home
    }
}
