using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInfo : EntityComponent
{

    public int id;
    public string species;
    public string nickname;
    public Faction faction;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityInfo = this;

        id = Random.Range(0, int.MaxValue);
        faction = GameManager.current.testFac;
    }



    void Start(){

    }





}
