using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player current;

    void Awake(){
        current = this;
    }
    
}
