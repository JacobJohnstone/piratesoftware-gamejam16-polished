using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class WallTorchController : MonoBehaviour
{
    Light2D light;
    [SerializeField]
    GameObject lightBox;
    PolygonCollider2D lightCollider;

    private void Awake()
    {
        light = GetComponentInChildren<Light2D>();
        lightCollider = lightBox.GetComponent<PolygonCollider2D>();
    }

    private void Start()
    {
        GameEvents.instance.onInteract += ToggleLight;
    }

    private void OnDestroy()
    {
        GameEvents.instance.onInteract -= ToggleLight;
    }

    private void ToggleLight(GameObject gameObject)
    {
        if (gameObject == this.gameObject)
        {
            light.enabled = !light.enabled;
            lightCollider.enabled = light.enabled;
        }
    }

}
