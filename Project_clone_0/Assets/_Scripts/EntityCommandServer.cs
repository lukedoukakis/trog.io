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

        this.fieldName = "entityCommandServer";

        base.Awake();
        npcPrefab = Resources.Load<GameObject>("Terrain/Humans/Npc");
        //Debug.Log("npcPrefab: " + npcPrefab.name);
    }

    public IEnumerator SpawnNpcWhenReady(EntityHandle leaderHandle, Vector3 position)
    {
        Debug.Log("SpawnNpcWhenReady() start");
        while (!NetworkClient.ready) {
            //Debug.Log("CHECKING...");
            yield return new WaitForSecondsRealtime(.05f);
        }
        SpawnNpc(leaderHandle, position);
        Debug.Log("SpawnNpcWhenReady() finished");
    }


    [Command]
    public void SpawnNpc(EntityHandle leaderHandle, Vector3 position)
    {

        Debug.Log("SpawnNpc() start");

        GameObject npc = GameObject.Instantiate(npcPrefab, position, Quaternion.identity);
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();

        Faction faction;
        if (leaderHandle != null)
        {
            faction = leaderHandle.entityInfo.faction;
            EntityInfo npcInfo = npcHandle.entityInfo;
            npcInfo.name = "tribemember";
            npcInfo.faction = faction;
            npcInfo.isFactionLeader = false;
            npcInfo.isFactionFollower = true;
            npcHandle.GetComponent<EntityBehavior>().UpdateHomePosition(faction.leaderInCamp);

            //npcHandle.entityItems.EquipClothing(Item.ClothingTest);

            foreach (EntityHandle factionMemberHandle in faction.memberHandles)
            {
                Utility.IgnorePhysicsCollisions(npcHandle.transform, factionMemberHandle.transform);
            }
            faction.AddMember(npcHandle, true);
            NetworkServer.Spawn(npc, GameManager.instance.localPlayer);
        }
        else
        {
            NetworkServer.Spawn(npc, GameManager.instance.localPlayer);
            StartCoroutine(SetNewFactionWhenReady(npcHandle));
        }


    
        Debug.Log("SpawnNpc() finished");
    }



    public IEnumerator SetNewFactionWhenReady(EntityHandle founderHandle){
        while (!NetworkClient.ready) {
            yield return new WaitForSecondsRealtime(.05f);
        }
        SetNewFaction(founderHandle);
        for(int i = 0; i < GameManager.startingTribeMembers; ++i)
        {
            StartCoroutine(GameManager.instance.localPlayerHandle.entityCommandServer.SpawnNpcWhenReady(founderHandle, founderHandle.transform.position));
        }
    }

    [Command]
    public void SetNewFaction(EntityHandle founderHandle)
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





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
