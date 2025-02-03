using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngineInternal;

public class PlayerMovement : MonoBehaviour
{

    /*
     
        Collisions and ground collisions can't be merged with a composite collider, they need to be max 1 tile in size (tables need to have 4 seperate colliders)
     
     */

    int health;
    bool canTakeDmg;

    // Interact Event parameter set by trigger
    GameObject interactableObject;
    bool onCooldown = false;
    bool invisCooldown = false;

    // Movement
    public InputSystem_Actions inputControls;
    InputAction moveAction;
    Vector2 move;
    Vector2 moveDirection = new Vector2(0, 0);
    [SerializeField]
    float moveSpeed;
    Rigidbody2D rigidBody2d;
    Animator anim;
    bool canMove;

    // Ghost boundaries
    [SerializeField]
    GameObject maxBoundaryObj;
    float xMax, yMax;
    [SerializeField]
    GameObject minBoundaryObj;
    float xMin, yMin;

    // Other Inputs
    InputAction interact;
    InputAction invisibility;

    // Invisibility
    [SerializeField]
    BoxCollider2D boxCollider2d;
    SpriteRenderer spriteRenderer;
    public bool isInvisible;
    LayerMask wallLayer;
    LayerMask collisionObjectsLayer;
    [SerializeField]
    float checkRadius;

    // Sanity Hit for reveal after invis
    [SerializeField]
    GameObject hitBox;

    // Sound
    [SerializeField]
    AudioSource whoomp;
    [SerializeField]
    AudioSource dmg;
    [SerializeField]
    AudioSource whoosh;




    // ----------------- Setup ------------------------------
    void Awake()
    {
        health = 100;
        // Components and Movement
        anim = GetComponent<Animator>();
        inputControls = new InputSystem_Actions();
        rigidBody2d = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Map boundaries set by two empty objects
        xMax = maxBoundaryObj.transform.position.x;
        yMax = maxBoundaryObj.transform.position.y;
        xMin = minBoundaryObj.transform.position.x;
        yMin = minBoundaryObj.transform.position.y;
        // invisibility
        isInvisible = false;
        wallLayer = LayerMask.GetMask("Obstacle");
        collisionObjectsLayer = LayerMask.GetMask("CollisionObjects");
        canMove = true;
        canTakeDmg = true;
    }

    private void Start()
    {
        // Take Damage Event
        GameEvents.instance.onTakingDamage += TakeDamage;
        boxCollider2d.enabled = true;
    }

    private void OnEnable()
    {
        // Movement
        moveAction = inputControls.Player.Move;
        moveAction.Enable();
        // Interact 'E'
        interact = inputControls.Player.Interact;
        interact.Enable();
        interact.performed += Interact;
        // Invisibility 'Q'
        invisibility = inputControls.Player.Ability;
        invisibility.Enable();
        invisibility.performed += Ability;

    }

    private void OnDisable()
    {
        // Interact 'E'
        interact.performed -= Interact;
        interact.Disable();
        // Invisibility 'Q'
        invisibility.performed -= Ability;
        invisibility.Disable();
        // Movement
        moveAction.Disable();
    }

    private void OnDestroy()
    {
        // Take Damage Event
        GameEvents.instance.onTakingDamage -= TakeDamage;
    }

    void Update()
    {

        if (canMove)
        {
            // Movement
            move = moveAction.ReadValue<Vector2>();
            if (!Mathf.Approximately(move.x, 0.0f) || !Mathf.Approximately(move.y, 0.0f))
            {
                moveDirection.Set(move.x, move.y);
                moveDirection.Normalize();
            }

            // Animation Direction
            if (moveDirection.x > 0.0f)
            {
                spriteRenderer.flipX = true;
            }
            else if (moveDirection.x < 0.0f)
            {
                spriteRenderer.flipX = false;
            }
            anim.SetFloat("speed", move.magnitude);
        }
    }

    private void FixedUpdate()
    {

        // Boundary restrictions based on empty object placements
        Vector2 position = new Vector2(
            Mathf.Clamp(rigidBody2d.position.x + move.x * moveSpeed * Time.deltaTime, xMin, xMax),
            Mathf.Clamp(rigidBody2d.position.y + move.y * moveSpeed * Time.deltaTime, yMin, yMax)
            );
        rigidBody2d.MovePosition(position);

    }

    private void TakeDamage()
    {
        if (canTakeDmg) { 
            health -= 10;
            dmg.Play();

            if (health <= 0)
            {
                anim.SetTrigger("death");
                canMove = false;
                canTakeDmg = false;
                rigidBody2d.constraints = RigidbodyConstraints2D.FreezePosition;
                rigidBody2d.constraints = RigidbodyConstraints2D.FreezeRotation;
                rigidBody2d.bodyType = RigidbodyType2D.Static;
                boxCollider2d.enabled = false;
                GameEvents.instance.LoseGame(); // call game over event
            }
        }
    }


