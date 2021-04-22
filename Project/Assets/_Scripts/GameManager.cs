using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public static GameManager current;
    public int cameraMode;

    void Awake(){
        current = this;
    }
 

    public int gameId;



}
