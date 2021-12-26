using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraController : MonoBehaviour
{
    

    public static float CAMERA_DISTANCE_OUTSIDECAMP = 4f;
    public static float CAMERA_DISTANCE_INSIDECAMP = .3f;
    public static float CAMERA_ZOOM_SPEED_CAMPTRANSITION = 4f;
    public static float CAMERA_LOCK_VERTICALITY = .3f;


    public Transform playerT;
    Transform followT;
    public Transform focusT;
    public float cameraDistance_baked;
    public float cameraDistance_input;
    public bool lockVerticalCameraMovement;
    public IEnumerator smoothZoomCoroutine;
    
    public static CameraController current;

    public Vector3 defaultCameraOffset;

    [SerializeField] float sensitivity_rotation;
    [SerializeField] float sensitivity_zoom;
    [SerializeField] float cullDistance_feature;
    [SerializeField] float cullDistance_smallFeature;
    [SerializeField] float cullDistance_creature;
    [SerializeField] float cullDistance_item;
    float acceleration;

    Vector3 targetPos;
    Vector3 targetLookAt;
    float posModifier;
    Vector3 targetOffset;
    Vector3 currentOffset;
    bool targetOffsetReached;

    void Awake(){
        current = this;
        SetBakedCameraDistance(CAMERA_DISTANCE_OUTSIDECAMP);
        SetLockVerticalCameraMovement(true);
    }
    // Start is called before the first frame update
    void Start()
    {
        SetTargetOffset(defaultCameraOffset);
    }

    public void Init(Transform t){
        playerT = t;
        followT = GameObject.Instantiate(new GameObject(), playerT.position, Quaternion.identity).transform;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        float[] cullDistances = new float[32];
        cullDistances[10] = cullDistance_feature;
        cullDistances[11] = cullDistance_smallFeature;
        cullDistances[12] = cullDistance_creature;
        cullDistances[13] = cullDistance_item;
        Camera.main.layerCullDistances = cullDistances;
        posModifier = 0f;
        cameraDistance_input = 1f;
        //RandomSpawn();
    }


    public void AdjustCamera(int mode){

        // static camera
        if (mode == 0)
        {


            if(!UIController.UImode){
                posModifier += Input.GetAxis("Mouse Y") * -1f * sensitivity_rotation * Time.fixedDeltaTime;
                ZoomInput();
            }

            Vector3 targetPos = playerT.position + (Vector3.forward * -6f) + (Vector3.up * 4f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 50f * Time.deltaTime);
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(30f, 0f, 0f));
        }

        // dynamic camera
        else if (mode == 1)
        {

            float pi = Mathf.PI;

            if(!UIController.UImode)
            {
                if(lockVerticalCameraMovement)
                {
                    posModifier = Mathf.Lerp(posModifier, CAMERA_LOCK_VERTICALITY, CAMERA_ZOOM_SPEED_CAMPTRANSITION * Time.deltaTime);
                }
                else
                {
                    posModifier += Input.GetAxis("Mouse Y") * -1f * sensitivity_rotation * Time.fixedDeltaTime;
                }
                //ZoomInput();
            }

            float max = .48f;
            float min = -.3f;

            // above
            if (posModifier > max)
            {
                posModifier = max;
            }

            // below
            if (posModifier < min)
            {
                posModifier = min;
            }
            float cameraDistance_combined = cameraDistance_baked * cameraDistance_input;

            followT.position = Vector3.Lerp(followT.position, playerT.position + Vector3.up * 3f * cameraDistance_combined, 22f * Time.deltaTime);
            targetPos = Vector3.Lerp(targetPos, followT.position + (Mathf.Cos(posModifier * pi) * playerT.forward * -14f * cameraDistance_combined) + (Mathf.Sin(posModifier * pi) * Vector3.up * 4f * cameraDistance_combined), 50f * Time.deltaTime);
            Camera.main.transform.position = targetPos;
            targetLookAt = Vector3.Lerp(targetLookAt, followT.position, 50f * Time.deltaTime);
            Camera.main.transform.LookAt(targetLookAt);

            ApplyOffset();

        
        
        }
    }

    void ApplyOffset(){
        if(!targetOffsetReached){
            if(Vector3.Distance(targetOffset, currentOffset) > .01f){
                currentOffset = Vector3.Lerp(currentOffset, targetOffset, 10f * Time.deltaTime);
            }
            else{
                targetOffsetReached = true;
            }
        }
        
        Camera.main.transform.position += Camera.main.transform.TransformDirection(currentOffset);
    }

    void ZoomInput(){
        float zoomDelta = Input.mouseScrollDelta.y * sensitivity_zoom;
        float targetZoom = Mathf.Clamp(cameraDistance_input - zoomDelta, .2f, 1f);
        cameraDistance_input = Mathf.Lerp(cameraDistance_input, targetZoom, 40f * Time.deltaTime);
    }

    public void SetTargetOffset(Vector3 offset){
        targetOffset = offset;
        targetOffsetReached = false;
    }

    public void SetBakedCameraDistance(float targetValue)
    {
        cameraDistance_baked = targetValue;
    }

    public void SetBakedCameraDistanceSmooth(float targetValue, float speed)
    {

        //Debug.Log("SetBakedCameraDistanceSmooth");

        if(smoothZoomCoroutine != null)
        {
            StopCoroutine(smoothZoomCoroutine);
        }
        smoothZoomCoroutine = _SetBakedCameraDistanceSmooth(targetValue, speed);
        StartCoroutine(smoothZoomCoroutine);

        IEnumerator _SetBakedCameraDistanceSmooth(float targetValue, float speed)
        {
            //Debug.Log("_SetBakedCameraDistanceSmooth");
            float v = cameraDistance_baked;
            while(Mathf.Abs(v - targetValue) > .01f)
            {
                v = Mathf.Lerp(cameraDistance_baked, targetValue, speed * Time.deltaTime);
                SetBakedCameraDistance(v);
                yield return null;
            }  
        }
    }

    public void SetLockVerticalCameraMovement(bool targetValue)
    {
        lockVerticalCameraMovement = targetValue;
    }


    void Update()
    {

        if(playerT != null){
            AdjustCamera(GameManager.cameraMode);
        }
    }

    void FixedUpdate()
    {
        

    }









    // void RandomSpawn(){
    //     bool landHit = false;
    //     Vector3 randomPos = Vector3.zero;
    //     int i = 0;
    //     while(!landHit){
    //         randomPos = new Vector3(Random.Range(-1000f, 1000f), 0f, Random.Range(-1000f, 1000f)) + Vector3.up * (ChunkGenerator.ElevationAmplitude*.82f);
    //         landHit = Mathf.PerlinNoise((randomPos.x - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale, (randomPos.z - ChunkGenerator.Seed + .01f) / ChunkGenerator.ElevationMapScale) >= .5f;
    //         i++;

    //         if(i > 1000){
    //             Debug.Log(":(");
    //             break;
    //         }
    //     } 
    //     MainCamera.transform.position = randomPos;
    //     MainCamera.transform.rotation = Quaternion.Euler(15f, 45f, 0f);
    // }
}
