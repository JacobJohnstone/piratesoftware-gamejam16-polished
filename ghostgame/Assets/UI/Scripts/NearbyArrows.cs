using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NearbyArrows : MonoBehaviour
{
    // A reference to all the npcs, populating a dictionary when there is only 3 npcs left
    List<GameObject> npcs;
    GameObject[] npcArr;
    Dictionary<GameObject, int> npcDic;

    GameObject ghost;

    // Arrow Images
    [SerializeField]
    RectTransform arrow1;
    [SerializeField]
    RectTransform arrow2;
    [SerializeField]
    RectTransform arrow3;
    RectTransform[] arrowObjects;

    // Arrow Images
    Image arrowOne;
    Image arrowTwo;
    Image arrowThree;
    // List of arrows
    Image[] arrows;

    // Used to ensure only one population of the dictionary occurs
    bool dicPopulated;

    void Start()
    {
        // Event
        GameEvents.instance.onNPCDestroy += RemoveNPC;

        ghost = GameObject.FindGameObjectWithTag("Player");

        arrowOne = arrow1.GetComponent<Image>();
        arrowTwo = arrow2.GetComponent<Image>();
        arrowThree = arrow3.GetComponent<Image>();

        arrowObjects = new RectTransform[3];
        arrowObjects[0] = arrow1;
        arrowObjects[1] = arrow2;
        arrowObjects[2] = arrow3;

        // Adding the arrow images to the list
        arrows = new Image[3];
        arrows[0] = arrowOne;
        arrows[1] = arrowTwo;
        arrows[2] = arrowThree;

        // Disabling all arrows at the start of the game, since they only need to be populated once the game has only 3 npcs left
        arrowOne.enabled = false;
        arrowTwo.enabled = false;
        arrowThree.enabled = false;
        dicPopulated = false;

        // NPC Tracking
        npcs = new List<GameObject>();
        npcArr = GameObject.FindGameObjectsWithTag("NPC");
        PopulateList(npcArr);
    }

    private void OnDestroy()
    {
        GameEvents.instance.onNPCDestroy -= RemoveNPC;
    }

    void Update()
    {
        // If there is only 3 npcs left or less
        if (dicPopulated)
        {
            // for each npc left, point the arrow in their direction each frame
            foreach (GameObject npc in npcs)
            {
                // get the direction from the player to the npc
                Vector2 direction = ghost.transform.position - npc.transform.position;

                // if the npc is more than 5 units away, enable the arrow
                if(direction.magnitude > 5)
                {
                    arrows[npcDic[npc]].enabled = true;

                    // set the position of the arrow 3 units away from the player in the direction of the npc
                    arrowObjects[npcDic[npc]].localPosition = direction.normalized * -150;

                    // calculate the arrow's rotation based on the direction to ensure it's actually pointing away from the player
                    float angle = Mathf.Atan2(direction.x, -1 * direction.y) * Mathf.Rad2Deg;
                    arrowObjects[npcDic[npc]].localRotation = Quaternion.Euler(0, 0, angle + 90);
                } else
                {
                    // If the npc is within 5 units, disable the arrow because they are on screen
                    arrows[npcDic[npc]].enabled = false;
                }
            }
        }
    }

    // Use the array from the start method to populate the list of npcs
    void PopulateList(GameObject[] array)
    {
        foreach (GameObject npc in array) 
        { 
            npcs.Add(npc);
        }
    }

    // When an npc dies it calls an event (passing itself as a gameobject) before it destroys itself, this function runs on the event call
    void RemoveNPC(GameObject npc)
    {
        if (npcs.Contains(npc))
        {
            npcs.Remove(npc);
        }
        else
        {
            Debug.Log("Removal Failed");
        }

        // If the list now only contains 3 or less npcs
        if (npcs.Count <= 3)
        {
            // if the dictionary has not yet been populated
            if (!dicPopulated)
            {
                PopulateDic(npcs);
                dicPopulated = true;
            } else
            {
                if (npcDic.ContainsKey(npc))
                {
                    // disable the arrow for the npc that just called the event and remove it from the dictionary
                    arrows[npcDic[npc]].enabled = false;
                    npcDic.Remove(npc);
                }
            }
        }
    }

    void PopulateDic(List<GameObject> npcList)
    {
        int i = 0;
        npcDic = new Dictionary<GameObject, int>();

        foreach (GameObject npc in npcList)
        {
            npcDic.Add(npc, i);
            i++;
        }
    }

}
