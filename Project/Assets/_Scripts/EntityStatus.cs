using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatus : EntityComponent
{
    

    public int faction;
    public float hp;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityStatus = this;
    }

    void Start(){
        
    }

    
}
