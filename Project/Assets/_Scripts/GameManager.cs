using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public int gameId;



    // options
    public static int GAME_SETTINGS_CAMERA_MODE = 1;
    public static bool GAME_SETTINGS_AUTO_ATTACK = true;



    void Awake()
    {
        instance = this;
    }





    
 

}
