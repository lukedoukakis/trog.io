using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInfo : EntityComponent
{

    public int id;
    public string species;
    public string nickname;
    public Faction faction;


    protected override void Awake(){

        base.Awake();


        id = Random.Range(0, int.MaxValue);
    }



    void Start(){

    }





}
