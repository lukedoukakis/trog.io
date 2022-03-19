using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCommand : MonoBehaviour
{


    public static PlayerCommand current;
    public string command;


    void Awake(){
        current = this;
    }


    // calls party command for player's party with the command command
    public void SendCommand(string c){


        // NEW PARTY CODE
        //Debug.Log("PlayerCommand(): SendCommand()");
        ClientCommand.instance.clientPlayerCharacterHandle.entityInfo.faction.SendPartyCommand(c);




        // OLD CODE

        // //Debug.Log("Sending command to selected");
        // EntityBehavior behavior;
        // foreach(EntityHandle handle in GlobalSelectionController.current.SelectedHandles.ToArray()){
        //     behavior = handle.entityBehavior;
        //     behavior.InsertActionImmediate(Action.GenerateAction(c, behavior.entityHandle), true);
        // }
        // //Debug.Log("Commands sent!");



    }



    // Update is called once per frame
    void Update()
    {

        
    }
}
