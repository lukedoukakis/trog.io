﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// class to hold commands sent from client to server


public class ClientCommand : NetworkBehaviour
{


    public static ClientCommand instance;
    public GameObject clientPlayerCharacter;
    public EntityHandle clientPlayerCharacterHandle;

    GameObject npcPrefab;

    void Awake()
    {
        instance = this;
        npcPrefab = Resources.Load<GameObject>("Terrain/Humans/Npc");
    }


    public override void OnStartLocalPlayer()
    {

        Debug.Log("OnStartLocalPlayer()");

        base.OnStartLocalPlayer();
        StartCoroutine(SpawnPlayerCharacterWhenReady(this.gameObject));
        StartCoroutine(SetAsPlayerWhenReady());
    }


    IEnumerator SpawnPlayerCharacterWhenReady(GameObject playerClientObject)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnPlayerCharacter(playerClientObject);
    }

    [Command]
    void CmdSpawnPlayerCharacter(GameObject playerClientObject)
    {

        GameObject p = GameObject.Instantiate(npcPrefab);
        EntityHandle pHandle = p.GetComponent<EntityHandle>();
        p.transform.position = new Vector3(0f, ChunkGenerator.ElevationAmplitude + 50f, 0f);

        ClientCommand pcc = playerClientObject.GetComponent<ClientCommand>();
        Debug.Log(pcc);
        pcc.clientPlayerCharacter = p;
        pcc.clientPlayerCharacterHandle = pHandle;

        StartCoroutine(SetNewFactionWhenReady(p.GetComponent<EntityHandle>(), false, FactionStartingItemsTier.PlayerTest));
        NetworkServer.Spawn(clientPlayerCharacter);

        Debug.Log("spawn done");
    }


    public IEnumerator SetAsPlayerWhenReady()
    {
        yield return new WaitUntil(() => NetworkClient.ready && clientPlayerCharacter != null);
        SetAsPlayer(clientPlayerCharacter);
    }
    public void SetAsPlayer(GameObject newPlayer)
    {

        clientPlayerCharacter = newPlayer;
        clientPlayerCharacterHandle = clientPlayerCharacter.GetComponent<EntityHandle>();
        clientPlayerCharacterHandle.entityUserInput.enabled = true;
        Utility.FindDeepChild(clientPlayerCharacter.transform, "HoverTrigger").gameObject.SetActive(false);
  
         // update global game variables
        Testing.instance.playerHandle = clientPlayerCharacterHandle;
        ChunkGenerator.instance.SetPlayerTransform(newPlayer.transform);
        CameraController.instance.SetPlayerTransform(newPlayer.transform);
        UIController.current.SetUIMode(false);
    }


    public IEnumerator SpawnNpcFollowerWhenReady(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnNpcFollower(leaderHandle, position, spawnWithGear);
    }
    [Command]
    public void CmdSpawnNpcFollower(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        Faction faction = leaderHandle.entityInfo.faction;
        EntityInfo npcInfo = npcHandle.entityInfo;
        npcInfo.name = "tribemember";
        npcInfo.faction = faction;
        npcHandle.entityBehavior.ResetFollowPosition();
        NetworkServer.Spawn(npc);

        //npcHandle.entityItems.EquipClothing(Item.ClothingTest);

        foreach (EntityHandle factionMemberHandle in faction.memberHandles)
        {
            Utility.IgnorePhysicsCollisions(npcHandle.transform, factionMemberHandle.transform);
        }
        faction.AddMember(npcHandle, FactionRole.Follower, true);

        if(spawnWithGear)
        {
            Item weaponItem = Item.GetRandomItem(ItemType.Weapon);
            GameObject weaponObject = Utility.InstantiateSameName(weaponItem.worldObjectPrefab, npcHandle.transform.position + Vector3.up * 20f, Quaternion.identity);
            npcHandle.entityItems.SetEquippedWeapon(weaponItem, weaponObject);
            npcHandle.entityItems.OnItemsChange();
        }

        ChunkGenerator.AddActiveCPUCreature(npc);
    }


    public IEnumerator SpawnNpcIndependentWhenReady(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnNpcIndependent(position, createCamp, factionTier);
    }
    [Command]
    public void CmdSpawnNpcIndependent(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        //Debug.Log("SpawnNpcIndependent()");
        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        NetworkServer.Spawn(npc);
        StartCoroutine(SetNewFactionWhenReady(npcHandle, createCamp, factionTier));

        ChunkGenerator.AddActiveCPUCreature(npc);
    }


    public IEnumerator SetNewFactionWhenReady(EntityHandle founderHandle, bool createCamp, FactionStartingItemsTier tier)
    {

        yield return new WaitUntil(() => NetworkClient.ready && founderHandle != null);

        CmdInstantiateNewFaction(founderHandle, tier);

        if(createCamp)
        {
            Camp.TryPlaceCamp(founderHandle.entityInfo.faction, founderHandle.transform);
        }
    }

    [Command]
    public void CmdInstantiateNewFaction(EntityHandle founderHandle, FactionStartingItemsTier tier)
    {
        //Debug.Log("SETTING FACTION");
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString());
        faction.leaderHandle = founderHandle;
        faction.AddMember(founderHandle, FactionRole.Leader, false);
        founderHandle.entityInfo.faction = faction;

        faction.AddStartingResources(tier);

        // if(ReferenceEquals(founderHandle, GameManager.instance.localPlayerHandle))
        // {
        //     Testing.instance.OnFactionCreation();
        // }
    }


}
