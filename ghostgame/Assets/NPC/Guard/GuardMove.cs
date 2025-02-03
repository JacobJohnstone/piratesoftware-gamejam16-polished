using UnityEngine;
using Pathfinding;
using System.Collections;
using UnityEngine.Rendering.Universal;

public class GuardMove : AbstractNPC
{
    public Vector2 velocity;
    private AILerp aiLerp;
    [SerializeField]
    Transform targetOne;
    [SerializeField]
    Transform targetTwo;
    Transform mostRecentDestination;
    Animator anim;
    SpriteRenderer spriteRenderer;
    public bool isAngry;
    BoxCollider2D collider;
    Rigidbody2D rigidBody2d;
    Light2D light;
    FlashLight flashLightDamage;

    Coroutine runningCoroutine;

    public enum StateMachine
    {
        Patrol,
        Chase,
        Scared,
        Dead
    }

    public StateMachine currentState;

    private void Awake()
    {
        aiLerp = GetComponent<AILerp>();
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rigidBody2d = GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        base.Start();
        // State and destination
        currentState = StateMachine.Patrol;
        SetGuardDestination(targetOne);
        mostRecentDestination = targetOne;
        
        // Sanity/flashlight
        light = gameObject.GetComponentInChildren<Light2D>();
        flashLightDamage = gameObject.GetComponentInChildren<FlashLight>();
        collider = GetComponent<BoxCollider2D>();
    }


    protected override void Update()
    {
        base.Update();

        if(0 < sanity && sanity <= 15)
        {
            currentState = StateMachine.Scared;
        } else if(sanity <= 0)
        {
            currentState = StateMachine.Dead;
        }

        // Checking if in patrol/angry state, if can see ghost, then chase
        if (currentState != StateMachine.Scared && currentState != StateMachine.Dead)
        {

            if (SeeGhost(5f))
            {
                currentState = StateMachine.Chase;
            }
            else
            {
                currentState = StateMachine.Patrol;
            }
        }

        switch (currentState)
        {
            case StateMachine.Patrol:
                Patrol();
                break;
            case StateMachine.Chase:
                Chase();
                break;
            case StateMachine.Scared:
                Scared();
                break;
            case StateMachine.Dead:
                Dead();
                break;
        }

        // Used for flashligh direction
        velocity = aiLerp.velocity;


        // Animation/sprite changing
        anim.SetFloat("speed", aiLerp.velocity.magnitude);
        if (aiLerp.velocity.x < 0f)
        {
            spriteRenderer.flipX = false;
        } else
        {
            spriteRenderer.flipX = true;
        }
    }

    void Patrol()
    {
        Patrolling();
        anim.SetBool("isAngry", false);
        isAngry = false;
    }

    void Chase()
    {
        isAngry = true;
        SetGuardDestination(ghost.transform);
        anim.SetBool("isAngry", true);
    }

    void Scared()
    {
        Patrolling();
        isAngry = false;
        anim.SetBool("isAngry", false);
        anim.SetBool("isScared", true);
        light.enabled = false;
        flashLightDamage.enabled = false;
    }

    protected override void Dead()
    {
        anim.SetBool("isScared", false);
        anim.SetBool("isDead", true);
        aiLerp.canMove = false;
        collider.enabled = false;
        rigidBody2d.constraints = RigidbodyConstraints2D.FreezePosition;
        if(runningCoroutine == null)
        {
            runningCoroutine = StartCoroutine(DestroyDead());
        }
    }

    IEnumerator DestroyDead()
    {
        yield return new WaitForSeconds(1f);
        GameEvents.instance.NpcDead(gameObject);
        Destroy(gameObject);
    }

    void Patrolling()
    {
        if (aiLerp.destination != targetOne.position && aiLerp.destination != targetTwo.position)
        {
            SetGuardDestination(mostRecentDestination);
        }

        if (aiLerp.destination == targetOne.position && aiLerp.reachedDestination)
        {
            SetGuardDestination(targetTwo);
            mostRecentDestination = targetTwo;
        }
        else if (aiLerp.reachedDestination)
        {
            SetGuardDestination(targetOne);
            mostRecentDestination = targetOne;
        }
    }

    private void SetGuardDestination(Transform target)
    {
        aiLerp.destination = target.position;
    }

}
