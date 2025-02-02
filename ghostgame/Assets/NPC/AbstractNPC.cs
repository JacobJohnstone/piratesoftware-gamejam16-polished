using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractNPC : MonoBehaviour
{
    // ------ Ghost Check ------
    protected GameObject ghost;
    LayerMask obstacleLayer;

    // -------- Sanity ---------
    protected float sanity;
    protected float maxSanity;
    protected Slider slider;
    protected bool inDarkness;

    protected virtual void Start()
    {
        // -------- Sanity ---------
        inDarkness = true;
        sanity = 100;
        maxSanity = 100;
        slider = gameObject.GetComponentInChildren<Slider>();


        // ------ Ghost Check ------
        ghost = GameObject.FindGameObjectWithTag("Player");
        obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    protected bool SeeGhost(float distance)
    {
        RaycastHit2D playerHit = Physics2D.Linecast(transform.position, ghost.transform.position + new Vector3(0.08838356f, 0.1350673f, 0), obstacleLayer);

        if (playerHit)
        {
            if (playerHit.collider.tag == "Player" && playerHit.distance <= distance && !playerHit.collider.isTrigger)
            {
                return true;
            }
        }

        return false;
    }

    protected void ChangeSanity(float change)
    {
        sanity += change;
        if (sanity > maxSanity)
        {
            sanity = maxSanity;
        }
        slider.value = sanity;
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            inDarkness = false;
        }
    }


    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            inDarkness = true;
        }
    }

}
