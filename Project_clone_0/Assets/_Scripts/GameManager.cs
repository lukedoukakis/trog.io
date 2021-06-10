using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{

    public static GameManager current;
    public int gameId;
    public GameObject localPlayer;



    // options
    public static int cameraMode = 1;
    public static int startingTribeMembers = 3;



    void Awake(){
        current = this;

    }

    public static void SpawnNpc(GameObject prefab, GameObject player){
        GameObject npc = GameObject.Instantiate(prefab, player.transform.position, Quaternion.identity);
        EntityHandle playerHandle = player.GetComponent<EntityHandle>();
        EntityHandle npcHandle = npc.GetComponent<EntityHandle>();
        playerHandle.entityInfo.faction.AddMember(npcHandle);
        npcHandle.entityInfo.name = "new npc";
    }

    
 

}
