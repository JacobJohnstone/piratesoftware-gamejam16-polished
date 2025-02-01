using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InteractCD : MonoBehaviour
{

    [SerializeField]
    private Image imageCD;
    [SerializeField]
    Text textCD;
    [SerializeField]
    private GameObject interactIcon;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        textCD.gameObject.SetActive(false);
        imageCD.fillAmount = 0;
        interactIcon.SetActive(false);
        GameEvents.instance.onInteract += StartCooldown;
        GameEvents.instance.onInRange += CanInteract;
        GameEvents.instance.onOutOfRange += CannotInteract;
    }

    private void OnDestroy()
    {
        GameEvents.instance.onInteract -= StartCooldown;
        GameEvents.instance.onInRange -= CanInteract;
        GameEvents.instance.onOutOfRange -= CannotInteract;

    }

    //--------------- Cooldown ----------------------------------------------
    void StartCooldown(GameObject gameObject)
    {
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        textCD.gameObject.SetActive(true);
        imageCD.fillAmount += 1;

        float cooldownTime = 1f;
        float timePassed = 0;

        while (timePassed < cooldownTime)
        {
            textCD.text = Mathf.RoundToInt(cooldownTime - timePassed).ToString();
            imageCD.fillAmount = (cooldownTime - timePassed)/cooldownTime;
            timePassed += Time.deltaTime;
            yield return null;
        }

        imageCD.fillAmount = 0;
        textCD.gameObject.SetActive(false);
    }

    //--------------- Within Range of Interacting ----------------------------------------------
    void CanInteract()
    {
        interactIcon.SetActive(true);
    }

    void CannotInteract()
    {
        interactIcon.SetActive(false);
    }


}
