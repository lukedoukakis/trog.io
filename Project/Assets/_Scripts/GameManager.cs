using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{

    public static GameManager current;
    public int gameId;
    public GameObject localPlayer;
    public EntityHandle localPlayerHandle;



    // options
    public static int cameraMode = 1;
    public static int startingTribeMembers = 3;



    void Awake(){
        current = this;

    }

    public void SetLocalPlayer(GameObject o){
        localPlayer = o;
        localPlayerHandle = o.GetComponent<EntityHandle>();
    }



    
 

}
