using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampComponent : MonoBehaviour
{

    public Camp camp;
    public GameObject worldObject;
    Animator animator;


    public virtual void SetCampComponent(Camp camp)
    {
        this.camp = camp;
    }

    public void SetWorldObject(GameObject o){
        worldObject = o;
        animator = worldObject.GetComponentInChildren<Animator>();
    }

    public void PlayEntryAnimation(){
        if(animator != null){
            animator.SetTrigger("Entry");
        }
    }

    public void PlayDismantleAnimation()
    {
        if(animator != null)
        {
            animator.SetTrigger("Dismantle");
        }
    }


}
