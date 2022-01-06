using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


// to be added to target positions for entity movement for specifications on wwhat to do when the entity has reached it
public enum TargetPositionType
{
    RestingPoint,
    WarmHandsPoint,
    StandingPoint,
    SquattingPoint
}
public class TargetPositionComponent : MonoBehaviour
{
    
    public TargetPositionType targetPositionType;
    public ActionSequence actionSequence;


    void InitFromTargetPositionType(EntityHandle entityHandle)
    {

        List<Action> actions = new List<Action>();
        switch (targetPositionType)
        {
            case TargetPositionType.RestingPoint :
                actions.Add(entityHandle.entityBehavior.OnRestFrame);
                break;
            case TargetPositionType.WarmHandsPoint :
                // TODO: warm hands
                break;
            case TargetPositionType.StandingPoint :
                actions.Add(entityHandle.entityPhysics.AssertStanding);
                break;
            case TargetPositionType.SquattingPoint :
                actions.Add(entityHandle.entityPhysics.AssertSquatting);
                break;
            default:
                break;
        }

        actionSequence = ActionSequence.CreateActionSequence(actions.ToArray());
    }


    // to be called when an entity has reached this position as a target destination
    public void OnTargetPositionReached(EntityHandle handle)
    {
        InitFromTargetPositionType(handle);
        actionSequence.Execute();
    }




}
