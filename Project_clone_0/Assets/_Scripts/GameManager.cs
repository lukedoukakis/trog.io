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
    public static int cameraMode = 2;
    public static int startingTribeMembers = 14;



    void Awake(){
        current = this;

    }



    
 

}
