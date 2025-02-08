using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameEvents : MonoBehaviour
{
    public static GameEvents instance;

    private void Awake()
    {
        instance = this;
    }

    //----------------------------------------------------------------------------------------------------------------------------------
    // Controls opening and closing the menu, called from the player, listened to by the UI canvas
    public event Action onMenu;
    public void MenuInput()
    {
        onMenu?.Invoke();
    }


    //----------------------------------------------------------------------------------------------------------------------------------
    // Controls interact cooldown and tells what is being interacted with (called within player)
    public event Action<GameObject> onInteract;
    public void Interact(GameObject gameObject)
    {
        onInteract?.Invoke(gameObject);
    }

    public event Action onInRange;
    public void CanInteract()
    {
        onInRange?.Invoke();
    }

    public event Action onOutOfRange;
    public void CannotInteract()
    {
        onOutOfRange?.Invoke();
    }


    //----------------------------------------------------------------------------------------------------------------------------------
    // Controls invisibility duration (called within player)
    public event Action<float> OnInvisibility;
    public void StartInvisibility(float duration) 
    {
        OnInvisibility?.Invoke(duration);
    }

    // Controls invisibility cooldown (called within player)
    public event Action<float> OnInvisibilityCooldown;
    public void StartCooldown(float cooldown)
    {
        OnInvisibilityCooldown?.Invoke(cooldown);
    }


    // Controls player taking damage (called within flashlight)
    // Subscribed to by healthbar UI and player
    public event Action<int> onTakingDamage;
    public void TakeDamage(int damage)
    {
        onTakingDamage?.Invoke(damage);
    }

    //----------------------------------------------------------------------------------------------------------------------------------
    // NPC destroyed calls this
    public event Action<GameObject> onNPCDestroy;
    public void NpcDead(GameObject npc)
    {
        onNPCDestroy?.Invoke(npc);
    }

    //----------------------------------------------------------------------------------------------------------------------------------
    // Player dies
    public event Action onLoseGame;
    public void LoseGame()
    {
        onLoseGame?.Invoke();
    }

    // Player Wins
    public event Action onWinGame;
    public void Winner()
    {
        onWinGame?.Invoke();
    }
}
