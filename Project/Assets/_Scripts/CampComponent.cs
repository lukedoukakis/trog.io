using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CampComponent : MonoBehaviour
{


    public GameObject worldObject;
    Animator animator;


    public void SetWorldObject(GameObject o){
        worldObject = o;
        animator = worldObject.GetComponentInChildren<Animator>();
    }

    public void PlayEntryAnimation(){
        if(animator != null){
            animator.SetTrigger("Entry");
        }
    }


}
