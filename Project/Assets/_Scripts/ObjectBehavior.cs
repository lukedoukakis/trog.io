using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectBehavior : MonoBehaviour
{

    ObjectPhysics physics;
    ObjectStats stats;


    public Transform home;
    static float distanceThreshhold_home = 10f;
    static float distanceThreshhold_spot = 2f;
    public static float randomOffsetRange = 1f;
    Vector3 randomOffset;


    // sensing and movement parameters
    public static float senseDistance = 3f;
    public static float maxJumpFromDistance = 3f;
    public static float acceleration = .1f;
    public static float rotationSpeed = 1f;
    public bool running;


    public Action activeAction;
    public List<Action> actions;
    public enum Priority{
        Back, Front, FrontImmediate
    }

    public enum Command{
        Idle, Go_home, Follow_player
    }
    IEnumerator coroutine_movement;
    IEnumerator coroutine_hands;


    // temp
    GameObject tempObject;




    public void Awake(){
        physics = GetComponent<ObjectPhysics>();
        stats = GetComponent<ObjectStats>();
        home = GameObject.FindGameObjectWithTag("Home").transform;
        randomOffset = new Vector3(Random.Range(randomOffsetRange*-1f, randomOffsetRange), 0f, Random.Range(randomOffsetRange*-1f, randomOffsetRange));
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
        Action a = new Action(-1, null, -1, -1, -1);
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
            if(IsAtPosition(home.position, distanceThreshhold_spot)){
                ProcessCommand((int)Command.Idle, (int)Priority.Front);
            }
            else{
                ProcessCommand((int)Command.Go_home, (int)Priority.Front);
            }
        }
        Action next = actions[0];
        actions.RemoveAt(0);
        activeAction = next;
        ExecuteAction(activeAction);
        //Debug.Log("NextAction() done");
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
					if(physics.CanJump()){
						physics.Jump();
					}
				}
			}
		}
        RotateToward(targetT.position + randomOffset, .05f);
		
		// move forward
		physics.Move(Vector3.forward, acceleration);



        bool SenseObstacle(){

            float castDistance = senseDistance;

            // set raycasts to reach castDistance units away
            Transform gs = physics.groundSense;
            bool leftCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized, out leftHitInfo, castDistance);
            bool centerCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), transform.TransformDirection(Vector3.forward).normalized * 1f, out centerHitInfo, castDistance);
            bool rightCast = Physics.Raycast(transform.position + new Vector3(0, .1f, 0), (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized, out rightHitInfo, castDistance);

            List<RaycastHit> hitInfos = new List<RaycastHit>();

            // set leftDistance, centerDistance, rightDistance
            if (leftCast){ leftDistance = (leftHitInfo.point - transform.position).magnitude; hitInfos.Add(leftHitInfo); }
            else{ leftDistance = int.MaxValue; }
            if (centerCast){ centerDistance = (centerHitInfo.point - transform.position).magnitude; hitInfos.Add(centerHitInfo); }
            else{ centerDistance = int.MaxValue; }
            if (rightCast){ rightDistance = (rightHitInfo.point - transform.position).magnitude; hitInfos.Add(rightHitInfo); }
            else{ rightDistance = int.MaxValue; }

            // return true if any of the raycasts hit something besides a tribe member
            foreach(RaycastHit hitInfo in hitInfos){
                string tag = hitInfo.collider.gameObject.tag;
                if(tag != "TribeMember" && tag != "Player"){
                    return true;
                }
            }
            return false;

        }

        bool CanClearObstacle(){
            Transform ohs = physics.obstacleHeightSense;
            if (
                !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.left)).normalized * 1f, out leftHitInfo, senseDistance / 2)
                && !Physics.Raycast(ohs.position, transform.TransformDirection(Vector3.forward).normalized * 1f, out leftHitInfo, senseDistance * 2)
                && !Physics.Raycast(ohs.position, (transform.TransformDirection(Vector3.forward) + transform.TransformDirection(Vector3.right)).normalized * 1f, out rightHitInfo, senseDistance / 2)
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
                physics.RotateTowards(rightRot, rotationSpeed);
            }
            else{
                physics.RotateTowards(leftRot, rotationSpeed);
            }
        }

        void RotateToward(Vector3 targetPos, float magnitude){
            Vector3 p = transform.position - targetPos;
            p.y = 0;
            Quaternion targetRot = Quaternion.LookRotation(p * -1, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, magnitude);
        }
	}



    public void Idle(Action a){

    }

    public void GoTo(Action a){
        TerminateMovement();
        coroutine_movement = _GoTo(a, false);
        StartCoroutine(coroutine_movement);
    }

    public void Follow(Action a){
        TerminateMovement();
        coroutine_movement = _GoTo(a, true);
        StartCoroutine(coroutine_movement);
    }

    public void Collect(Action a){
        
    }

    public void Attack(Action a){

    }

    public void Build(Action a){

    }

    public void Hunt(Action a){

    }


    void TerminateMovement(){
        if(coroutine_movement != null){
            StopCoroutine(coroutine_movement);
            coroutine_movement = null;
        }
        if(tempObject != null){
            GameObject.Destroy(GameObject.Find("temp_" + stats.id));
        }
    }



    public bool IsAtPosition(Vector3 position, float distanceThreshhold){
        return Vector3.Distance(transform.position, position) < distanceThreshhold;
    }





    IEnumerator _GoTo(Action a, bool follow){
        Transform t = gameObject.transform;
        Transform targetT;
        if (follow) { targetT = a.obj.transform; }
        else
        {
            tempObject = new GameObject("temp_" + stats.id);
            tempObject.transform.position = a.obj.transform.position;
            targetT = tempObject.transform;
        }
        while(true)
        {
            if(Vector3.Distance(t.position, targetT.position) > (distanceThreshhold_spot)){
                NavigateTowards(targetT);
            }
            else{
                if(!follow){
                    //Debug.Log("Destination reached");
                    TerminateMovement();
                }
            }
            yield return null;
        }
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
