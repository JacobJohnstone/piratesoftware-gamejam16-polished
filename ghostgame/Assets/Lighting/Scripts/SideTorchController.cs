using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SideTorchController : AbstractLight
{
    CircleCollider2D lightCollider;

    protected override void Awake()
    {
        light = GetComponentInChildren<Light2D>();
        lightCollider = lightBox.GetComponent<CircleCollider2D>();
        lightCollider.radius = light.pointLightOuterRadius;
        npcList = new List<GameObject>();
    }

    protected override void ToggleLight(GameObject gameObject)
    {
        if (gameObject == this.gameObject)
        {
            light.enabled = !light.enabled;
            lightCollider.enabled = light.enabled;
            NpcIntoDarkness();
        }
    }
}
