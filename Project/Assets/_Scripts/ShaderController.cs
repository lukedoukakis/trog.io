using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;

    [SerializeField] Material TerrainMaterial;
    [SerializeField] Material GrassMaterial;
    [SerializeField] Material[] RockMaterials;
    [SerializeField] Material[] SnowMaterials;
    [SerializeField] Material[] FadeMaterials;


    void Awake()
    {

        instance = this;
        InitShaderSettings();

    }

    void InitShaderSettings()
    {
        TerrainMaterial.SetFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude + .05f);
        GrassMaterial.SetFloat("_WaterHeight", ChunkGenerator.SeaLevel * ChunkGenerator.ElevationAmplitude + .5f);
        GrassMaterial.SetFloat("_GrassNormal", ChunkGenerator.GrassNormal);
        GrassMaterial.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
        GrassMaterial.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        foreach(Material mat in SnowMaterials){
            //mat.SetFloat("_SnowMinimumSurfaceNormal", ChunkGenerator.SnowNormal);
            mat.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
            mat.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        }
    }

    void UpdateShaderSettings()
    {
        UpdateFadeMaterials();
        UpdateRockMaterials();
    }

    void UpdateFadeMaterials()
    {
        foreach (Material mat in FadeMaterials)
        {
            mat.SetVector("_TargetVector", GameManager.current.localPlayerHandle.entityPhysics.hips.position);
        }
    }

    void UpdateRockMaterials()
    {
        Vector3 refPosition = Camera.main.transform.position;
        ChunkData cd = ChunkGenerator.GetChunk(refPosition);
        if(cd == null){ return; }

        Vector2 coordinatesOnChunk = ChunkGenerator.GetChunkCoordinates(refPosition);
        float temperatureAtCoordinates = 0;
        try{
            temperatureAtCoordinates = cd.TemperatureMap[(int)coordinatesOnChunk.x, (int)coordinatesOnChunk.y];
        }
        catch
        {
            Debug.Log(coordinatesOnChunk);
        }
        

        foreach (Material mat in RockMaterials)
        {
            mat.SetFloat("_Temperature", temperatureAtCoordinates);
        }
    }

    public void UpdateGrassShaderSettings(Camp camp)
    {
        Vector3 campOrigin = camp.origin;
        float radius = camp.radius;
        GrassMaterial.SetVector("_CampOrigin", campOrigin);
        GrassMaterial.SetFloat("_CampRadius", radius);
    }

    void Update()
    {
        UpdateShaderSettings();
    }
}
