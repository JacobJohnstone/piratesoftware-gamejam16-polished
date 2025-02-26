using UnityEngine;
using UnityEngine.UI;

public class Clock : MonoBehaviour
{
    GameObject[] npcObjects;
    Text clockText;
    AudioSource source;
    [SerializeField]
    AudioClip clip;
    float elapsedTime = 72000f;
    float timeScale = 90f;
    float prevHour;

    void Start()
    {
        clockText = GetComponent<Text>();
        source = GetComponent<AudioSource>();
        prevHour = Mathf.FloorToInt(elapsedTime / 3600f);
    }

    void Update()
    {
        //check if time up || totaltime == 36000
        if (Mathf.FloorToInt(elapsedTime / 3600f) == 6)
        {
            GameEvents.instance.LoseGame();
        }

        elapsedTime += Time.deltaTime * timeScale;

        //update text
        int hours = Mathf.FloorToInt(elapsedTime/3600f);
        int minutes = Mathf.FloorToInt((elapsedTime - hours*3600f)/60f);

        if (hours != prevHour) 
        {
            prevHour = hours;
            AlertHourChange();
        }

        //enforce clock cycle
        if(elapsedTime >= 86400f)
        {
            elapsedTime = 0;
        }

        string clockString = string.Format("{0:00}:{1:00}", hours, minutes);
        clockText.text = clockString;
    }

    private void AlertHourChange()
    {
        // Play hour change sound
        source.PlayOneShot(clip);

        // Trigger each npc to seek next room
        npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (GameObject npcObject in npcObjects) 
        {
            FlightNPC npc = npcObject.GetComponent<FlightNPC>();
            if (npc != null)
            {
                npc.changePath();
            }
        }
    }
}
