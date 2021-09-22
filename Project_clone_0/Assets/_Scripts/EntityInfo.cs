using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum Species{ Any, Human, Bear, Tree, Deer
}
public enum BehaviorType{ None, Aggressive, Timid, Steadfast }

public class EntityInfo : EntityComponent
{

    public int id;
    public string nickname;
    public Species species;
    public Faction faction;
    public SpeciesInfo speciesInfo;


    protected override void Awake(){

        base.Awake();

        Init();
    
    }

    public void Init(){
        id = UnityEngine.Random.Range(0, int.MaxValue);
        speciesInfo = SpeciesInfo.GetSpeciesInfo(species);
        faction = speciesInfo.baseFaction;
        FindAndSetEntityReferences();
    }

}


// to hold references to things like base stats, drops etc for all species
public class SpeciesInfo : ScriptableObject{
    public Faction baseFaction;
    public ItemCollection baseDrop;
    public Stats baseStats;
    public IkProfile ikProfile;
    public BehaviorProfile behaviorProfile;

    public static SpeciesInfo InstantiateSpeciesInfo(Faction baseFaction, ItemCollection baseDrop, Stats baseStats, IkProfile ikProfile, BehaviorProfile behaviorProfile){
        SpeciesInfo si = ScriptableObject.CreateInstance<SpeciesInfo>();
        si.baseFaction = baseFaction;
        si.baseDrop = baseDrop;
        si.baseStats = baseStats;
        si.ikProfile = ikProfile;
        si.behaviorProfile = behaviorProfile;
        
        return si;
    }

    public static SpeciesInfo GetSpeciesInfo(Species spec){
        //Debug.Log(spec.ToString());
        return SpeciesInfoDict[spec];
    }

    static Dictionary<Species, SpeciesInfo> SpeciesInfoDict = new Dictionary<Species, SpeciesInfo>(){
        {
            Species.Human, SpeciesInfo.InstantiateSpeciesInfo(
                Faction.InstantiateFaction(Species.Human.ToString(), false),
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: drops for human
                    }
                    
                ),
                Stats.BASE_HUMAN,
                IkProfile.InstantiateIkProfile("B-head", "B-hips", "B-foot_R", "B-foot_L", "B-toe_R", "B-toe_L", "B-palm_01_R", "B-palm_01_L", "B-f_index_01_R", "B-f_index_01_L", false, true, 3f, 5f, 1f, .58f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Aggressive,
                    new List<AttackType>(){AttackType.Weapon},
                    new List<ActionParameters>(){ ActionParameters.GenerateActionParameters(ActionType.Follow, null, -1, null, null, -1, EntityBehavior.distanceThreshold_combat, EntityOrientation.BodyRotationMode.Target, true)},
                    0f,
                    false)            
            )

        },

        {
            Species.Bear, SpeciesInfo.InstantiateSpeciesInfo(
                Faction.InstantiateFaction(Species.Bear.ToString(), false),
                new ItemCollection(
                    new Dictionary<Item, int>{
                        {Item.CarcassBear, 1}
                    }
                    
                ),
                Stats.BASE_BEAR,
                IkProfile.InstantiateIkProfile("head", "spine_lower", "leg_lower_right_end", "leg_lower_left_end", "", "", "arm_lower_right_end", "arm_lower_left_end", "", "", true, false, 3f, 10f, 2.25f, .58f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Aggressive,
                    new List<AttackType>(){AttackType.Swipe},
                    new List<ActionParameters>(){ },
                    .75f,
                    false)
            )

        },

        {
            Species.Deer, SpeciesInfo.InstantiateSpeciesInfo(
                Faction.InstantiateFaction(Species.Deer.ToString(), false),
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: deer carcass
                        {Item.CarcassBear, 1}
                    }
                    
                ),
                Stats.BASE_DEER,
                IkProfile.InstantiateIkProfile("head", "spine_lower", "leg_lower_right_end_end", "leg_lower_left_end_end", "", "", "arm_lower_right_end_end_end", "arm_lower_left_end_end_end", "", "", true, false, 3f, 10f, 2.25f, .58f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Timid,
                    new List<AttackType>(){ AttackType.Swipe },
                    new List<ActionParameters>(){ },
                    .75f,
                    false)
            )

        },

        {
            Species.Tree, SpeciesInfo.InstantiateSpeciesInfo(
                null,
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: finish drop for tree
                        {Item.LogFir, 1},
                    }
                ),
                Stats.BASE_TREE,
                null,
                null
            )

        },


    };
}



public class IkProfile : ScriptableObject
{

    // string names of body parts in body transform
    public string name_head, name_hips, name_footRight, name_footLeft, name_toeRight, name_toeLeft, name_handRight, name_handLeft, name_fingerRight, name_fingerLeft;

    public bool quadripedal;
    public bool hasFingersAndToes;
    public float runCycle_strideFrequency;
    public float runCycle_lerpTightness;
    public float runCycle_limbVerticalDisplacement;
    public float runCycle_limbForwardReachDistance;

    public static IkProfile InstantiateIkProfile(string name_head, string name_hips, string name_footRight, string name_footLeft, string name_toeRight, string name_toeLeft, string name_handRight, string name_handLeft, string name_fingerRight, string name_fingerLeft, bool quadripedal, bool hasFingersAndToes, float runCycle_strideFrequency, float runCycle_lerpTightness, float runCycle_limbVerticalDisplacement, float runCycle_limbForwardReachDistance)
    {
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
        ikProfile.hasFingersAndToes = hasFingersAndToes;
        ikProfile.runCycle_strideFrequency = runCycle_strideFrequency;
        ikProfile.runCycle_lerpTightness = runCycle_lerpTightness;
        ikProfile.runCycle_limbVerticalDisplacement = runCycle_limbVerticalDisplacement;
        ikProfile.runCycle_limbForwardReachDistance = runCycle_limbForwardReachDistance;

        return ikProfile;
    }

}

public class BehaviorProfile : ScriptableObject
{

    public BehaviorType behaviorType;
    public List<AttackType> attackTypes;
    public List<ActionParameters> attackRecoverySequence;
    public float lungePower;
    public bool domesticatable;



    public static BehaviorProfile InstantiateBehaviorProfile(BehaviorType behaviorType, List<AttackType> attackTypes, List<ActionParameters> attackRecoverySequence, float lungePower, bool domesticatable)
    {
        BehaviorProfile bp = ScriptableObject.CreateInstance<BehaviorProfile>();
        bp.behaviorType = behaviorType;
        bp.attackTypes = attackTypes;
        bp.attackRecoverySequence = attackRecoverySequence;
        bp.lungePower = lungePower;
        bp.domesticatable = domesticatable;

        return bp;
    }

}




