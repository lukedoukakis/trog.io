using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager current;
    public int gameId;
    public int cameraMode;
    public Faction testFac;



    void Awake(){
        current = this;


        testFac = Faction.GenerateFaction("TestFaction", true);

    }
 

}
