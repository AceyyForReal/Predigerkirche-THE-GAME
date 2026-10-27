using UnityEngine;

public class AquamanController : MonoBehaviour
{
    public Animator aqmAnim;
    public bool isTalkingToAQM = false;
    private bool lastTalkingState = false;

    void Update()
    {
        if (isTalkingToAQM != lastTalkingState)
        {
            aqmAnim.SetBool("isTalking", isTalkingToAQM);
            lastTalkingState = isTalkingToAQM;
        }
    }
}