using System.Collections;
using UnityEngine;

public class FlashLight : MonoBehaviour
{
    [SerializeField]
    GameObject parent;
    [SerializeField]
    GameObject ghost;

    GuardMove guard;
    bool angry;
    LayerMask wallLayer;
    float timePassed = 0f;
    float hitInterval = 1f;

    private void Awake()
    {
        guard = parent.GetComponent<GuardMove>();
        wallLayer = LayerMask.GetMask("Obstacle");
    }


    void Update()
    {
        Vector2 direction = Vector2.zero;
        angry = guard.isAngry;
        if (angry) {
            direction = (ghost.transform.position - transform.position).normalized;
        } else
        {
            direction = guard.velocity.normalized;
        }
        // Flashlight angle
        float angle = Mathf.Atan2 (-1 * direction.x, direction.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // Track time to only call damage once every second
        timePassed += Time.deltaTime;


        // Call player damage event on raycast hit
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 4f, wallLayer);
        if(hit && hit.collider.tag == "Player" && timePassed >= hitInterval)
        {
            timePassed = 0f;
            GameEvents.instance.TakeDamage();
        }

    }

}
