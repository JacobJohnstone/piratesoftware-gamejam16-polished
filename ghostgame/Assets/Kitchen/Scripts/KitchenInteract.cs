using UnityEngine;

public class KitchenInteract : MonoBehaviour
{
    [SerializeField]
    Rigidbody2D item;
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
        GameEvents.instance.onInteract += ThrowItem;
    }

    private void OnDestroy()
    {
        // unsubscribe
        GameEvents.instance.onInteract -= ThrowItem;
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

    void ThrowItem(GameObject gameObject)
    {
        if (gameObject == this.gameObject) 
        {

            Rigidbody2D plateInstance;
            plateInstance = Instantiate(item, this.gameObject.transform) as Rigidbody2D;
            plateInstance.AddForce(throwDirection * 550f);


            // Enable hit trigger, disable it 1s later through update function for hit box
            hitCollider.enabled = true;
            timePassed = 0f;
        }

    }
}
