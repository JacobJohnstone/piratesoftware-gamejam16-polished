using UnityEngine;
using UnityEngine.Rendering.Universal;
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

    protected virtual void Update()
    {
        if (inDarkness)
        {
            ChangeSanity(-2.5f * Time.deltaTime);
        }
        else
        {
            ChangeSanity(2.5f * Time.deltaTime);
        }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Interact object thrown/ghost jumpscare
        if (collision.gameObject.tag == "SanityHit")
        {
            ChangeSanity(-10);
        } else if (collision.gameObject.tag == "Light")
        {
            inDarkness = false;
        }
    }

    protected virtual void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            inDarkness = false;
        }
        else if (collision.gameObject.GetComponent<Light2D>() != null && collision.gameObject.GetComponent<PolygonCollider2D>() == null)
        {
            GameEvents.instance.NPCInteract(collision.gameObject);
        }
    }


    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Light")
        {
            inDarkness = true;
        }
    }

    public void OnTriggerDestroy()
    {
        inDarkness = true;
    }

    protected abstract void Dead();

}
