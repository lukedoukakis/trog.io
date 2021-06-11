using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EntityComponent : NetworkBehaviour
{

    public EntityHandle handle;
    public EntityInfo entityInfo;
    public EntityStats entityStats;
    public EntityBehavior entityBehavior;
    public EntityAnimation entityAnimation;
    public EntityPhysics entityPhysics;
    public EntityStatus entityStatus;
    public EntityItems entityItems;
    public EntityUserInputMovement entityUserInputMovement;


    protected virtual void Awake(){
        handle = GetComponent<EntityHandle>();
        entityInfo = GetComponent<EntityInfo>();
        entityStats = GetComponent<EntityStats>();
        entityBehavior = GetComponent<EntityBehavior>();
        entityAnimation = GetComponent<EntityAnimation>();
        entityPhysics = GetComponent<EntityPhysics>();
        entityStatus = GetComponent<EntityStatus>();
        entityItems = GetComponent<EntityItems>();
        entityUserInputMovement = GetComponent<EntityUserInputMovement>();
    }


    public void Log(string msg){
        Debug.Log("Entity " + entityInfo.id + ": " + this.GetType().Name + ": " + msg);
    }
}
