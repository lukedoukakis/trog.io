using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionCountLimiter : MonoBehaviour
{

    [SerializeField] int remainingInteractionsAllowed;


    public int GetRemainingInteractionsAllowed()
    {
        return remainingInteractionsAllowed;
    }

    public void OnInteract()
    {
        AddRemainingInteractionsAllowed(-1);
    }

    void AddRemainingInteractionsAllowed(int addCount)
    {
        remainingInteractionsAllowed += addCount;
        if(remainingInteractionsAllowed <= 0)
        {
            remainingInteractionsAllowed = 0;
            OnInteractionCountZero();  
        }
    }

    void OnInteractionCountZero()
    {
        //Debug.Log("interactions 0 -> disabling hover trigger");
        Utility.FindDeepChild(transform, "HoverTrigger").GetComponent<Collider>().enabled = false;
    }

}
