using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;
    [SerializeField] float distanceDropMagnitude;


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
        UpdateDistanceDrop();

    }


    void UpdateFadeMaterials()
    {
        if(ClientCommand.instance.clientPlayerCharacterHandle == null){ return; }     
        Shader.SetGlobalVector("_TargetVector", ClientCommand.instance.clientPlayerCharacterHandle.entityPhysics.hips.position);
    }

    void UpdateWaterSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude - 100f);
    }

    void UpdateGrassNormalSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_GrassNormal", ChunkGenerator.GrassNormal);
    }

    void UpdateSnowSensitiveMaterials()
    {
        Shader.SetGlobalFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel) * ChunkGenerator.ElevationAmplitude);
        Shader.SetGlobalFloat("_SnowHeightCap", 1f * ChunkGenerator.ElevationAmplitude);
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

    void UpdateDistanceDrop()
    {
        //float magnitude = Mathf.Lerp(.25f, 1f, 1f - Mathf.InverseLerp(0f, 40f, CameraController.instance.distanceFromPlayer));
        Shader.SetGlobalFloat("_DistanceDropMagnitude", distanceDropMagnitude);
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
