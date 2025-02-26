using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
public class UIManager : MonoBehaviour
{
    [SerializeField]
    GameObject instructionMenu;
    bool menuActive;

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
        GameEvents.instance.onMenu += CloseMenu;

        instructionMenu.SetActive(true);
        menuActive = true;

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
        GameEvents.instance.onMenu -= CloseMenu;
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
        float finishTime = (Mathf.Round(timePassed * 100)) / 100;
        yourTime.text = "YOUR TIME: " + (int)finishTime/60 + ":" + ((int)finishTime%60).ToString("00");
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
        SceneManager.LoadScene("MainScene");
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

    void CloseMenu()
    {
        instructionMenu.SetActive(!menuActive);
        menuActive = !menuActive;
    }

}
