using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    GameObject ghost;

    GuardMove guard;
    LayerMask obstacleLayer;
    Vector2 direction;
    float timePassed = 0f;
    float hitInterval = 1f;

    private void Awake()
    {
        guard = parent.GetComponent<GuardMove>();
        ghost = GameObject.FindGameObjectWithTag("Player");
        obstacleLayer = LayerMask.GetMask("Obstacle");
    }


    void Update()
    {
        if (guard.isAngry) {
            direction = (ghost.transform.position + new Vector3(0.08838356f, 0.1350673f, 0) - transform.position).normalized;
        } else
        {
            direction = guard.velocity.normalized;
        }
        // Flashlight angle
        float angle = Mathf.Atan2 (-1 * direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Track time to only call damage once every second
        timePassed += Time.deltaTime;

        RaycastHit2D playerHit = Physics2D.Linecast(transform.position, ghost.transform.position + new Vector3(0.08838356f, 0.1350673f, 0), obstacleLayer);

        if (playerHit)
        {
            if (playerHit.collider.tag == "Player" && playerHit.distance <= 4f && timePassed >= hitInterval && !playerHit.collider.isTrigger)
            {
                timePassed = 0f;
                GameEvents.instance.TakeDamage();
            }
        }
    }
}
