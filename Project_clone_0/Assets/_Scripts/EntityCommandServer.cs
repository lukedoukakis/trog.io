﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// class to hold commands sent from client to server


public class EntityCommandServer : EntityComponent
{


    public GameObject npcPrefab;

    protected override void Awake()
    {
        base.Awake();
        npcPrefab = Resources.Load<GameObject>("Entities/NpcIK");
        //Debug.Log("npcPrefab: " + npcPrefab.name);
    }

    public IEnumerator SpawnNpcWhenReady(EntityHandle leaderHandle){
        //Debug.Log("SpawnNpcWhenReady()");
        while (!NetworkClient.ready) {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpc(leaderHandle);
    }


    [Command]
    public void SpawnNpc(EntityHandle leaderHandle){
        //Debug.Log("SPAWNING");
        GameObject npc = GameObject.Instantiate(npcPrefab, leaderHandle.transform.position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        Faction owningPlayerFaction = leaderHandle.entityInfo.faction;
        npc.transform.position = leaderHandle.transform.position + Vector3.up;
        
        npcHandle.entityItems.EquipClothing(Item.ClothingTest);
        foreach(EntityHandle factionMemberHandle in owningPlayerFaction.members)
        {
            Utility.IgnorePhysicsCollisions(npcHandle.transform, factionMemberHandle.transform);
        }
        owningPlayerFaction.AddMember(npcHandle, true);
        npcHandle.entityInfo.name = "new npc";

        NetworkServer.Spawn(npc, leaderHandle.gameObject);
    
        Debug.Log("SpawnNpc() finished");
    }



    public IEnumerator SetNewFactionWhenReady(EntityHandle leaderHandle){
        while (!NetworkClient.ready) {
            yield return new WaitForSeconds(.05f);
        }
        SetNewFaction(leaderHandle);
    }

    [Command]
    public void SetNewFaction(EntityHandle leaderHandle){
        //Debug.Log("SETTING FACTION");
        EntityHandle handle = leaderHandle.GetComponent<EntityHandle>();
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString());
        faction.leader = leaderHandle.GetComponent<EntityHandle>();
        faction.AddMember(handle, false);
        handle.entityInfo.faction = faction;

        Testing.instance.OnFactionCreation();
    }





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
