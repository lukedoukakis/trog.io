using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager current;
    public int gameId;
    public int cameraMode;


    // entries indicate whether the factions delimited by the indices are at war
    public bool[,] factionRelations;

    void Awake(){
        current = this;


        factionRelations = new bool[100,100];

        // TODO:LOADGAME



        // TEST: factions 0 and 1 to war
        SetFactionRelations(0, 1, true);
    }


    public void SetFactionRelations(int fac1, int fac2, bool war){
        factionRelations[fac1,fac2] = war;
        factionRelations[fac2,fac1] = war;
    }
 





}
