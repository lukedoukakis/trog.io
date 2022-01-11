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

    public IEnumerator SpawnNpcFollowerWhenReady(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        //Debug.Log("SpawnNpcWhenReady() start");
        while (!NetworkClient.ready)
        {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpcFollower(leaderHandle, position, spawnWithGear);
        //Debug.Log("SpawnNpcWhenReady() finished");
    }


    public IEnumerator SpawnNpcIndependentWhenReady(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        //Debug.Log("SpawnNpcWhenReady() start");
        while (!NetworkClient.ready)
        {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpcIndependent(position, createCamp, factionTier);
        //Debug.Log("SpawnNpcWhenReady() finished");
    }



    [Command]
    public void SpawnNpcFollower(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        Faction faction = leaderHandle.entityInfo.faction;
        EntityInfo npcInfo = npcHandle.entityInfo;
        npcInfo.name = "tribemember";
        npcInfo.faction = faction;
        npcInfo.isFactionLeader = false;
        npcInfo.isFactionFollower = true;
        npcHandle.entityBehavior.ResetFollowPosition();
        NetworkServer.Spawn(npc, GameManager.instance.localPlayer);

        //npcHandle.entityItems.EquipClothing(Item.ClothingTest);

        foreach (EntityHandle factionMemberHandle in faction.memberHandles)
        {
            Utility.IgnorePhysicsCollisions(npcHandle.transform, factionMemberHandle.transform);
        }
        faction.AddMember(npcHandle, true);

        if(spawnWithGear)
        {
            Item weaponItem = Item.GetRandomItem(ItemType.Weapon);
            GameObject weaponObject = Utility.InstantiateSameName(weaponItem.worldObjectPrefab, npcHandle.transform.position + Vector3.up * 20f, Quaternion.identity);
            npcHandle.entityItems.SetEquippedWeapon(weaponItem, weaponObject);
            npcHandle.entityItems.OnItemsChange();
        }

        ChunkGenerator.AddActiveCPUCreature(npc);
    }

    [Command]
    public void SpawnNpcIndependent(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        //Debug.Log("SpawnNpcIndependent()");
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        NetworkServer.Spawn(npc, GameManager.instance.localPlayer);
        StartCoroutine(SetNewFactionWhenReady(npcHandle, createCamp, factionTier));

        ChunkGenerator.AddActiveCPUCreature(npc);
    }


    public IEnumerator SetNewFactionWhenReady(EntityHandle founderHandle, bool createCamp, FactionStartingItemsTier tier)
    {
        while (!NetworkClient.ready) {
            yield return new WaitForSecondsRealtime(.05f);
        }

        InstantiateNewFaction(founderHandle, tier);

        if(createCamp)
        {
            Camp.TryPlaceCamp(founderHandle.entityInfo.faction, founderHandle.transform);
        }
    }

    [Command]
    public void InstantiateNewFaction(EntityHandle founderHandle, FactionStartingItemsTier tier)
    {
        //Debug.Log("SETTING FACTION");
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString());
        faction.leaderHandle = founderHandle;
        faction.AddMember(founderHandle, false);
        founderHandle.entityInfo.faction = faction;
        founderHandle.entityInfo.isFactionLeader = true;
        founderHandle.entityInfo.isFactionFollower = false;

        faction.AddStartingResources(tier);

        // if(ReferenceEquals(founderHandle, GameManager.instance.localPlayerHandle))
        // {
        //     Testing.instance.OnFactionCreation();
        // }
    }


}
