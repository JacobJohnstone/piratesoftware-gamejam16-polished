using UnityEngine;

public class Bookshelf : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D book;
    [SerializeField]
    Vector2 throwDirection;
    [SerializeField]
    GameObject childObject;
    BoxCollider2D hitCollider;
    float timePassed;

    private void Awake()
    {
        hitCollider = childObject.GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        // subscribe
        GameEvents.instance.onInteract += ThrowBook;

    }

    private void OnDestroy()
    {
        // unsubscribe
        GameEvents.instance.onInteract -= ThrowBook;
    }

    private void Update()
    {
        if (hitCollider.enabled)
        {
            timePassed += Time.deltaTime;
            if (timePassed > 1f)
            {
                hitCollider.enabled = false;
            }
        }
    }

    private void ThrowBook(GameObject gameObject)
    {

        if(gameObject == this.gameObject)
        {
            // Instantiate Book
            Rigidbody2D bookInstance;
            bookInstance = Instantiate(book, this.gameObject.transform) as Rigidbody2D;
            bookInstance.AddForce(throwDirection * 350f);

            // Enable hit trigger, disable it 1s later through update function for hit box
            hitCollider.enabled = true;
            timePassed = 0f;
        }

    }

}
