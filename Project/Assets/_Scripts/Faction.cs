using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Faction : ScriptableObject
{


    public bool isPlayerFaction;
    public int id;
    public string factionName;
    public List<EntityHandle> members;
    public List<GameObject> objects_currentlyTargeted;
    public List<GameObject> objects_currentlyOwned;
    public List<Faction> warringFactions;




    void Init(){
        members = new List<EntityHandle>();
    }
    public void AddMember(EntityHandle handle){
        members.Add(handle);
        handle.entityInfo.faction = this;
    }

    public static void AddItemTargeted(GameObject o, Faction fac){
        fac.objects_currentlyTargeted.Add(o);
    }
    public static void RemoveItemTargeted(GameObject o, Faction fac){
        fac.objects_currentlyOwned.Remove(o);
    }
    public static void AddItemOwned(GameObject o, Faction fac){
        fac.objects_currentlyOwned.Add(o);
    }
    public static void RemoveItemOwned(GameObject o, Faction fac){
        fac.objects_currentlyOwned.Remove(o);
    }

    public string ToString(){
        string str = factionName + ": ";
        foreach(EntityHandle handle in members){
            str += handle.entityInfo.nickname + ", ";
        }
        return str;
    }

    public static void SetWarStatus(Faction fac1, Faction fac2, bool war){
        if(war){
            if(!fac1.warringFactions.Contains(fac2)){
                fac1.warringFactions.Add(fac2);
            }
            if(!fac2.warringFactions.Contains(fac1)){
                fac2.warringFactions.Add(fac1);
            }

        }
        else{
            if(fac1.warringFactions.Contains(fac2)){
                fac1.warringFactions.Remove(fac2);
            }
            if(fac2.warringFactions.Contains(fac1)){
                fac2.warringFactions.Remove(fac1);
            }
        }
    }

    public static bool ItemIsTargetedByFaction(GameObject o, Faction fac){
        return fac.objects_currentlyTargeted.Contains(o);
    }
    public static bool ItemIsOwnedByFaction(GameObject o, Faction fac){
        return fac.objects_currentlyOwned.Contains(o);
    }



    public static Faction GenerateFaction(string _factionName, bool _isPlayerFaction){
        Faction f = ScriptableObject.CreateInstance<Faction>();
        f.id = UnityEngine.Random.Range(0, int.MaxValue);
        f.factionName = _factionName;
        f.isPlayerFaction = _isPlayerFaction;
        f.objects_currentlyTargeted = new List<GameObject>();
        f.objects_currentlyOwned = new List<GameObject>();
        f.warringFactions = new List<Faction>();
        f.Init();
        return f;
    }
}
