using UnityEngine;

public class GhostHome : GhostBehavior
{
    // On n'ajoute pas "float speed" ici car elle est déjà dans GhostBehavior

    // Position de sortie (Centre de la porte)
    private Vector3 exitPoint = new Vector3(13.5f, -11.5f, 0);

    void Update()
    {
        // On utilise "speed" hérité de GhostBehavior (si elle est public/protected)
        // Sinon, remplace par une valeur fixe comme 4f pour tester
        float moveSpeed = 4f;

        // Déplacement vers la sortie
        transform.position = Vector3.MoveTowards(transform.position, exitPoint, moveSpeed * Time.deltaTime);

        // Si on est arrivé à la porte
        if (Vector3.Distance(transform.position, exitPoint) < 0.1f)
        {
            transform.position = exitPoint;
            this.Disable(); // Désactive GhostHome

            // Active le comportement suivant (Chase)
            GetComponent<GhostChase>().Enable();
        }
    }
}
