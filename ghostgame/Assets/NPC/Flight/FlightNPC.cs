using UnityEngine;
using Pathfinding;
using System.Collections;
using UnityEngine.UI;

public class FlightNPC : AbstractNPC
{

    private float interactCooldown = 0;
    GameObject interactableObject;

    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rigidbody2d;

    //Pathfinding
    private bool inChase = true;
    private bool pathFinished = false;
    private bool idle = true;
    private Vector3 wanderTarget;
    private bool onPath = false;
    private AIPath path;
    private float maxMoveSpeed = 1;
    [SerializeField] private Transform[] targets;
    private int targetIndex = 0;

    private GameObject[] npcObjects;
    private GameObject closestNPC = null;


    private AudioSource source;
    [SerializeField] private AudioClip clip;


    [SerializeField] GameObject deadPrefab;
    protected override void Start()
    {
        base.Start();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        path = GetComponent<AIPath>();
        npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        source = GetComponent<AudioSource>();
    }

    private void Update()
    {
        interactCooldown -= Time.deltaTime;

        if (sanity <= 0)
        {
            //die
            Instantiate(deadPrefab, transform.position, Quaternion.identity);
            GameEvents.instance.NpcDead(gameObject);
            Object.Destroy(this.gameObject);
        }

        //check if ghost in vision
        //Vector3 playerPosition = playerObject.transform.position + new Vector3(0,.1f,0);
        //RaycastHit2D playerHit = Physics2D.Raycast(transform.position, playerPosition - transform.position, 8f, playerLayer);
        //RaycastHit2D obstacleHit = Physics2D.Raycast(transform.position, playerPosition - transform.position, 8f, obstacleLayer);

        RaycastHit2D[] hits = SeeGhost();
        RaycastHit2D playerHit = hits[0];
        RaycastHit2D obstacleHit = hits[1];


        if (playerHit && playerHit.collider.tag == "Player" && !playerHit.collider.isTrigger && !obstacleHit)
        {
            if (!inChase)
            {
                //play scream sound
                source.PlayOneShot(clip);
            }
            ChangeSanity(-.05f * Time.deltaTime);
            inChase = true;
        } else if (inChase)
        {
            inChase = false;
            idle = true;
        }

        findClosestNPC();
        //increase sanity if near friends
        if (calcDistance(closestNPC.transform.position) < 5)
        {
            ChangeSanity(.01f * Time.deltaTime);
        }

        //change sanity with lighting
        if (inDarkness)
        {
            ChangeSanity(-.01f * Time.deltaTime);
        }
        else
        {
            ChangeSanity(.01f * Time.deltaTime);
        }

        //--------------------------AI Movement----------------------------
        path.maxSpeed = maxMoveSpeed+ (sanity/100);
        if (inChase)//---------------Chased------------------
        {
            //faster in chase
            path.maxSpeed = maxMoveSpeed + (sanity/100) + .5f;
            //try to find nearest person
            GameObject closestGuard = findClosestGuard();
            if (closestGuard != null)
            {
                path.destination = closestGuard.transform.position;
            } else
            {
                //all guards dead, just try to run away from ghost
                Vector3 playerVector = ghost.transform.position - transform.position;
                playerVector.x = playerVector.x * -1;
                playerVector.y = playerVector.y * -1;
                path.destination = playerVector + transform.position;
            }
            animator.SetBool("Running", true);
        } else if (onPath)//---------------Path to Room------------------
        {
            animator.SetBool("Running", false);
            //go to desired target
            path.destination = targets[targetIndex].position;
            //go back to roaming after reaching room
            if (path.velocity.magnitude == 0)
            {
                onPath = false;
            }
        }
        else//---------------Wandering in room------------------
        {
            animator.SetBool("Running", false);
            if (idle)
            {
                if (sanity/100 >= .5)
                {
                    //pick random point to wander to
                    wanderTarget = Random.insideUnitSphere * 6;
                    wanderTarget.y = 0;
                    wanderTarget += transform.position;
                }
                else //if low on sanity seek closest friend
                {
                    wanderTarget = closestNPC.transform.position;
                    wanderTarget.x += (Random.Range(0, 2) * 2 - 1) * 3;
                    wanderTarget.y += (Random.Range(0, 2) * 2 - 1) * 3;
                }

                idle = false;
                if (interactCooldown <= 0 && interactableObject != null)
                {
                    interactCooldown = 10;
                    GameEvents.instance.Interact(interactableObject);
                }
            } else
            {
                //roam to random point
                path.destination = wanderTarget;
                if (path.velocity.magnitude  == 0 && !pathFinished)
                {
                    pathFinished = true;
                    StartCoroutine(setIdle());
                }
            }
 
        }

        //------------Animation-----------------
        if (path.velocity.magnitude > 0) {
            animator.SetBool("Moving", true);
        } else
        {
            animator.SetBool("Moving", false);
        }

        //direction
        if (path.targetDirection.x < 0) 
            /*its obsolete but its the only attribute of aipath that works for this so hopefully this doesnt break
            desired velocity cant return negative and steeringtarget doesnt update immediately*/
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }

        animator.SetFloat("Sanity", sanity/100);
    }

    IEnumerator setIdle()
    {
        yield return new WaitForSeconds(5f);
        idle = true;
        pathFinished = false;
    }

    //to be called by clock after reaching next hour
    public void changePath() 
    {
        //increment index but keep in bounds
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
            float tempDistance = calcDistance(npc.transform.position);
            if (tempDistance < distance && tempDistance != 0)
            {
                distance = tempDistance;
                closestNPC = npc;
            }
        }
    }

    private GameObject findClosestGuard() 
    {
        float distance = 100000;
        GameObject closestGuard = null;
        foreach (GameObject npc in npcObjects)
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

        return closestGuard;
    }

    //private void ChangeSanity(float change)
    //{
    //    if (sanity + change < 0)
    //    {
    //        sanity = 0;
    //    } else if (sanity + change > 1)
    //    {
    //        sanity = 1;
    //    }
    //    else
    //    {
    //        sanity += change;
    //    }
    //    slider.value = sanity;
    //}
//    private void OnTriggerStay2D(Collider2D collision)
//    {
//        if (collision.gameObject.tag == "Light")
//        {
//            inDarkness = false;
//            interactableObject = collision.gameObject;
//        }
//        if (collision.tag == "Interact")
//        {
//            interactableObject = collision.gameObject;
//        }
//    }

//    private void OnTriggerExit2D(Collider2D collision)
//    {
//        if (collision.gameObject.tag == "Light")
//        {
//            inDarkness = true;
//        }
//    }
}
