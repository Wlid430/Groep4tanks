using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnTriggerEnter(Collider collision) //Als je de triggerzone in gaat, wat het raakt is Collider, wordt doorgestuurd naar collision
    {
        var hit = collision.gameObject; // hit is een variable met de gameObject van collison
        var hitPlayer = hit.GetComponent<PlayerMove>(); // hitPlayer is een variable met de script "PlayerMove" van de gameObject
        if (hitPlayer != null) // Als het de script "PlayerMove" vindt, doe de onderstaande. Anders is de variable leeg en slaat het de if over
        {
            var combat = hit.GetComponent<Combat>(); // combat is een variable met de script "Combat" van de gameObject
            combat.TakeDamage(20); // Roep de TakeDamage functie uit met 20 damage van de "Combat" script

           
        }
        Destroy(gameObject); // Vernietig de kogel
    }

}