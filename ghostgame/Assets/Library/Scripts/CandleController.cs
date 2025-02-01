using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CandleController : MonoBehaviour
{
    Light2D light;
    Animator anim;
    [SerializeField]
    GameObject lightBox;
    CircleCollider2D lightCollider;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        light = GetComponent<Light2D>();
        lightCollider = lightBox.GetComponent<CircleCollider2D>();
        lightCollider.radius = light.pointLightOuterRadius;
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
            anim.SetBool("isFlame", light.enabled);
            lightCollider.enabled = light.enabled;
        }
    }

}
