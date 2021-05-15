using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityStatus : EntityComponent
{
    
    public float hp;
    public bool alive;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityStatus = this;
    }

    void Start(){
        
    }

    
}
