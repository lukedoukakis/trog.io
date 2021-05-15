using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandEveryoneController : MonoBehaviour
{
    

    Animator animator;
    public bool expanded;

    public static CommandEveryoneController current;

    void Awake(){
        current = this;
        animator = GetComponent<Animator>();
    }


    public void OnHornPress(){
        ToggleExpanded();
        if(expanded){
            GlobalSelectionController.current.SelectAllEntitiesWithTag("Npc");
        }
        else{
            GlobalSelectionController.current.ClearSelected();
        }
        
    }


    public void ToggleExpanded(){
        expanded = !expanded;
        animator.SetBool("CommandsVisible", expanded);
    }




}
