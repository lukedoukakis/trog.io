using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Action : MonoBehaviour
{

    // type of action
    public int type;

    // gameobject to interact with
    public GameObject obj;

    // ambiguous number of things
    public int number;

    // target item
    public Item item_target;

    // resultant item
    public Item item_result;


    // maximum time to be spent executing the action
    public int maxSeconds;

    public Action(int _type, GameObject _obj, int _number, Item _item_target, Item _item_result, int _maxSeconds){
        type = _type;
        obj = _obj;
        number = _number;
        item_target = _item_target;
        item_result = _item_result;
        maxSeconds = _maxSeconds;
    }
    public Action(){
        type = -1;
        obj = null;
        number = -1;
        item_result = null;
        item_target = null;
        maxSeconds = -1;
    }



    public enum ActionTypes{
        Idle,
        GoTo,
        Follow,
        Collect,
        Pickup,
        Attack,
        Swing,
        Build,
        Hunt
    }
}
