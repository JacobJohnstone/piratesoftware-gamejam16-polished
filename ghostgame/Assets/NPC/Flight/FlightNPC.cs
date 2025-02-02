using UnityEngine;
using Pathfinding;
using System.Collections;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

public class FlightNPC : AbstractNPC
{
    // ------ Components ---------
    SpriteRenderer spriteRenderer;
    Animator animator;

    // ------- Interact ----------
    GameObject interactableObject;
    float interactCooldown;

    // ------ Pathfinding --------
    AIPath path;
    [SerializeField]
    Transform[] targets;
    GameObject[] npcObjects;
    GameObject closestNPC = null;
    Vector3 wanderTarget;
    bool inChase = true;
    bool pathFinished = false;
    bool idleComplete = true;
    bool onPath = false;
    float maxMoveSpeed = 1;
    int targetIndex = 0;

    // ---------- Audio -----------
    AudioSource source;
    [SerializeField]
    AudioClip clip;

    // ------- Death Sprite -------
    [SerializeField]
    GameObject deadPrefab;

    // -------- NPC States --------
    public enum StateMachine
    {
        Wander,
        Pathing,
        InChase,
        Dead
    }

    public StateMachine currentState;

    protected override void Start()
    {
        // Parent Start()
        base.Start();
        
        // --------- Components ---------
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        path = GetComponent<AIPath>();
        source = GetComponent<AudioSource>();

        // --------- GameObjects ---------
        npcObjects = GameObject.FindGameObjectsWithTag("NPC");

        // ----------  Interact  ---------
        interactCooldown = 0;

        // -----------  Event  -----------
        GameEvents.instance.onNPCDestroy += onNPCDeath;
    }

    private void OnDestroy()
    {
        GameEvents.instance.onNPCDestroy -= onNPCDeath;
    }

    private void Update()
    {
        // Update the Interact cooldown
        interactCooldown -= Time.deltaTime;


        // ------------------- Manage State ----------------------------

        if (sanity <= 0)
        {
            currentState = StateMachine.Dead;
        }
        else if (currentState != StateMachine.Dead && SeeGhost(4f))
        {
            // If the chase has just started, play the scream audio
            if(currentState != StateMachine.InChase)
            {
                // Play Audio
                source.PlayOneShot(clip);
            }
            // Enter chase state
            currentState = StateMachine.InChase;
        } else if (onPath)
        {
            currentState = StateMachine.Pathing;
        } else
        {
            currentState = StateMachine.Wander;
        }

        switch (currentState)
        {
            case StateMachine.Wander:
                Wander();
                break;
            case StateMachine.InChase:
                InChase();
                break;
            case StateMachine.Pathing:
                Pathing();
                break;
            case StateMachine.Dead:
                Dead();
                break;
        }


        // ------------------- Manage Sanity ----------------------------

        // Increase sanity if near another resident
        findClosestNPC();
        if (closestNPC != null && calcDistance(closestNPC.transform.position) < 5)
        {
            ChangeSanity(10f * Time.deltaTime);
        }

        // Change sanity according to being in our out of the darkness
        if (inDarkness)
        {
            ChangeSanity(-30f * Time.deltaTime);
        }
        else
        {
            ChangeSanity(40f * Time.deltaTime);
        }

        // ------------ Movement Animation -----------------
        if (path.velocity.magnitude > 0)
        {
            animator.SetBool("Moving", true);
        }
        else
        {
            animator.SetBool("Moving", false);
        }

        // Direction
        if (path.targetDirection.x < 0)
        /*its obsolete but its the only attribute of aipath that works for this so hopefully this doesnt break
        desired velocity can't return negative and steeringtarget doesnt update immediately*/
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }

