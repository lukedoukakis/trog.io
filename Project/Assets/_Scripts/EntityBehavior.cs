using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EntityBehavior : EntityComponent
{

    public Transform home;
    static float distanceThreshhold_home = 10f;
    static float distanceThreshhold_spot = 4f;
    static float distanceThreshhold_point = 2f;
    public static float randomOffsetRange = 1f;
    Vector3 randomOffset;


    // sensing and movement parameters
    public static float senseDistance_obstacle = 1f;
    public static float senseDistance_immediate = .25f;
    public static float senseDistance_search = 15f;
    public static float senseDistance_earshot = 50f;
    public static float senseDistance_infinite = 500f;
    public static float maxJumpFromDistance = 3f;
    public static float rotationSpeed = 1f;
    public bool running;

    public List<Item> surroundingItems;


    public Action activeAction;
    public List<Action> actions;
    public enum Priority{
        Back, Front, FrontImmediate
    }

    public enum Command{
        Idle, Go_home, Follow_player, Collect_item, Find_weapon
    }

    public Dictionary<string, Action> actionLayers;
    public Dictionary<string, IEnumerator> coroutineLayers;



    void Awake(){
        handle = GetComponent<EntityHandle>();
        handle.entityBehavior = this;
        home = GameObject.FindGameObjectWithTag("Home").transform;
        randomOffset = new Vector3(UnityEngine.Random.Range(randomOffsetRange*-1f, randomOffsetRange), 0f, UnityEngine.Random.Range(randomOffsetRange*-1f, 0));
        actionLayers = new Dictionary<string, Action>{
            {"Movement", null},
            {"Hands", null},
        };
        coroutineLayers = new Dictionary<string, IEnumerator>{
            {"Movement", null},
            {"Hands", null},
        };
    }

    void Start(){

    }



    // primary method to be used for queueing actions
    public void ProcessCommand(int command, int priority){
        Action a = CreateAction(command);
        switch(priority){
            case (int)Priority.Back :
                AddAction(a);
                break;
            case (int)Priority.Front :
                InsertAction(a);
                break;
            case (int)Priority.FrontImmediate :
                InsertActionImmediate(a, true);
                break;
        }
        //Debug.Log("ProcessCommand() done");
    }

    public Action CreateAction(int command){
        Action a = new Action();
        switch(command){
            case (int)Command.Idle :
                a.type = (int)Action.ActionTypes.Idle;
                break;
            case (int)Command.Go_home :
                a.type = (int)Action.ActionTypes.GoTo;
                a.obj = home.gameObject;
                break;
            case (int)Command.Follow_player :
                a.type = (int)Action.ActionTypes.Follow;
                a.obj = Player.current.gameObject;
                break;
            case (int)Command.Collect_item :
                a.type = (int)Action.ActionTypes.Collect;
                break;
            case 777 :
                a.type = (int)Action.ActionTypes.Collect;
                a.item_target = Item.Spear;
                //Log(a.item_target.nme);
                break;
            case 888 :
                a.type = (int)Action.ActionTypes.Collect;
                a.item_target = Item.Stone;
                //Log(a.item_target.nme);
                break;         
            default :
            //Debug.Log("ObjectBehavior: no action for command specified");
                break;
        }
        //Debug.Log("CreateAction() done");
        return a;    
    }

    // add an action to the end of the queue
    void AddAction(Action a){
        actions.Add(a);
    }

    // insert an action to the front of the queue, to be executed when the current action is finished
    void InsertAction(Action a){
        if(actions.Count > 0){
            actions.Insert(0, a);
        }
        else{
            actions.Add(a);
        }
    }

    // insert an action to the front of the queue and immediately execute
    void InsertActionImmediate(Action a, bool clear){
        if(clear){
            if(actions.Count > 0){
                actions.Clear();
            }
        }
        InsertAction(a);
        OnActionInterrupt();
        NextAction();
        //Debug.Log("InsertActionImmediate() done");
    }



    // select and execute the next action in the queue... if list is empty, insert "go home" or "idle" action
    public Action NextAction(){
        if(actions.Count == 0){
            ProcessCommand((int)Command.Follow_player, (int)Priority.Front);
        }
        Action next = actions[0];
        actions.RemoveAt(0);
        activeAction = next;
        ExecuteAction(activeAction);
        return next;
    }
    public void OnActionInterrupt(){

    }

    public void ExecuteAction(Action a){
        switch(a.type){
            case (int)Action.ActionTypes.Idle :
                Idle(a);
                break;
            case (int)Action.ActionTypes.GoTo :
                GoTo(a);
                break;
            case (int)Action.ActionTypes.Follow :
                Follow(a);
                break;
            case (int)Action.ActionTypes.Collect :
                Collect(a);
                break;
            case (int)Action.ActionTypes.Pickup :
                Pickup(a);
                break;
            case (int)Action.ActionTypes.Attack :
                Attack(a);
                break;
            case (int)Action.ActionTypes.Build :
                Build(a);
                break;
            case (int)Action.ActionTypes.Hunt :
                Hunt(a);
                break;
            default:
                Debug.Log("ObjectBehavior: called action not a defined action (" + a.type + ")... idling.");
                break;
        }

        //NextAction();
    }



    


    public void Idle(Action a){

    }

    public void GoTo(Action a){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _GoTo());

        IEnumerator _GoTo()
        {

            Transform targetT = a.obj.transform;   
            while (true)
            {
                if (!IsAtPosition(targetT.position, distanceThreshhold_point)){
                    NavigateTowards(targetT);
                }
                else
                {
                    TerminateActionLayer("Movement");
                    NextAction();
                }
                yield return null;
            }
        }
    }

    public void Follow(Action a){
        TerminateActionLayer("Movement");
        BeginActionLayer("Movement", a, _Follow());

        IEnumerator _Follow()
        {

            Transform targetT = a.obj.transform;   
            while (true)
            {
                if (!IsAtPosition(targetT.position, distanceThreshhold_spot))
                {
                    NavigateTowards(targetT);
                }
                yield return null;
            }
        }
    }

    public void Collect(Action a){

        Item i_target = a.item_target;
        Log("target name: " + i_target.nme);

        List<GameObject> foundObjects = SenseSurroundingItems(i_target.type, i_target.nme, senseDistance_infinite);
        if(foundObjects.Count == 0){
            // TODO: search in new area if nothing found
            Log("Collect: nothing found");
            NextAction();
        }
        else{
            Log("Collect: picking up object");
            GameObject target = foundObjects[0];
            Action gotoObject = new Action((int)(Action.ActionTypes.GoTo), target, -1, Item.GetItemByName(target.name), null, 60);
            Action pickupObject = new Action((int)(Action.ActionTypes.Pickup), target, -1, Item.GetItemByName(target.name), null, 60);
            InsertActionImmediate(gotoObject, false);
            InsertAction(pickupObject);
        }
    }

    public void Pickup(Action a){
        TakeFromGround(a.obj);
        NextAction();
    }

    public void Attack(Action a){

    }

    public void Build(Action a){

    }

    public void Hunt(Action a){

    }


    void TerminateActionLayer(string layer){
        IEnumerator current = coroutineLayers[layer];
        if(current != null){
            StopCoroutine(current);
        }
        actionLayers[layer] = null;
        coroutineLayers[layer] = null;
    }

    void BeginActionLayer(string layer, Action a, IEnumerator coroutine){
        actionLayers[layer] = a;
        coroutineLayers[layer] = coroutine;
        StartCoroutine(coroutine);
    }


    void NavigateTowards(Transform targetT){
		
        float leftDistance, centerDistance, rightDistance;
        RaycastHit leftHitInfo, centerHitInfo, rightHitInfo;
		Vector3 path = transform.position - targetT.position;
		path.y = 0;

			
		// if obstacle in front and it's not the player object
		if(SenseObstacle()){
				
			// if obstacle can't be jumped over, navigate around it
			if(!CanClearObstacle()){
				TurnTowardsMostOpenPath();
			}
			else{	
				
				// if close enough to obstacle and on the ground, jump
				if(Mathf.Min(Mathf.Min(leftDistance, centerDistance), rightDistance) < maxJumpFromDistance){
					if(handle.entityPhysics.CanJump()){
						handle.entityPhysics.Jump();
					}
				}
			}
		}

        float offsetMultipler;
        Rigidbody targetRb = targetT.GetComponent<Rigidbody>();
        if(targetRb == null){
            offsetMultipler = 1f;
        }
        else{
            offsetMultipler = (Mathf.InverseLerp(0f, handle.entityPhysics.maxSpeed, targetT.GetComponent<Rigidbody>().velocity.magnitude) + 1f) * 5f;
        }
        Vector3 tp = targetT.position;
        offsetMultipler = 1f;
        tp += targetT.TransformDirection(randomOffset * offsetMultipler);
        RotateToward(tp, .05f);
		
		// move forward
		handle.entityPhysics.Move(Vector3.forward, handle.entityPhysics.acceleration);



        bool SenseObstacle(){

            // set raycasts to reach castDistance units away
            Transform gs = handle.entityPhysics.groundSense;
            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized, out leftHitInfo, senseDistance_obstacle);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, senseDistance_obstacle);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized, out rightHitInfo, senseDistance_obstacle);

            List<RaycastHit> hitInfos = new List<RaycastHit>();

            // set leftDistance, centerDistance, rightDistance
            if (leftCast){ leftDistance = (leftHitInfo.point - transform.position).magnitude; hitInfos.Add(leftHitInfo); }
            else{ leftDistance = int.MaxValue; }
            if (centerCast){ centerDistance = (centerHitInfo.point - transform.position).magnitude; hitInfos.Add(centerHitInfo); }
            else{ centerDistance = int.MaxValue; }
            if (rightCast){ rightDistance = (rightHitInfo.point - transform.position).magnitude; hitInfos.Add(rightHitInfo); }
            else{ rightDistance = int.MaxValue; }

            // return true if any of the raycasts hit something besides a tribe member
            int hits = 0;
            foreach(RaycastHit hitInfo in hitInfos){
                string tag = hitInfo.collider.gameObject.tag;
                if(tag != "TribeMember" && tag != "Player"){
                    hits++;
                }
            }
            return hits >= 1;

        }

        bool CanClearObstacle(){
            Transform ohs = handle.entityPhysics.obstacleHeightSense;
            if
            (
                // !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized * 1f, out leftHitInfo, senseDistance/2)
                // && !Physics.Raycast(ohs.position, transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, senseDistance*2)
                // && !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized * 1f, out rightHitInfo, senseDistance/2)
                !Physics.BoxCast(ohs.position, new Vector3(.5f, .01f, .5f), transform.forward, transform.rotation, Mathf.Max(leftDistance, centerDistance, rightDistance))
            )
            {
                return true;
            }
            return false;
        }

        void TurnTowardsMostOpenPath(){

            Quaternion leftRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left) * 1f, Vector3.up);
            Quaternion rightRot = Quaternion.LookRotation(transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right) * 1f, Vector3.up);
            if (leftDistance < rightDistance){
                handle.entityPhysics.RotateTowards(rightRot, rotationSpeed);
            }
            else{
                handle.entityPhysics.RotateTowards(leftRot, rotationSpeed);
            }
        }

        void RotateToward(Vector3 targetPos, float magnitude){
            Vector3 p = transform.position - targetPos;
            p.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(p * -1, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, magnitude);
        }
	}

    void TakeFromGround(GameObject o){
        Log("TakeFromGround()");
        Item item = Item.GetItemByName(o.name);
        Tuple<Item, GameObject> pair = new Tuple<Item, GameObject>(item, o);
        switch(item.type){
            case (int)Item.Type.Misc:
                handle.entityItems.SetHolding(pair);
                break;
            case (int)Item.Type.Weapon:
                handle.entityItems.SetWeapon(pair);
                break;
            case (int)Item.Type.Container:
                handle.entityItems.SetHolding(pair);
                break;
            case (int)Item.Type.Pocket:
                handle.entityItems.PocketItem(item);
                break;
        }
        handle.entityAnimation.Pickup(item);
        //o.transform.position = o.transform.position += new Vector3(UnityEngine.Random.Range(-30f, 30f), 1f, UnityEngine.Random.Range(-30f, 30f));
    }

    public List<GameObject> SenseSurroundingItems(int type, string name, float distance){
        Collider[] colliders = Physics.OverlapSphere(transform.position, distance, LayerMask.GetMask("Item"));
       
        string sur = "";
        List<GameObject> foundObjects = new List<GameObject>();
        GameObject o;
        foreach(Collider col in colliders){
            o = col.gameObject;
            if(type == -1 || Item.GetItemByName(o.name).type == type){
                if(name == null || o.name == name){
                    foundObjects.Add(o);
                    sur += o.name + ", ";
                }
            }
        }
        //Debug.Log("Surroundings: " + sur);

        // order by proximity to entity
        foundObjects = foundObjects.OrderBy(c => Vector3.Distance(transform.position, c.transform.position)).ToList();


        return foundObjects;
        
    }



    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }


    GameObject CreateTemporaryObject(Vector3 location){
        GameObject o = new GameObject("temp_" + handle.entityInfo.ID);
        o.transform.position = location;
        return o;
    }

    // Update is called once per frame
    void Update()
    {   
        if(tag == "Player"){
            if(Input.GetKeyUp(KeyCode.K)){
                MainCommand.current.SendCommand(777);
            }
            if(Input.GetKeyUp(KeyCode.L)){
                MainCommand.current.SendCommand(888);
            }
        }


        
    }




}
