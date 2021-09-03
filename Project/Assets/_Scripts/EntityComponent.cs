using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class EntityComponent : NetworkBehaviour
{

    [HideInInspector] public EntityHandle entityHandle;
    [HideInInspector] public EntityInfo entityInfo;
    [HideInInspector] public EntityStats entityStats;
    [HideInInspector] public EntityBehavior entityBehavior;
    [HideInInspector] public EntityAnimation entityAnimation;
    [HideInInspector] public EntityPhysics entityPhysics;
    [HideInInspector] public EntityStatus entityStatus;
    [HideInInspector] public EntityItems entityItems;
    [HideInInspector] public EntityHitDetection entityHitDetection;
    [HideInInspector] public EntityUserInputMovement entityUserInputMovement;
    [HideInInspector] public EntityCommandServer entityCommandServer;


    protected virtual void Awake(){
        entityHandle = GetComponent<EntityHandle>();
        entityInfo = GetComponent<EntityInfo>();
        entityStats = GetComponent<EntityStats>();
        entityBehavior = GetComponent<EntityBehavior>();
        entityAnimation = GetComponent<EntityAnimation>();
        entityPhysics = GetComponent<EntityPhysics>();
        entityStatus = GetComponent<EntityStatus>();
        entityItems = GetComponent<EntityItems>();
        entityHitDetection = GetComponent<EntityHitDetection>();
        entityUserInputMovement = GetComponent<EntityUserInputMovement>();
        entityCommandServer = GetComponent<EntityCommandServer>();
    }


    public void Log(string msg){
        Debug.Log("Entity " + entityInfo.id + ": " + this.GetType().Name + ": " + msg);
    }
}
