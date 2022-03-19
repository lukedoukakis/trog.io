using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using Mirror;
using System.Reflection;

public class EntityComponent : NetworkBehaviour
{

    protected string fieldName;

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


    protected virtual void Awake()
    {
        FindAndSetEntityReferences();
    }


    public void FindAndSetEntityReferences()
    {
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


        Type thisComponentType = GetType();
        FieldInfo componentFieldInfo;
        foreach(EntityComponent component in GetComponents<EntityComponent>())
        {
            componentFieldInfo = thisComponentType.GetField(this.fieldName);
            componentFieldInfo.SetValue(component, this);
        }
    }

    // called when an entity is to woosh be removed from the world
    public void RemoveFromWorld()
    {

        //Debug.Log("RemoveFromWorld()");

        // destroy any items the entity is carrying
        if(entityItems != null)
        {
            if(entityItems.holding_object != null)
            {
                GameObject.Destroy(entityItems.holding_object);
            }
            if(entityItems.weaponEquipped_object != null)
            {
                GameObject.Destroy(entityItems.weaponEquipped_object);
            }
            if(entityItems.weaponUnequipped_object != null)
            {
                GameObject.Destroy(entityItems.weaponUnequipped_object);
            }
        }

        // stop behavior actions and destroy follow position GameObject
        if(entityBehavior != null)
        {
            entityBehavior.StopActions();
            if(entityBehavior.followPositionTransform != null)
            {
                entityBehavior.followPositionTransform.SetParent(null);
                GameObject.Destroy(entityBehavior.followPositionTransform.gameObject);
            }
        }
    
        // handle faction stuff
        if(entityInfo != null)
        {
            Faction fac = entityInfo.faction;
            if(fac != null)
            {
                // remove from faction and set a new leader if a candidate is available
                fac.RemoveMember(entityHandle);
                EntityHandle newLeaderHandle = fac.memberHandles.Count > 0 ? fac.memberHandles[0] : null;
                if(newLeaderHandle != null)
                {
                    // set leader status to the oldest faction member
                    fac.SetLeader(fac.memberHandles[0]);

                    // if this is the player, transfer player status to the new leader
                    if(ReferenceEquals(entityHandle, GameManager.instance.localPlayerHandle))
                    {
                        GameManager.instance.TransferPlayerStatus(newLeaderHandle);
                    }
                }
                else
                {
                    // no members to assign as faction leader, so faction dies
                    fac.DestroyFaction();
                }
            }
        }

        // destroy GameObject
        GameObject.Destroy(this.gameObject);

    }


    public void Log(string msg){
        Debug.Log("Entity " + entityInfo.id + ": " + this.GetType().Name + ": " + msg);
    }
}
