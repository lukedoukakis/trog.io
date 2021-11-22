using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{

    public static MaterialController instance;

    [SerializeField] Material[] clothingMaterials;



    void Awake()
    {
        instance = this;
    }

    public Material GetRandomClothingMaterial()
    {
        return clothingMaterials[UnityEngine.Random.Range(0, clothingMaterials.Length)];
    }



    


}
