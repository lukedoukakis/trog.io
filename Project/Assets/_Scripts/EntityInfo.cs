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


    protected override void Awake(){

        base.Awake();


        id = Random.Range(0, int.MaxValue);
    }


}


// to hold references to things like base stats, drops etc for all species
public class SpeciesBaseReferences : ScriptableObject{
    public ItemCollection baseDrop;
    public Stats baseStats;

    public static SpeciesBaseReferences InstantiateSpeciesBaseReferences(ItemCollection baseDrop, Stats baseStats){
        SpeciesBaseReferences speciesBaseReferences = ScriptableObject.CreateInstance<SpeciesBaseReferences>();
        speciesBaseReferences.baseDrop = baseDrop;
        speciesBaseReferences.baseStats = baseStats;
        return speciesBaseReferences;
    }

    public static SpeciesBaseReferences GetBaseReferences(Species spec){
        return SpeciesBaseReferencesMap[spec];
    }

    static Dictionary<Species, SpeciesBaseReferences> SpeciesBaseReferencesMap = new Dictionary<Species, SpeciesBaseReferences>(){
        {
            Species.Human, SpeciesBaseReferences.InstantiateSpeciesBaseReferences(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: drops for human
                    }
                    
                ),
                Stats.BASE_HUMAN
            )

        },

        {
            Species.Tree, SpeciesBaseReferences.InstantiateSpeciesBaseReferences(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: finish drop for tree
                        {Item.Wood, 4},
                    }
                ),
                Stats.BASE_TREE
            )

        },


    };
}




