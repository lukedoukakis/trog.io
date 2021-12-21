using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;


    [SerializeField] Material WaterMaterial;


    [SerializeField] Material[] GrassNormalSensitiveMaterials;
    [SerializeField] Material[] WaterSensitiveMaterials;
    [SerializeField] Material[] DesertSensitiveMaterials;
    [SerializeField] Material[] SnowSensitiveMaterials;
    [SerializeField] Material[] PlayerPositionSensitiveMaterials;
    [SerializeField] Material[] FadeMaterials;
    


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

        // foreach (Material mat in FadeMaterials)
        // {
        //     mat.SetVector("_TargetVector", GameManager.instance.localPlayerHandle.entityPhysics.hips.position);
        // }
    }

    void UpdateWaterSensitiveMaterials()
    {

        Shader.SetGlobalFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude - 100f);

        // foreach(Material mat in WaterSensitiveMaterials){
        //     mat.SetFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude - 100f);
        // }
    }

    void UpdateGrassNormalSensitiveMaterials()
    {

        // Vector3 refPosition = Camera.main.transform.position;
        // ChunkData cd = ChunkGenerator.GetChunkFromRawPosition(refPosition);
        // if(cd == null){ return; }

        // Vector2 coordinatesInChunk = ChunkGenerator.GetCoordinatesInChunk(refPosition);
        // float desertnessAtCoordinates = ChunkGenerator.CalculateDesertness(cd.TemperatureMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y], cd.HumidityMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y]);


        Shader.SetGlobalFloat("_GrassNormal", Mathf.Lerp(ChunkGenerator.GrassNormalMin, ChunkGenerator.GrassNormalMax, 1f));

        // foreach(Material mat in GrassNormalSensitiveMaterials){
        //     mat.SetFloat("_GrassNormal", Mathf.Lerp(ChunkGenerator.GrassNormalMin, ChunkGenerator.GrassNormalMax, desertnessAtCoordinates));
        // }
    }

    void UpdateSnowSensitiveMaterials()
    {

        Shader.SetGlobalFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
        Shader.SetGlobalFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);

        // foreach(Material mat in SnowSensitiveMaterials){
        //     //mat.SetFloat("_SnowMinimumSurfaceNormal", ChunkGenerator.SnowNormal);
        //     mat.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
        //     mat.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        // }
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

        // foreach (Material mat in DesertSensitiveMaterials)
        // {
        //     mat.SetFloat("_Desertness", desertnessAtCoordinates);
        // }
    }

    void UpdatePlayerPositionSensitiveMaterials()
    {
        if(GameManager.instance.localPlayer != null)
        {
            Vector3 playerPos = GameManager.instance.localPlayer.transform.position;
            Shader.SetGlobalVector("_PlayerPosition", playerPos);

            // foreach (Material mat in PlayerPositionSensitiveMaterials)
            // {
            //     mat.SetVector("_PlayerPosition", playerPos);
            // }
        }


        
        
    }

    void Update()
    {
        UpdateFadeMaterials();
        UpdateDesertSensitiveMaterials();
        //UpdateGrassNormalSensitiveMaterials();
        UpdatePlayerPositionSensitiveMaterials();
    }
}
