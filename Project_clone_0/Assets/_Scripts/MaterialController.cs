using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialController : MonoBehaviour
{

    public static MaterialController instance;
    public Material baseClothingMaterial;
    public Material selectedMaterial;
    public Material[] clothingMaterials;



    void Awake()
    {
        instance = this;
        clothingMaterials = new Material[100];

        Material newClothingMat;
        Color newColor;
        for(int i = 0; i < clothingMaterials.Length; ++i)
        {
            newClothingMat = new Material(baseClothingMaterial);
            newColor = UnityEngine.Random.ColorHSV(0f, 1f, .25f, .25f, .5f, .5f);
            newClothingMat.SetColor("_MainColor", newColor);
            clothingMaterials[i] = newClothingMat;

        }
    }

    public Material GetRandomClothingMaterial()
    {
        return clothingMaterials[UnityEngine.Random.Range(0, clothingMaterials.Length)];
    }



    


}
