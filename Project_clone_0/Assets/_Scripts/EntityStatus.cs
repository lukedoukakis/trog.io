using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatus : EntityComponent
{
    
    public float hp;
    public bool alive;



    protected override void Awake()
    {

        this.fieldName = "entityStatus";

        base.Awake();
    }

    void Start(){
        
    }

    
}