    // ----------------------- Interact Event 'E' ------------------------
    private void Interact(InputAction.CallbackContext context)
    {
        if (isInvisible && interactableObject != null && !onCooldown)
        {
            GameEvents.instance.Interact(interactableObject);
            anim.SetTrigger("interact");
            onCooldown = true;
            StartCoroutine(InteractCooldown(1));
        }
    }
    IEnumerator InteractCooldown(float timer)
    {
        yield return new WaitForSeconds(timer);
        onCooldown = false;
    }

    // Sets interactableObject to be sent as the parameter for the event, to be checked by all interactable objects against themselves
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (isInvisible && collision.tag == "Interact")
        {
            interactableObject = collision.gameObject;
            // can add event that instead of disappearing from ui, just adjusts the transparency of the icon part only
            GameEvents.instance.CanInteract();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Interact")
        {
            interactableObject = null;
            GameEvents.instance.CannotInteract();
        }
    }



    // ----------------- Invisibility set to 'Q' -----------------------
    private void Ability(InputAction.CallbackContext context)
    {
        GoInvisible();
    }
    bool inWall()
    {
        bool wallHit = Physics2D.OverlapCircle(transform.position, checkRadius, wallLayer);
        bool objectHit = Physics2D.OverlapCircle(transform.position, checkRadius, collisionObjectsLayer);
        return wallHit || objectHit;
    }

    private void GoInvisible()
    {
        if (!invisCooldown)
        {
            whoomp.Play();
            isInvisible = true;
            TransparencyChange(0.27f);
            ManageColliders(isInvisible);
            spriteRenderer.sortingLayerName = "Invisible";
            invisCooldown = true;
            StartCoroutine(InvisibilityCD());
        }
    }


    IEnumerator RevokeInvisibility()
    {
        if (inWall())
        {
            canMove = false;
            FindPosition();
        }
        yield return new WaitUntil(() => canMove);
        boxCollider2d.enabled = true;
        isInvisible = false;
        TransparencyChange(1f);
        ManageColliders(isInvisible);
        spriteRenderer.sortingLayerName = "Collisions";
        Instantiate(hitBox, gameObject.transform);
        whoosh.Play();
        anim.SetTrigger("boo");
    }

    private void FindPosition()
    {
        // Try to find a nearby position where the player isn't colliding with a wall
        Vector2 newPosition = transform.position;

        // Check in several directions to find an open space
        Vector2[] directions = { 
            Vector2.up, 
            Vector2.down, 
            Vector2.left, 
            Vector2.right,
            Vector2.up + Vector2.right,
            Vector2.up + Vector2.left,
            Vector2.down + Vector2.right,
            Vector2.down + Vector2.left,
        };

        float modifier = 0;
        bool positionFound = false;

        while (modifier < 10)
        {

            foreach (var dir in directions)
            {
                // check for a wall a specified distance away
                bool inWall = Physics2D.OverlapCircle((Vector2)transform.position + (dir * modifier), 0.5f, wallLayer);
                bool inObject = Physics2D.OverlapCircle((Vector2)transform.position + (dir * modifier), 0.5f, collisionObjectsLayer);

                bool inCollider = inWall || inObject;

                if (!inCollider)
                {
                    newPosition = (Vector2)transform.position + dir * modifier;
                    Vector2 clampedPosition = new Vector2(
                        Mathf.Clamp(newPosition.x, xMin, xMax), 
                        Mathf.Clamp(newPosition.y, yMin, yMax)
                        );

                    if (clampedPosition == newPosition)
                    {
                        positionFound = true;
                        break;
                    }
                }
            }

            if (positionFound)
            {
                StartCoroutine(MoveToMap(newPosition));
                break;
            }

            modifier += 0.25f;

        }
    }

    IEnumerator MoveToMap(Vector2 inMapPosition)
    {
        Vector2 startPosition = transform.position;
        float duration = 0.25f;
        float timePassed = 0;

        while(timePassed < duration)
        {
            transform.position = Vector2.Lerp(startPosition, inMapPosition, timePassed / duration);
            timePassed += Time.deltaTime;
            yield return null;
        }

        transform.position = inMapPosition;
        canMove = true;
        boxCollider2d.enabled = true;
    }

    private void TransparencyChange(float transparency)
    {
        Color color = Color.white;
        color.a = transparency;
        spriteRenderer.color = color;
    }

    private void ManageColliders(bool isInvisible)
    {
        //triggerCollider.enabled = isInvisible;
        boxCollider2d.isTrigger = isInvisible;
    }

    IEnumerator InvisibilityCD()
    {
        // call duration event
        GameEvents.instance.StartInvisibility(2f);
        yield return new WaitForSeconds(2f); // invisibility duration
        boxCollider2d.enabled = false;
        StartCoroutine(RevokeInvisibility());
        yield return new WaitUntil(() => !inWall());
        // call cooldown event
        GameEvents.instance.StartCooldown(2f);
        yield return new WaitForSeconds(2f);
        invisCooldown = false;
    }

}
