﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    
    public static ParticleController instance;

    public GameObject TreeDebris;



    void Awake()
    {
        instance = this;
    }


}