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

    // resultant item
    public string item_result;

    // target item
    public string item_target;

    // maximum time to be spent executing the action
    public int maxSeconds;

    public Action(int _type, GameObject _obj, int _number, string _item_result, string _item_target, int _maxSeconds){
        type = _type;
        obj = _obj;
        number = _number;
        item_result = _item_result;
        item_target = _item_target;
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
        Attack,
        Build,
        Hunt
    }
}
