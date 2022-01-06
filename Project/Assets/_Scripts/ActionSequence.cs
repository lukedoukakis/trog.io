using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// structure to hold a list of actions and execute them one by one when 'Execute()' is called
public class ActionSequence : ScriptableObject
{


    public Action[] actionsList;

    public static ActionSequence CreateActionSequence(params Action[] actions)
    {
        ActionSequence actionSequence = ScriptableObject.CreateInstance<ActionSequence>();
        actionSequence.actionsList = actions;
        return actionSequence;
    }


    // execute every action in the list of actions
    public void Execute()
    {
        for(int i = 0; i < actionsList.Length; ++i)
        {
            actionsList[i]();
        }
    }



    
}
