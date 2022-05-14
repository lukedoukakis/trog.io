using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;
    public static float DISTANCE_DROP_STATIC = 0f;
    public static float DISTANCE_DROP_MIN = 0f;
    public static float DISTANCE_DROP_MAX = 0f;

    [SerializeField] Material realisticGrassMaterial;

    void Awake()
    {

        instance = this;
        InitShaderSettings();

    }

    void InitShaderSettings()
    {


        UpdateGrassNormalSensitiveMaterials();
        UpdateWaterSensitiveMaterials();
        UpdateSnowSensitiveMaterials();
        SetDistanceDrop(DISTANCE_DROP_STATIC);
    }


    void UpdateFadeMaterials()
    {
        if(ClientCommand.instance.clientPlayerCharacterHandle == null){ return; }     
        Shader.SetGlobalVector("_TargetVector", ClientCommand.instance.clientPlayerCharacterHandle.entityPhysics.hips.position);
    }

    void UpdateWaterSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.Amplitude - 100f);
    }

    void UpdateGrassNormalSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_GrassNormal", ChunkGenerator.GrassNormal);
    }

    void UpdateSnowSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel) * ChunkGenerator.Amplitude);
        Shader.SetGlobalFloat("_SnowHeightCap", 1f * ChunkGenerator.Amplitude);
        Shader.SetGlobalFloat("_SnowMinimumSurfaceNormal", ChunkGenerator.SnowNormalMin);
        Shader.SetGlobalFloat("_SnowMaximumSurfaceNormal", ChunkGenerator.SnowNormalMax);
    }

    void UpdateDesertSensitiveMaterials()
    {
        Vector3 refPosition = Camera.main.transform.position;
        ChunkData cd = ChunkGenerator.GetChunkFromRawPosition(refPosition);
        if(cd == null){ return; }

        Vector2 coordinatesInChunk = ChunkGenerator.GetCoordinatesInChunk(refPosition);
        float desertnessAtCoordinates = 0;
        desertnessAtCoordinates = ChunkGenerator.CalculateDesertness(cd.TemperatureMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y], cd.HumidityMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y]);
        //Debug.Log(desertnessAtCoordinates);


        Shader.SetGlobalFloat("_Desertness", desertnessAtCoordinates);
    }

    void UpdatePlayerPositionSensitiveMaterials()
    {
        if(ClientCommand.instance.clientPlayerCharacter != null)
        {
            Vector3 playerPos = ClientCommand.instance.clientPlayerCharacter.transform.position;
            Shader.SetGlobalVector("_PlayerPosition", playerPos);
        }   
    }

    void UpdateCameraDistanceSensitiveMaterials()
    {
        //Shader.SetGlobalVector("_CameraDistance", playerPos);
    }

    public void SetDistanceDrop(float magnitude)
    {
        Shader.SetGlobalFloat("_DistanceDropMagnitude", magnitude);
    }


    void Update()
    {
        //UpdateFadeMaterials();
        //UpdateDesertSensitiveMaterials();
        //UpdateGrassNormalSensitiveMaterials();
        UpdatePlayerPositionSensitiveMaterials();
        //UpdateDistanceDrop();

    }
}
