using System.Collections;
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

    public IEnumerator SpawnNpcWhenReady(GameObject owningPlayer){
        Debug.Log("SpawnNpcWhenReady()");
        while (!NetworkClient.ready) {
            Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpc(owningPlayer);
    }


    [Command]
    public void SpawnNpc(GameObject owningPlayer){
        Debug.Log("SPAWNING");
        GameObject npc = GameObject.Instantiate(npcPrefab, owningPlayer.transform.position, Quaternion.identity);
        EntityHandle playerHandle = owningPlayer.GetComponent<EntityHandle>();
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        Faction owningPlayerFaction = playerHandle.entityInfo.faction;
        npc.transform.position = owningPlayer.transform.position + Vector3.up;
        
        npcHandle.entityItems.EquipClothing(Item.ClothingTest);
        foreach(EntityHandle factionMemberHandle in owningPlayerFaction.members)
        {
            Utility.IgnorePhysicsCollisions(npcHandle.gameObject, factionMemberHandle.gameObject.GetComponentsInChildren<Collider>());
        }
        owningPlayerFaction.AddMember(npcHandle, true);
        npcHandle.entityInfo.name = "new npc";

        NetworkServer.Spawn(npc, owningPlayer);
    }



    public IEnumerator SetNewFactionWhenReady(GameObject entity){
        while (!NetworkClient.ready) {
            yield return new WaitForSeconds(.05f);
        }
        SetNewFaction(entity);
    }

    [Command]
    public void SetNewFaction(GameObject entity){
        //Debug.Log("SETTING FACTION");
        EntityHandle handle = entity.GetComponent<EntityHandle>();
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, int.MaxValue)).ToString(), true);
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
