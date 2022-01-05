using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;


public enum Species{ Any, Human, Bear, Deer
}
public enum BehaviorType{ None, Aggressive, Timid, Steadfast }

public class EntityInfo : EntityComponent
{

    public int id;
    public string nickname;
    public Species species;
    public Faction faction;
    public bool isFactionLeader;
    public bool isFactionFollower;
    public SpeciesInfo speciesInfo;


    protected override void Awake()
    {
        this.fieldName = "entityInfo";

        base.Awake();

        Init();
    
    }

    public void Init(){
        id = UnityEngine.Random.Range(0, int.MaxValue);
        if(species != Species.Any){
            speciesInfo = SpeciesInfo.GetSpeciesInfo(species);
            faction = speciesInfo.baseFaction;
            isFactionLeader = false;
            isFactionFollower = false;
        }
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
    public GameObject onHitParticlesPrefab;

    public static SpeciesInfo InstantiateSpeciesInfo(Faction baseFaction, ItemCollection baseDrop, Stats baseStats, IkProfile ikProfile, BehaviorProfile behaviorProfile, GameObject onHitParticlesPrefab){
        SpeciesInfo speciesInfo = ScriptableObject.CreateInstance<SpeciesInfo>();
        speciesInfo.baseFaction = baseFaction;
        speciesInfo.baseDrop = baseDrop;
        speciesInfo.baseStats = baseStats;
        speciesInfo.ikProfile = ikProfile;
        speciesInfo.behaviorProfile = behaviorProfile;
        speciesInfo.onHitParticlesPrefab = onHitParticlesPrefab;
        
        return speciesInfo;
    }

    public static SpeciesInfo GetSpeciesInfo(Species spec){
        //Debug.Log("GetSpeciesInfo(): " + spec.ToString());
        return SpeciesInfoDict[spec];
    }

    static Dictionary<Species, SpeciesInfo> SpeciesInfoDict = new Dictionary<Species, SpeciesInfo>()
    {
        {
            Species.Human, SpeciesInfo.InstantiateSpeciesInfo
            (
                Faction.InstantiateFaction(Species.Human.ToString()),
                new ItemCollection(
                    new Dictionary<Item, int>{

                    }
                ),
                Stats.InstantiateStats(1f,1f,1f,.7f,.38f,1f,.5f,1f,1f,1f,1f,1f),
                IkProfile.InstantiateIkProfile("B-head", "B-hips", "B-foot_R", "B-foot_L", "B-toe_R", "B-toe_L", "B-palm_01_R", "B-palm_01_L", "B-f_index_01_R", "B-f_index_01_L", true, false, true, 3f, 8f, 2f, .5f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Steadfast,
                    new List<AttackType>(){AttackType.Weapon},
                    new List<ActionType>(){ ActionType.StepSide },
                    .5f,
                    true,
                    false,
                    true
                ),
                ParticleController.instance.BloodSpatter
            )
        },

        {
            Species.Bear, SpeciesInfo.InstantiateSpeciesInfo(
                Faction.InstantiateFaction(Species.Bear.ToString()),
                new ItemCollection(
                    new Dictionary<Item, int>{
                        {Item.CarcassBear, 1},
                    }
                ),
                Stats.InstantiateStats(2f, .5f, 6f, .5f, .4f, 1f, .25f, 1f, 1f, 1f, 1f, 10f),
                IkProfile.InstantiateIkProfile("head", "spine_lower", "leg_lower_right_end", "leg_lower_left_end", "", "", "arm_lower_right_end", "arm_lower_left_end", "", "", false, true, false, 3f, 10f, 1f, .75f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Steadfast,
                    new List<AttackType>(){AttackType.Swipe},
                    new List<ActionType>(){ ActionType.StepSide },
                    .75f,
                    false,
                    false,
                    false
                ),
                ParticleController.instance.BloodSpatter
            )

        },

        {
            Species.Deer, SpeciesInfo.InstantiateSpeciesInfo(
                Faction.InstantiateFaction(Species.Deer.ToString()),
                new ItemCollection(
                    new Dictionary<Item, int>{
                        // todo: deer carcass
                        {Item.CarcassBear, 1},
                    }
                ),
                Stats.InstantiateStats(.01f, .75f, .1f, .5f, .35f, 1f, .25f, 1f, 1f, 1f, 1f, 10f),
                IkProfile.InstantiateIkProfile("head", "spine_lower", "leg_lower_right_end_end", "leg_lower_left_end_end", "", "", "arm_lower_right_end_end_end", "arm_lower_left_end_end_end", "", "", false, true, false, 3f, 10f, 8f, .7f),
                BehaviorProfile.InstantiateBehaviorProfile(
                    BehaviorType.Timid,
                    new List<AttackType>(){ AttackType.Swipe },
                    new List<ActionType>(){ },
                    .75f,
                    false,
                    false,
                    false
                ),
                ParticleController.instance.BloodSpatter
            )

        },


    };
}



public class IkProfile : ScriptableObject
{

    // string names of body parts in body transform
    public string name_head, name_hips, name_footRight, name_footLeft, name_toeRight, name_toeLeft, name_handRight, name_handLeft, name_fingerRight, name_fingerLeft;
    public bool useAnimationMovement;
    public bool quadripedal;
    public bool hasFingersAndToes;
    public float runCycle_strideFrequency;
    public float runCycle_lerpTightness;
    public float runCycle_limbVerticalDisplacement;
    public float runCycle_limbForwardReachDistance;

    public static IkProfile InstantiateIkProfile(string name_head, string name_hips, string name_footRight, string name_footLeft, string name_toeRight, string name_toeLeft, string name_handRight, string name_handLeft, string name_fingerRight, string name_fingerLeft, bool useAnimationMovement, bool quadripedal, bool hasFingersAndToes, float runCycle_strideFrequency, float runCycle_lerpTightness, float runCycle_limbVerticalDisplacement, float runCycle_limbForwardReachDistance)
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
        ikProfile.useAnimationMovement = useAnimationMovement;
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
    public List<ActionType> attackRecoverySequence;
    public float lungePower;
    public bool canJump;
    public bool domesticatable;
    public bool requiresRest;



    public static BehaviorProfile InstantiateBehaviorProfile(BehaviorType behaviorType, List<AttackType> attackTypes, List<ActionType> attackRecoverySequence, float lungePower, bool canJump, bool domesticatable, bool requiresRest)
    {
        BehaviorProfile bp = ScriptableObject.CreateInstance<BehaviorProfile>();
        bp.behaviorType = behaviorType;
        bp.attackTypes = attackTypes;
        bp.attackRecoverySequence = attackRecoverySequence;
        bp.lungePower = lungePower;
        bp.canJump = canJump;
        bp.domesticatable = domesticatable;
        bp.requiresRest = requiresRest;

        return bp;
    }

}




