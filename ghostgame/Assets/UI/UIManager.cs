using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject healthBar;
    [SerializeField]
    GameObject invisibility;
    [SerializeField]
    GameObject interact;
    [SerializeField]
    GameObject menu;
    [SerializeField]
    Text winOrLoseText;
    [SerializeField]
    Text yourTime;
    [SerializeField]
    Text yourScares;
    float timePassed;
    int scareCount;

    private List<GameObject> npcList = new List<GameObject>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npc in npcs) { 
            npcList.Add(npc);
        }

        // gameobject destroyed event
        GameEvents.instance.onLoseGame += Lost;
        GameEvents.instance.onWinGame += Win;
        GameEvents.instance.onNPCDestroy += RemoveNPC;

        healthBar.SetActive(true);
        invisibility.SetActive(true);
        interact.SetActive(true);
        menu.SetActive(false);

        timePassed = 0;
        scareCount = 0;
    }

    private void Update()
    {
        timePassed += Time.deltaTime;
    }

    private void OnDestroy()
    {
        // game object destroyed event
        GameEvents.instance.onLoseGame -= Lost;
        GameEvents.instance.onWinGame -= Win;
        GameEvents.instance.onNPCDestroy -= RemoveNPC;

    }


    void Lost()
    {
        DisableUI();
        winOrLoseText.text = "GAME OVER";
        yourTime.text = "";
        yourScares.text = "SCARE COUNT: " + scareCount.ToString();
    }

    void Win()
    {
        DisableUI();
        winOrLoseText.text = "YOU WON!";
        yourTime.text = "YOUR TIME: " + ((Mathf.Round(timePassed * 100)) / 100);
        yourScares.text = "SCARE COUNT: " + scareCount.ToString();
    }

    void DisableUI()
    {
        healthBar.SetActive(false);
        invisibility.SetActive(false);
        interact.SetActive(false);
        menu.SetActive(true);
    }

    public void RestartBtn()
    {
        //SceneManager.LoadScene("MainScene");
        Debug.Log("Button Working");
    }

    void RemoveNPC(GameObject npc)
    {
        if (npcList.Contains(npc)) { 
            npcList.Remove(npc);
            scareCount++;
        } else
        {
            Debug.Log("Removal Failed");
        }

        if(npcList.Count == 0)
        {
            GameEvents.instance.Winner();
        }
    }

}
