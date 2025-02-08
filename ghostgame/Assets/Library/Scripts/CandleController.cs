using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;


public class CandleController : AbstractLight
{
    Animator anim;
    CircleCollider2D lightCollider;

    protected override void Awake()
    {
        base.Awake();
        anim = GetComponent<Animator>();
        lightCollider = lightBox.GetComponent<CircleCollider2D>();
        lightCollider.radius = light.pointLightOuterRadius;
    }

    protected override void LightOn(GameObject gameObject)
    {
        base.LightOn(gameObject);
        if(gameObject == this.gameObject)
        {
            anim.SetBool("isFlame", light.enabled);
            lightCollider.enabled = light.enabled;
        }
    }

    protected override void ToggleLight(GameObject gameObject)
    {
        base.ToggleLight(gameObject);
        if (gameObject == this.gameObject)
        {
            anim.SetBool("isFlame", light.enabled);
            lightCollider.enabled = light.enabled;
        }
    }

}
