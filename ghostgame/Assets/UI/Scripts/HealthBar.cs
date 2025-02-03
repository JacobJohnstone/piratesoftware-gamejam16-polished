using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    Slider slider;

    private void Start()
    {
        GameEvents.instance.onTakingDamage += TakeDamage;
    }

    private void OnDestroy()
    {
        GameEvents.instance.onTakingDamage -= TakeDamage;
    }

    private void TakeDamage(int damage)
    {
        slider.value -= damage;
    }

}
