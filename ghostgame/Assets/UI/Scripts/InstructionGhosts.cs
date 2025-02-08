using UnityEngine;
using UnityEngine.UI;

public class InstructionGhosts : MonoBehaviour
{
    [SerializeField]
    Image ghost;
    float timePassed;
    bool invis;

    void Start()
    {
        invis = false;
        timePassed = 0;
    }

    void Update()
    {
        timePassed += Time.deltaTime;
        if(timePassed > 2f)
        {
            timePassed = 0;
            invis = !invis;
            ChangeTransparency(invis);
        }
    }

    void ChangeTransparency(bool invis)
    {
        if (invis) {
            Color color = Color.white;
            color.a = 0.27f;
            ghost.color = color;
        } else
        {
            Color color = Color.white;
            color.a = 1f;
            ghost.color = color;
        }
    }
}
