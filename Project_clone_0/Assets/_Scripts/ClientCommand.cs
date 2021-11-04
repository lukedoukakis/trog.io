using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// class to hold commands sent from client to server


public class ClientCommand : NetworkBehaviour
{


    public static ClientCommand instance;

    public GameObject npcPrefab;

    void Awake()
    {
        instance = this;
        
        npcPrefab = Resources.Load<GameObject>("Terrain/Humans/Npc");
        //Debug.Log("npcPrefab: " + npcPrefab.name);
    }

    public IEnumerator SpawnNpcFollowerWhenReady(EntityHandle leaderHandle, Vector3 position)
    {
        Debug.Log("SpawnNpcWhenReady() start");
        while (!NetworkClient.ready)
        {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpcFollower(leaderHandle, position);
        Debug.Log("SpawnNpcWhenReady() finished");
    }


    public IEnumerator SpawnNpcIndependentWhenReady(Vector3 position, bool createCamp)
    {
        Debug.Log("SpawnNpcWhenReady() start");
        while (!NetworkClient.ready)
        {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpcIndependent(position, createCamp);
        Debug.Log("SpawnNpcWhenReady() finished");
    }



    [Command]
    public void SpawnNpcFollower(EntityHandle leaderHandle, Vector3 position)
    {
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        Faction faction = leaderHandle.entityInfo.faction;
        EntityInfo npcInfo = npcHandle.entityInfo;
        npcInfo.name = "tribemember";
        npcInfo.faction = faction;
        npcInfo.isFactionLeader = false;
        npcInfo.isFactionFollower = true;
        npcHandle.entityBehavior.UpdateHomePosition(faction.leaderInCamp);
        NetworkServer.Spawn(npc, GameManager.instance.localPlayer);

        //npcHandle.entityItems.EquipClothing(Item.ClothingTest);

        foreach (EntityHandle factionMemberHandle in faction.memberHandles)
        {
            Utility.IgnorePhysicsCollisions(npcHandle.transform, factionMemberHandle.transform);
        }
        faction.AddMember(npcHandle, true);
    }

    [Command]
    public void SpawnNpcIndependent(Vector3 position, bool createCamp)
    {
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        NetworkServer.Spawn(npc, GameManager.instance.localPlayer);
        StartCoroutine(SetNewFactionWhenReady(npcHandle, createCamp));
    }


    public IEnumerator SetNewFactionWhenReady(EntityHandle founderHandle, bool createCamp)
    {
        while (!NetworkClient.ready) {
            yield return new WaitForSecondsRealtime(.05f);
        }

        InstantiateNewFaction(founderHandle);

        // spawn tribe members
        for(int i = 0; i < GameManager.startingTribeMembers; ++i)
        {
            StartCoroutine(ClientCommand.instance.SpawnNpcFollowerWhenReady(founderHandle, founderHandle.transform.position));
        }

        if(createCamp)
        {
            Camp.TryPlaceCamp(founderHandle.entityInfo.faction, founderHandle.transform);
        }
    }

    [Command]
    public void InstantiateNewFaction(EntityHandle founderHandle)
    {
        //Debug.Log("SETTING FACTION");
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString());
        faction.leaderHandle = founderHandle;
        faction.AddMember(founderHandle, false);
        founderHandle.entityInfo.faction = faction;
        founderHandle.entityInfo.isFactionLeader = true;
        founderHandle.entityInfo.isFactionFollower = false;

        Testing.instance.OnFactionCreation();
    }


}
