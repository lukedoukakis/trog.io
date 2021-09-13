using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Species{ Human, Bear, Tree }

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
    public IkProfile ikProfile;

    public static SpeciesInfo InstantiateSpeciesInfo(ItemCollection baseDrop, Stats baseStats, IkProfile ikProfile){
        SpeciesInfo speciesInfo = ScriptableObject.CreateInstance<SpeciesInfo>();
        speciesInfo.baseDrop = baseDrop;
        speciesInfo.baseStats = baseStats;
        speciesInfo.ikProfile = ikProfile;
        
        return speciesInfo;
    }

    public static SpeciesInfo GetSpeciesInfo(Species spec){
        return SpeciesInfoDict[spec];
    }

    static Dictionary<Species, SpeciesInfo> SpeciesInfoDict = new Dictionary<Species, SpeciesInfo>(){
        {
            Species.Human, SpeciesInfo.InstantiateSpeciesInfo(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: drops for human
                    }
                    
                ),
                Stats.BASE_HUMAN,
                IkProfile.InstantiateIkProfile("B-head", "B-hips", "B-foot_R", "B-foot_L", "B-toe_R", "B-toe_L", "B-palm_01_R", "B-palm_01_L", "B-f_index_01_R", "B-f_index_01_L", false, 3f, 5f, 1f, .58f)
            )

        },

        {
            Species.Bear, SpeciesInfo.InstantiateSpeciesInfo(
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: drops for bear
                    }
                    
                ),
                Stats.BASE_BEAR,
                IkProfile.InstantiateIkProfile("head", "spine_lower", "leg_lower_right.001", "leg_lower_left.001", "leg_lower_right.001_end", "leg_lower_left.001_end", "arm_lower_right.001_end", "arm_lower_left.001_end", "arm_lower_right.001_end_end", "arm_lower_left.001_end_end", true, 3f, 5f, .1f, .58f)
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
                null
            )

        },


    };
}



public class IkProfile : ScriptableObject {

    // string names of body parts in body transform
    public string name_head, name_hips, name_footRight, name_footLeft, name_toeRight, name_toeLeft, name_handRight, name_handLeft, name_fingerRight, name_fingerLeft;

    public bool quadripedal;
    public float runCycle_strideFrequency;
    public float runCycle_lerpTightness;
    public float runCycle_limbVerticalDisplacement;
    public float runCycle_limbForwardReachDistance;

    public static IkProfile InstantiateIkProfile(string name_head, string name_hips, string name_footRight, string name_footLeft, string name_toeRight, string name_toeLeft, string name_handRight, string name_handLeft, string name_fingerRight, string name_fingerLeft, bool quadripedal, float runCycle_strideFrequency, float runCycle_lerpTightness, float runCycle_limbVerticalDisplacement, float runCycle_limbForwardReachDistance){
        IkProfile ikProfile = ScriptableObject.CreateInstance<IkProfile>();
        ikProfile.name_head = name_head;
        ikProfile.name_hips = name_hips;
        ikProfile.name_footRight  = name_footRight;
        ikProfile.name_footLeft = name_footLeft;
        ikProfile.name_toeRight = name_toeRight;
        ikProfile.name_toeLeft = name_toeLeft;
        ikProfile.name_handRight = name_handRight;
        ikProfile.name_handLeft = name_handLeft;
        ikProfile.name_fingerRight = name_fingerRight;
        ikProfile.name_fingerLeft = name_fingerLeft;
        ikProfile.quadripedal = quadripedal;
        ikProfile.runCycle_strideFrequency = runCycle_strideFrequency;
        ikProfile.runCycle_lerpTightness = runCycle_lerpTightness;
        ikProfile.runCycle_limbVerticalDisplacement = runCycle_limbVerticalDisplacement;
        ikProfile.runCycle_limbForwardReachDistance = runCycle_limbForwardReachDistance;
        return ikProfile;
    }










}




