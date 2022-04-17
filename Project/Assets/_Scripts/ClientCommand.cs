using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


// class to hold commands sent from client to server


public class ClientCommand : NetworkBehaviour
{


    public static ClientCommand instance;
    public GameObject clientPlayerCharacter;
    public EntityHandle clientPlayerCharacterHandle;

    GameObject characterPrefab;

    void Awake()
    {
        instance = this;
        characterPrefab = Resources.Load<GameObject>("Terrain/Humans/Character");
    }


    public override void OnStartLocalPlayer()
    {

        Debug.Log("OnStartLocalPlayer()");

        base.OnStartLocalPlayer();
        StartCoroutine(SpawnPlayerCharacterWhenReady());
    }


    IEnumerator SpawnPlayerCharacterWhenReady()
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnPlayerCharacter();
    }

    [Command]
    void CmdSpawnPlayerCharacter()
    {

        GameObject p = GameObject.Instantiate(characterPrefab);
        EntityHandle pHandle = p.GetComponent<EntityHandle>();
        p.transform.position = new Vector3(0f, ChunkGenerator.ElevationAmplitude + 50f, 0f);

        //Debug.Log(pcc);
        clientPlayerCharacter = p;
        clientPlayerCharacterHandle = pHandle;

        StartCoroutine(SetNewFactionWhenReady(p.GetComponent<EntityHandle>(), false, FactionStartingItemsTier.PlayerTest));
        NetworkServer.Spawn(clientPlayerCharacter);

        SetPlayerCharacter(clientPlayerCharacter);

        Debug.Log("spawn done");
    }


    public void SetPlayerCharacter(GameObject character)
    {
        clientPlayerCharacterHandle.entityUserInput.enabled = false;
        Utility.FindDeepChild(clientPlayerCharacter.transform, "HoverTrigger").gameObject.SetActive(true);

        clientPlayerCharacter = character;
        clientPlayerCharacterHandle = clientPlayerCharacter.GetComponent<EntityHandle>();
        clientPlayerCharacterHandle.entityUserInput.enabled = true;
        Utility.FindDeepChild(clientPlayerCharacter.transform, "HoverTrigger").gameObject.SetActive(false);
  
         // update global game variables
        Testing.instance.playerHandle = clientPlayerCharacterHandle;
        ChunkGenerator.instance.SetPlayerTransform(character.transform);
        CameraController.instance.SetPlayerTransform(character.transform);
        //CameraController.instance.SetPlayerTransform(clientPlayerCharacterHandle.entityOrientation.body);

        UIController.instance.SetUIMode(false);
    }


    public IEnumerator SpawnCharacterAsFollowerWhenReady(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnCharacterAsFollower(leaderHandle, position, spawnWithGear);
    }
    [Command]
    public void CmdSpawnCharacterAsFollower(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {
        GameObject character = GameObject.Instantiate(characterPrefab, position, Quaternion.identity);
        EntityHandle characterHandle = character.GetComponent<EntityHandle>();
        Faction faction = leaderHandle.entityInfo.faction;
        EntityInfo characterInfo = characterHandle.entityInfo;
        characterInfo.name = "tribemember";
        characterInfo.faction = faction;
        characterHandle.entityBehavior.ResetFollowPosition();
        NetworkServer.Spawn(character);

        //npcHandle.entityItems.EquipClothing(Item.ClothingTest);

        foreach (EntityHandle factionMemberHandle in faction.memberHandles)
        {
            Utility.IgnorePhysicsCollisions(characterHandle.transform, factionMemberHandle.transform);
        }
        faction.AddMember(characterHandle, FactionRole.Follower, true);

        if(spawnWithGear)
        {
            Item weaponItem = Item.GetRandomItem(ItemType.Weapon);
            GameObject weaponObject = Utility.InstantiateSameName(weaponItem.worldObjectPrefab, characterHandle.transform.position + Vector3.up * 20f, Quaternion.identity);
            characterHandle.entityItems.SetEquippedWeapon(weaponItem, weaponObject);
            characterHandle.entityItems.OnItemsChange();
        }

        ChunkGenerator.AddActiveCPUCreature(character);
    }


    public IEnumerator SpawnCharacterAsLeaderWhenReady(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        yield return new WaitUntil(() => NetworkClient.ready);
        CmdSpawnCharacterAsLeader(position, createCamp, factionTier);
    }
    [Command]
    public void CmdSpawnCharacterAsLeader(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        GameObject character = GameObject.Instantiate(characterPrefab, position, Quaternion.identity);
        EntityHandle characterHandle = character.GetComponent<EntityHandle>();
        NetworkServer.Spawn(character);
        StartCoroutine(SetNewFactionWhenReady(characterHandle, createCamp, factionTier));

        ChunkGenerator.AddActiveCPUCreature(character);
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
