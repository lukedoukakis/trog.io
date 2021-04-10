using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityInfo : EntityComponent
{

    public int ID;
    public string TYPE;
    public string NAME;


    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityInfo = this;
    }

    void Start(){
        Init();
    }

    void Init(){

        bool CREATENEW = true;
        // TODO: createNew if doesnt exist in memory

        ID = Random.Range(0, int.MaxValue);

    }





}
