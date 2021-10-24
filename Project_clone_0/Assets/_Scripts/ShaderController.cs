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

        GrassMaterial.SetFloat("_GrassNormal", ChunkGenerator.GrassNormal);

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
        foreach(Material mat in GrassNormalSensitiveMaterials){
            mat.SetFloat("_GrassNormal", ChunkGenerator.GrassNormal);
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
        ChunkData cd = ChunkGenerator.GetChunk(refPosition);
        if(cd == null){ return; }

        Vector2 coordinatesOnChunk = ChunkGenerator.GetChunkCoordinates(refPosition);
        float desertnessAtCoordinates = 0;
        desertnessAtCoordinates = ChunkGenerator.CalculateDesertness(cd.TemperatureMap[(int)coordinatesOnChunk.x, (int)coordinatesOnChunk.y], cd.HumidityMap[(int)coordinatesOnChunk.x, (int)coordinatesOnChunk.y]);
        
        
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
    }
}