        animator.SetFloat("Sanity", sanity / 100);
    }

    private void Wander()
    {

        // If NPC is currently
        if (idleComplete)
        {
            // Pick a random spot to wander to if high on sanity or last npc
            if (sanity / 100 >= .5 || closestNPC == null)
            {
                wanderTarget = Random.insideUnitSphere * 6;
                wanderTarget.y = 0;
                wanderTarget += transform.position;
            }
            // Attempt to roam to closest NPC to increase sanity
            else
            {
                wanderTarget = closestNPC.transform.position;
                wanderTarget.x += (Random.Range(0, 2) * 2 - 1) * 3;
                wanderTarget.y += (Random.Range(0, 2) * 2 - 1) * 3;
            }

            // If you can interact with an object in the area, do so
            if (interactCooldown <= 0 && interactableObject != null)
            {
                interactCooldown = 1;
                GameEvents.instance.Interact(interactableObject);
            }

            // Reset the idleComplete flag for next check
            idleComplete = false;
        }
        else
        {
            // Start walking to a new random target
            path.destination = wanderTarget;

            // If NPC reaches target, the path is finished and stand still for 5s (startIdle coroutine)
            if (path.velocity.magnitude == 0 && !pathFinished)
            {
                pathFinished = true;
                StartCoroutine(startIdle());
            }
        }

        // Update NPC Animation
        animator.SetBool("Running", false);
    }

    private void Pathing()
    {
        // Go to next target (next room assignment)
        path.destination = targets[targetIndex].position;
        // Go back to roaming after reaching room
        if (path.velocity.magnitude == 0)
        {
            onPath = false;
        }

        // Update NPC Animation
        animator.SetBool("Running", false);
    }

    private void InChase()
    {
        // Increase speed while being chased
        path.maxSpeed = maxMoveSpeed + (sanity / 100) + 0.5f;
        
        // Look to run towards the nearest guard
        GameObject closestGuard = findClosestGuard();
        if (closestGuard != null)
        {
            path.destination = closestGuard.transform.position;
        }
        // If there are no guards alive, run the opposite direction of the ghost
        else
        {
            Vector3 playerVector = ghost.transform.position - transform.position;
            playerVector.x = playerVector.x * -1;
            playerVector.y = playerVector.y * -1;
            path.destination = playerVector + transform.position;
        }

        // Update NPC animation
        animator.SetBool("Running", true);
    }

    protected override void Dead()
    {
        Instantiate(deadPrefab, transform.position, Quaternion.identity);
        GameEvents.instance.NpcDead(gameObject);
        Destroy(gameObject);
    }

    IEnumerator startIdle()
    {
        yield return new WaitForSeconds(5f);
        idleComplete = true;
        pathFinished = false;
    }

    // Called by clock after reaching next hour
    public void changePath() 
    {
        // Increment index but keep in bounds
        targetIndex += 1;
        if (targetIndex >= targets.Length) { targetIndex = 0; }
        onPath = true;
    }

    private float calcDistance(Vector3 point)
    {
        Vector3 distanceVector = point - transform.position;
        return distanceVector.magnitude;
    }

    private void findClosestNPC()
    {
        float distance = 100000;
        foreach (GameObject npc in npcObjects)
        {
            if (npc != null)
            {
                float tempDistance = calcDistance(npc.transform.position);
                if (tempDistance < distance && tempDistance != 0)
                {
                    distance = tempDistance;
                    closestNPC = npc;
                }
            }
        }
    }

    private GameObject findClosestGuard() 
    {
        float distance = 100000;
        GameObject closestGuard = null;
        foreach (GameObject npc in npcObjects)
        {
            if (npc != null)
            {
                GuardMove guard = npc.GetComponent<GuardMove>();
                if (guard != null)
                {
                    float tempDistance = calcDistance(npc.transform.position);
                    if (tempDistance < distance && tempDistance != 0)
                    {
                        distance = tempDistance;
                        closestGuard = npc;
                    }
                }
            }
        }

        return closestGuard;
    }

    private void onNPCDeath(GameObject gameObject)
    {
        for (int i = 0; i < npcObjects.Length; i++)
        {

            if (npcObjects[i] == gameObject)
            {
                npcObjects[i] = null;
            }

        }
    }

}
