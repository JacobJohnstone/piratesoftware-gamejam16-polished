using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public abstract class AbstractNPC : MonoBehaviour
{
    // ------ Ghost Check ------
    protected GameObject ghost;
    LayerMask playerLayer;
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
        playerLayer = LayerMask.GetMask("Player");
        obstacleLayer = LayerMask.GetMask("Obstacle");
    }

    protected RaycastHit2D[] SeeGhost()
    {
        Vector2 direction = ghost.transform.position + new Vector3(0.08838356f, 0.1350673f, 0) - transform.position;

        // Call player damage event on raycast hit
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, direction, 5f, playerLayer);
        RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, direction, 5f, obstacleLayer);

        return new RaycastHit2D[] { playerHit, obstacleHit };

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
