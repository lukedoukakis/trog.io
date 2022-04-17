using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public enum RotationalInputMode { Mouse, ArrowKeys }

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public int gameId;



    // options
    public static bool GAME_SETTINGS_AUTO_ATTACK = true;
    public static RotationalInputMode GAME_SETINGS_ROTATIONALINPUTMODE = RotationalInputMode.ArrowKeys;



    void Awake()
    {
        instance = this;
    }





    
 

}
