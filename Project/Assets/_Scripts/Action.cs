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
    public int item;

    // maximum time to be spent executing the action
    public int maxSeconds;

    public Action(int _type, GameObject _obj, int _number, int _item, int _maxSeconds){
        type = _type;
        obj = _obj;
        number = _number;
        item = _item;
        maxSeconds = _maxSeconds;
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



    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
