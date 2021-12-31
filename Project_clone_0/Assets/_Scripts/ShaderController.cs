﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;    


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

    }


    void UpdateFadeMaterials()
    {
        if(GameManager.instance.localPlayerHandle == null){ return; }     
        Shader.SetGlobalVector("_TargetVector", GameManager.instance.localPlayerHandle.entityPhysics.hips.position);
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
        if(GameManager.instance.localPlayer != null)
        {
            Vector3 playerPos = GameManager.instance.localPlayer.transform.position;
            Shader.SetGlobalVector("_PlayerPosition", playerPos);
        }   
    }

    void UpdateCameraDistanceSensitiveMaterials()
    {
        //Shader.SetGlobalVector("_CameraDistance", playerPos);
    }

    void Update()
    {
        //UpdateFadeMaterials();
        //UpdateDesertSensitiveMaterials();
        //UpdateGrassNormalSensitiveMaterials();
        UpdatePlayerPositionSensitiveMaterials();
    }
}
