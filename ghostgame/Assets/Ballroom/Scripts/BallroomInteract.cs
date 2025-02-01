using UnityEngine;

public class BallroomInteract : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D plate;
    [SerializeField]
    Rigidbody2D wine;
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

        if (gameObject == this.gameObject)
        {

            float option = Random.Range(0, 5);
            if(option > 2.5f)
            {
                Rigidbody2D plateInstance;
                plateInstance = Instantiate(plate, this.gameObject.transform) as Rigidbody2D;
                plateInstance.AddForce(throwDirection * 550f);
            } else
            {
                Rigidbody2D wineInstance;
                wineInstance = Instantiate(wine, this.gameObject.transform) as Rigidbody2D;
                wineInstance.AddForce(throwDirection * 550f);
            }

            // Enable hit trigger, disable it 1s later through update function
            hitCollider.enabled = true;
            timePassed = 0f;

        }

    }
}
