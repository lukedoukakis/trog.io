using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommandEveryoneController : MonoBehaviour
{
    

    Animator animator;
    public Transform commandEveryoneT;
    public bool expanded;

    public static CommandEveryoneController current;

    void Awake(){
        current = this;
        animator = commandEveryoneT.GetComponent<Animator>();
    }


    public void OnHornPress(){
        ToggleExpanded();
        if(expanded){
            GlobalSelectionController.current.SelectAllPlayerFactionMembers();
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
