using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityActionRecorder : EntityComponent
{


    public Dictionary<ActionParameters, float> actionsPast;



    protected override void Awake()
    {
        base.Awake();

        actionsPast = new Dictionary<ActionParameters, float>();
    }

  
    public void RecordAction(ActionParameters ap)
    {
        if(ap != null)
        {
            actionsPast.Add(ap, Time.time);
        }
    }
}
