using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;

    [SerializeField] Material GrassMaterial;

    [SerializeField] Material[] GrassNormalSensitiveMaterials;
    [SerializeField] Material[] WaterSensitiveMaterials;
    [SerializeField] Material[] DesertSensitiveMaterials;
    [SerializeField] Material[] SnowSensitiveMaterials;
    [SerializeField] Material[] FadeMaterials;


    void Awake()
    {

        instance = this;
        InitShaderSettings();

    }

    void InitShaderSettings()
    {

        GrassMaterial.SetFloat("_GrassNormal", ChunkGenerator.GrassNormalMin);

        UpdateWaterSensitiveMaterials();
        UpdateSnowSensitiveMaterials();
    }


    void UpdateFadeMaterials()
    {
        foreach (Material mat in FadeMaterials)
        {
            mat.SetVector("_TargetVector", GameManager.current.localPlayerHandle.entityPhysics.hips.position);
        }
    }

    void UpdateWaterSensitiveMaterials()
    {
        foreach(Material mat in WaterSensitiveMaterials){
            mat.SetFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude + .05f);
        }
    }

    void UpdateGrassNormalSensitiveMaterials()
    {

        Vector3 refPosition = Camera.main.transform.position;
        ChunkData cd = ChunkGenerator.GetChunkFromRawPosition(refPosition);
        if(cd == null){ return; }

        Vector2 coordinatesInChunk = ChunkGenerator.GetCoordinatesInChunk(refPosition);
        float humidity = cd.HumidityMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y];
        float targetGrassNormal = Mathf.Lerp(ChunkGenerator.GrassNormalMin, ChunkGenerator.GrassNormalMax, 1f - humidity);


        foreach(Material mat in GrassNormalSensitiveMaterials){
            mat.SetFloat("_GrassNormal", targetGrassNormal);
        }
    }

    void UpdateSnowSensitiveMaterials()
    {
        foreach(Material mat in SnowSensitiveMaterials){
            //mat.SetFloat("_SnowMinimumSurfaceNormal", ChunkGenerator.SnowNormal);
            mat.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
            mat.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        }
    }

    void UpdateDesertSensitiveMaterials()
    {
        Vector3 refPosition = Camera.main.transform.position;
        ChunkData cd = ChunkGenerator.GetChunkFromRawPosition(refPosition);
        if(cd == null){ return; }

        Vector2 coordinatesInChunk = ChunkGenerator.GetCoordinatesInChunk(refPosition);
        float desertnessAtCoordinates = 0;
        desertnessAtCoordinates = ChunkGenerator.CalculateDesertness(cd.TemperatureMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y], cd.HumidityMap[(int)coordinatesInChunk.x, (int)coordinatesInChunk.y]);
        
        
        foreach (Material mat in DesertSensitiveMaterials)
        {
            mat.SetFloat("_Desertness", desertnessAtCoordinates);
        }
    }

    public void UpdateGrassSettings(Camp camp)
    {
        Vector3 campOrigin = camp.origin;
        float radius = camp.radius;
        GrassMaterial.SetVector("_CampOrigin", campOrigin);
        GrassMaterial.SetFloat("_CampRadius", radius);
    }

    void Update()
    {
        UpdateFadeMaterials();
        UpdateDesertSensitiveMaterials();
        UpdateGrassNormalSensitiveMaterials();
    }
}
