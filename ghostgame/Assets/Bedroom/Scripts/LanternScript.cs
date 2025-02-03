using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LanternScript : AbstractLight
{
    CircleCollider2D lightCollider;

    protected override void Awake()
    {
        base.Awake();
        lightCollider = lightBox.GetComponent<CircleCollider2D>();
        lightCollider.radius = light.pointLightOuterRadius;
    }

    protected override void ToggleLight(GameObject gameObject)
    {
        base.ToggleLight(gameObject);
        if (gameObject == this.gameObject)
        {
            lightCollider.enabled = light.enabled;
        }
    }
}
