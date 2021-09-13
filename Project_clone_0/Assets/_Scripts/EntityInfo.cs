using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Species{ Human, Tree }

public class EntityInfo : EntityComponent
{

    public int id;
    public Species species;
    public string nickname;
    public Faction faction;
    public SpeciesInfo speciesInfo;


    protected override void Awake(){

        base.Awake();


        id = Random.Range(0, int.MaxValue);
        speciesInfo = SpeciesInfo.GetSpeciesInfo(species);
    }


}


// to hold references to things like base stats, drops etc for all species
public class SpeciesInfo : ScriptableObject{
    public ItemCollection baseDrop;
    public Stats baseStats;
    public bool quadripedal;

    public static SpeciesInfo InstantiateSpeciesInfo(ItemCollection baseDrop, Stats baseStats, bool quadripedal){
        SpeciesInfo speciesInfo = ScriptableObject.CreateInstance<SpeciesInfo>();
        speciesInfo.baseDrop = baseDrop;
        speciesInfo.baseStats = baseStats;
        speciesInfo.quadripedal = quadripedal;
        
        return speciesInfo;
    }

    public static SpeciesInfo GetSpeciesInfo(Species spec){
        return SpeciesBaseReferencesMap[spec];
    }

    static Dictionary<Species, SpeciesInfo> SpeciesBaseReferencesMap = new Dictionary<Species, SpeciesInfo>(){
        {
            Species.Human, SpeciesInfo.InstantiateSpeciesInfo(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: drops for human
                    }
                    
                ),
                Stats.BASE_HUMAN,
                false
            )

        },

        {
            Species.Tree, SpeciesInfo.InstantiateSpeciesInfo(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: finish drop for tree
                        {Item.LogFir, 1},
                    }
                ),
                Stats.BASE_TREE,
                true
            )

        },


    };
}




