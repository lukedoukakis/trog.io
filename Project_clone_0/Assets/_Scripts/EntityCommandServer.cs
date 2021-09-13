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
        npcPrefab = Resources.Load<GameObject>("Entities/Bear_NpcIK");
        //Debug.Log("npcPrefab: " + npcPrefab.name);
    }

    public IEnumerator SpawnNpcWhenReady(GameObject owningPlayer){
        while (!NetworkClient.ready) {
            yield return new WaitForSeconds(.05f);
        }
        SpawnNpc(owningPlayer);
    }


    [Command]
    public void SpawnNpc(GameObject owningPlayer){
        //Debug.Log("SPAWNING");
        GameObject npc = GameObject.Instantiate(npcPrefab, owningPlayer.transform.position, Quaternion.identity);
        EntityHandle playerHandle = owningPlayer.GetComponent<EntityHandle>();
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        playerHandle.entityInfo.faction.AddMember(npcHandle);
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
        Faction faction = Faction.InstantiateFaction("Faction " + (Random.Range(0, 10000f)).ToString(), true);
        faction.AddMember(handle);
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
