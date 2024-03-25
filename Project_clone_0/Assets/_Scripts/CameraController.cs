using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CameraController : MonoBehaviour
{
    
    public UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset renderPipelineAsset;

    public static float CAMERA_DISTANCE_OUTSIDECAMP = 20f;
    public static float CAMERA_DISTANCE_INSIDECAMP = 20f;
    public static float CAMERA_ZOOM_SPEED_CAMPTRANSITION = 4f;
    public static float CAMERA_LOCK_VERTICALITY_OUTSIDECAMP = .15f;
    public static float CAMERA_LOCK_VERTICALITY_INSIDECAMP = .15f;
    public static float CAMERA_TARGET_FOV = 75f;
    public static float CAMERA_ZOOM_INPUT_SPEED = 15f;


    public Transform playerTransform;
    Transform followT;
    public Transform focusT;
    public float cameraDistance_baked;
    public float cameraDistance_input;
    public float cameraDistance_total;
    public bool lockVerticalCameraMovement;
    public float lockVerticalCameraMovement_verticality;
    public IEnumerator smoothZoomCoroutine;
    public float distanceFromPlayer;
    
    public static CameraController instance;

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
    float verticalityModifier;
    Vector3 targetOffset;
    Vector3 currentOffset;
    bool targetOffsetReached;

    void Awake(){
        instance = this;
        //renderPipelineAsset = GetComponent<UnityEngine.Rendering.Universal.UniversalRenderPipelineAsset>();
        SetBakedCameraDistance(CAMERA_DISTANCE_OUTSIDECAMP);
        //ShaderController.instance.SetDistanceDrop(ShaderController.DISTANCE_DROP_MIN);
        SetLockVerticalCameraMovement(false, CAMERA_LOCK_VERTICALITY_OUTSIDECAMP);
        Camera.main.fieldOfView = CAMERA_TARGET_FOV;

        followT = GameObject.Instantiate(new GameObject()).transform;
        Time.timeScale = 3f;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        float[] cullDistances = new float[32];
        cullDistances[10] = cullDistance_feature;
        cullDistances[11] = cullDistance_smallFeature;
        cullDistances[12] = cullDistance_creature;
        cullDistances[13] = cullDistance_item;
        Camera.main.layerCullDistances = cullDistances;
        verticalityModifier = 0f;
        cameraDistance_input = 1f;
        //RandomSpawn();


    }
    // Start is called before the first frame update
    void Start()
    {
        SetTargetOffset(defaultCameraOffset);
    }

    public void SetPlayerTransform(Transform t)
    {
        playerTransform = t;
    }

    public void UpdateCamera()
    {


        float pi = Mathf.PI;


        if (lockVerticalCameraMovement)
        {
            verticalityModifier = Mathf.Lerp(verticalityModifier, lockVerticalCameraMovement_verticality, CAMERA_ZOOM_SPEED_CAMPTRANSITION * Time.deltaTime);
        }
        else
        {
            // verticality from zoom
            verticalityModifier = Mathf.Lerp(CAMERA_LOCK_VERTICALITY_INSIDECAMP, CAMERA_LOCK_VERTICALITY_OUTSIDECAMP, cameraDistance_input);

            // // free verticality
            // if(GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.Mouse)
            // {
            //     verticalityModifier += Input.GetAxis("Mouse Y") * -1f * sensitivity_rotation * Time.deltaTime;
            // }
            // else if(GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.ArrowKeys)
            // {
            //     float up = Convert.ToSingle(Input.GetKey(KeyCode.UpArrow)) * -1f;
            //     float down = Convert.ToSingle(Input.GetKey(KeyCode.DownArrow));
            //     verticalityModifier += (up + down) * sensitivity_rotation * .25f * Time.deltaTime;
            // }
        }
        ZoomInput();
        

        float max = .48f;
        float min = 0f;

        // cap verticality from above
        if (verticalityModifier > max)
        {
            verticalityModifier = max;
        }

        // cap verticality from below
        if (verticalityModifier < min)
        {
            verticalityModifier = min;
        }
        float newTotalD = cameraDistance_baked * cameraDistance_input;
        // if(newTotalD != cameraDistance_total)
        // {
        //     float magnitude = Mathf.Lerp(ShaderController.DISTANCE_DROP_MAX, ShaderController.DISTANCE_DROP_MIN, Mathf.InverseLerp(0f, 60f, newTotalD));
        //     ShaderController.instance.SetDistanceDrop(magnitude);
        // }
        cameraDistance_total = newTotalD;

        followT.position = Vector3.Lerp(followT.position, playerTransform.position + Vector3.up * 1f, 22f * Time.deltaTime);
        targetPos = Vector3.Lerp(targetPos, followT.position + (Mathf.Cos(verticalityModifier * pi) * playerTransform.forward * -1f * Mathf.Pow(cameraDistance_total, 1f)) + (Mathf.Sin(verticalityModifier * pi)) * Vector3.up * 1f * cameraDistance_total, 50f * Time.deltaTime);
        
        // stay above terrain
        RaycastHit hit;
        float minHeightAboveGround = 1f;
        if(Physics.Raycast(targetPos + (Vector3.up * 1000f), Vector3.down, out hit, 1000f + minHeightAboveGround, LayerMaskController.TERRAIN))
        {
            Vector3 adjustedPos = hit.point + (Vector3.up * minHeightAboveGround);
            if(adjustedPos.y > targetPos.y)
            {
                targetPos = adjustedPos;
            }
        }
        
        Camera.main.transform.position = targetPos;
        targetLookAt = Vector3.Lerp(targetLookAt, followT.position, 50f * Time.deltaTime);
        Camera.main.transform.LookAt(targetLookAt);

        //ApplyOffset();
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

    void ZoomInput()
    {

        float zoomDelta;
        if (GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.Mouse)
        {
            zoomDelta = Input.mouseScrollDelta.y * sensitivity_zoom;
        }
        else if (GameManager.GAME_SETINGS_ROTATIONALINPUTMODE == RotationalInputMode.ArrowKeys)
        {
            float z_in = Convert.ToSingle(Input.GetKey(KeyCode.UpArrow));
            float z_out = Convert.ToSingle(Input.GetKey(KeyCode.DownArrow)) * -1f;
            zoomDelta = (z_in + z_out) * sensitivity_zoom * .2f;
        }
        else
        {
            zoomDelta = 0;
        }

        float targetZoom = Mathf.Clamp(cameraDistance_input - zoomDelta, .01f, 1f);
        cameraDistance_input = Mathf.Lerp(cameraDistance_input, targetZoom, CAMERA_ZOOM_INPUT_SPEED * Time.deltaTime);
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

    public void SetLockVerticalCameraMovement(bool targetValue, float verticality)
    {
        lockVerticalCameraMovement = targetValue;
        lockVerticalCameraMovement_verticality = verticality;
    }

    void UpdateRenderScale()
    {
        renderPipelineAsset.renderScale = Mathf.Lerp(.25f, .5f, Mathf.InverseLerp(0f, CAMERA_DISTANCE_OUTSIDECAMP, cameraDistance_total));
    }


    void Update()
    {

        if(playerTransform != null)
        {
            UpdateCamera();
            distanceFromPlayer = Vector3.Distance(Camera.main.transform.position, playerTransform.position);
        }


        //UpdateRenderScale();
        
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
