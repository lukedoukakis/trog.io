using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShaderController : MonoBehaviour
{
    
    public static ShaderController instance;

    [SerializeField] Material TerrainMaterial;
    [SerializeField] Material GrassMaterial;
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
            mat.SetFloat("_SnowMinimumSurfaceNormal", ChunkGenerator.SnowNormal);
            mat.SetFloat("_SnowHeightStart", (ChunkGenerator.SnowLevel - .13f) * ChunkGenerator.ElevationAmplitude);
            mat.SetFloat("_SnowHeightCap", ChunkGenerator.SnowLevel * ChunkGenerator.ElevationAmplitude);
        }
    }

    public void UpdateGrassShaderSettings(Camp camp)
    {
        Vector3 campOrigin = camp.origin;
        float radius = camp.radius;
        GrassMaterial.SetVector("_CampOrigin", campOrigin);
        GrassMaterial.SetFloat("_CampRadius", radius);
    }

    void UpdateShaderSettings()
    {
        foreach (Material mat in FadeMaterials)
        {
            mat.SetVector("_TargetVector", GameManager.current.localPlayerHandle.entityPhysics.obstacleHeightSense.position);
        }
    }

    void Update()
    {
        UpdateShaderSettings();
    }
}
