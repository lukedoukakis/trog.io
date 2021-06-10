﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class CameraController : MonoBehaviour
{
    public Transform playerT;
    Transform followT;
    public Transform focusT;
    public float cameraDistance_baked;
    public float cameraDistance_input;
    
    public static CameraController current;

    [SerializeField] float sensitivity_rotation;
    [SerializeField] float sensitivity_zoom;
    [SerializeField] float featureCullDistance;
    [SerializeField] float smallFeatureCullDistance;
    [SerializeField] bool showCursor;
    float acceleration;

    Vector3 targetPos;
    Vector3 targetLookAt;
    float posModifier;

    void Awake(){
        current = this;
    }
    // Start is called before the first frame update
    void Start()
    {
    
    }

    public void Init(Transform t){
        playerT = t;
        followT = GameObject.Instantiate(new GameObject(), playerT.position, Quaternion.identity).transform;
        Cursor.visible = showCursor;
        Application.targetFrameRate = -1;
        QualitySettings.vSyncCount = 1;
        float[] cullDistances = new float[32];
        cullDistances[10] = featureCullDistance;
        cullDistances[11] = smallFeatureCullDistance;
        Camera.main.layerCullDistances = cullDistances;
        posModifier = 0f;
        cameraDistance_input = 1f;
        //RandomSpawn();
    }


    public void AdjustCamera(int mode){

        ZoomInput();

        // static camera
        if (mode == 0)
        {
            Vector3 targetPos = playerT.position + (Vector3.forward * .07f) + (Vector3.up * .09f);
            Camera.main.transform.position = Vector3.Lerp(Camera.main.transform.position, targetPos, 50f * Time.deltaTime);
            Camera.main.transform.rotation = Quaternion.Euler(new Vector3(45f, 0f, 0f));
        }

        // dynamic camera
        else if (mode == 1)
        {

            float pi = Mathf.PI;

            posModifier += Input.GetAxis("Mouse Y") * -1f * sensitivity_rotation * Time.fixedDeltaTime;

            // above
            if (posModifier > .48f)
            {
                posModifier = .48f;
            }

            // below
            if (posModifier < -.48f)
            {
                posModifier = -.48f;
            }

            cameraDistance_baked = 1f - (Mathf.Min(0f, posModifier) * -1.5f);
            float cameraDistance_combined = cameraDistance_baked * cameraDistance_input;

            followT.position = Vector3.Lerp(followT.position, playerT.position + Vector3.up*3f*cameraDistance_combined, 18f * Time.deltaTime);
            targetPos = Vector3.Lerp(targetPos, followT.position + (Mathf.Cos(posModifier * pi) * playerT.forward * -7f * cameraDistance_combined) + (Mathf.Sin(posModifier * pi) * Vector3.up * 4f * cameraDistance_combined), 50f * Time.deltaTime);
            Camera.main.transform.position = targetPos;
            targetLookAt = Vector3.Lerp(targetLookAt, followT.position, 50f * Time.deltaTime);
            Camera.main.transform.LookAt(targetLookAt);
        
        
        }
    }

    void ZoomInput(){
        float zoomDelta = Input.mouseScrollDelta.y * sensitivity_zoom;
        float targetZoom = Mathf.Clamp(cameraDistance_input - zoomDelta, .2f, 1f);
        cameraDistance_input = Mathf.Lerp(cameraDistance_input, targetZoom, 40f * Time.deltaTime);
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
