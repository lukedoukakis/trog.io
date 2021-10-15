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
    [HideInInspector] public EntityOrientation entityOrientation;
    [HideInInspector] public EntityPhysics entityPhysics;
    [HideInInspector] public EntityStatus entityStatus;
    [HideInInspector] public EntityItems entityItems;
    [HideInInspector] public EntityActionRecorder entityActionRecorder;
    [HideInInspector] public EntityHitDetection entityHitDetection;
    [HideInInspector] public EntityUserInput entityUserInput;
    [HideInInspector] public EntityCommandServer entityCommandServer;


    protected virtual void Awake(){
        FindAndSetEntityReferences();
    }


    // public static void SetupEntity()
    // {

    // }

    public void FindAndSetEntityReferences(){
        entityHandle = GetComponent<EntityHandle>();
        entityInfo = GetComponent<EntityInfo>();
        entityStats = GetComponent<EntityStats>();
        entityBehavior = GetComponent<EntityBehavior>();
        entityOrientation = GetComponent<EntityOrientation>();
        entityPhysics = GetComponent<EntityPhysics>();
        entityStatus = GetComponent<EntityStatus>();
        entityItems = GetComponent<EntityItems>();
        entityActionRecorder = GetComponent<EntityActionRecorder>();
        entityHitDetection = GetComponent<EntityHitDetection>();
        entityUserInput = GetComponent<EntityUserInput>();
        entityCommandServer = GetComponent<EntityCommandServer>();
    }


    public void Log(string msg){
        Debug.Log("Entity " + entityInfo.id + ": " + this.GetType().Name + ": " + msg);
    }
}
