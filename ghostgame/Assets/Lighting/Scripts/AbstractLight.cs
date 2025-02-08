using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class AbstractLight : MonoBehaviour
{

    protected Light2D light;
    [SerializeField]
    protected GameObject lightBox;
    protected List<GameObject> npcList;

    protected virtual void Awake()
    {
        light = GetComponent<Light2D>();
        npcList = new List<GameObject>();
    }

    protected void Start()
    {
        GameEvents.instance.onInteract += ToggleLight;
        GameEvents.instance.onNPCInteract += LightOn;
    }

    protected void OnDestroy()
    {
        GameEvents.instance.onInteract -= ToggleLight;
        GameEvents.instance.onNPCInteract -= LightOn;
    }

    protected virtual void LightOn(GameObject gameObject)
    {
        if (gameObject == this.gameObject)
        {
            light.enabled = true;
        }
    }

    protected virtual void ToggleLight(GameObject gameObject)
    {
        if (gameObject == this.gameObject)
        {
            light.enabled = !light.enabled;
            NpcIntoDarkness();
        }
    }


    /* 
     * For each npc in the light, since disabling the trigger does not invoke the onTriggerExit2D function we have to create a new
     * function to invoke called OnTriggerDestroy() within the NPC abstract class and call for any NPC within the light when we destroy
     * this trigger
     */
    protected void NpcIntoDarkness()
    {
        if (!light.enabled && npcList.Count > 0)
        {
            foreach (GameObject npc in npcList)
            {
                FlightNPC npcScript = npc.GetComponent<FlightNPC>();
                npcScript.OnTriggerDestroy();
            }
            npcList.Clear();
        }
    }

    protected void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "NPC")
        {
            npcList.Add(collision.gameObject);
        }
    }

    protected void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "NPC")
        {
            npcList.Remove(collision.gameObject);
        }
    }

}
