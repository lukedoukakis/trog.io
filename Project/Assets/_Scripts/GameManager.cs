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
    public static int startingTribeMembers = 0;



    void Awake(){
        current = this;

    }



    
 

}
