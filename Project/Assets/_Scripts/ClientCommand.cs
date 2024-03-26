using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// class to hold commands sent from client to server


public class ClientCommand : MonoBehaviour
{


    public static ClientCommand instance;
    public GameObject clientPlayerCharacter;
    public EntityHandle clientPlayerCharacterHandle;

    [SerializeField] GameObject characterPrefab;

    void Awake()
    {
        instance = this;
    }

    public void OnGameStart()
    {

        Debug.Log("OnGameStart()");
        SpawnPlayerCharacter();
    }




    void SpawnPlayerCharacter()
    {

        GameObject playerCharacter = GameObject.Instantiate(characterPrefab);
        playerCharacter.name = "player";
        EntityHandle playerHandle = playerCharacter.GetComponent<EntityHandle>();
        playerCharacter.transform.position = new Vector3(0f, ChunkGenerator.Amplitude + 200f, 0f);
        playerHandle.entityPhysics.rb.position = new Vector3(0f, ChunkGenerator.Amplitude + 200f, 0f);
        clientPlayerCharacter = playerCharacter;
        clientPlayerCharacterHandle = playerHandle;

        CreateNewFaction(playerHandle, false, FactionStartingItemsTier.PlayerTest);
        SetPlayerCharacter(clientPlayerCharacter);


        //Testing.instance.OnFactionCreation();
        
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


    public void SpawnCharacterAsFollower(EntityHandle leaderHandle, Vector3 position, bool spawnWithGear)
    {

        Debug.Log("spawning follower");

        GameObject character = GameObject.Instantiate(characterPrefab, position, Quaternion.identity);
        EntityHandle characterHandle = character.GetComponent<EntityHandle>();
        characterHandle.entityPhysics.rb.position = position;
        Faction faction = leaderHandle.entityInfo.faction;
        EntityInfo characterInfo = characterHandle.entityInfo;
        characterInfo.name = "tribemember";
        characterInfo.faction = faction;
        characterHandle.entityBehavior.ResetFollowPosition();
        //NetworkServer.Spawn(character);

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

        Debug.Log("spawned follower");
    }


    public void SpawnCharacterAsLeader(Vector3 position, bool createCamp, FactionStartingItemsTier factionTier)
    {
        GameObject character = GameObject.Instantiate(characterPrefab, position, Quaternion.identity);
        EntityHandle characterHandle = character.GetComponent<EntityHandle>();
        CreateNewFaction(characterHandle, createCamp, factionTier);

        ChunkGenerator.AddActiveCPUCreature(character);
    }


    public void CreateNewFaction(EntityHandle founderHandle, bool createCamp, FactionStartingItemsTier tier)
    {
        //Debug.Log("SETTING FACTION");
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString());
        faction.leaderHandle = founderHandle;
        faction.AddMember(founderHandle, FactionRole.Leader, false);
        founderHandle.entityInfo.faction = faction;
        faction.AddStartingResources(tier);
        if(createCamp)
        {
            Camp.TryPlaceCamp(founderHandle.entityInfo.faction, founderHandle.transform);
        }
    }



}
