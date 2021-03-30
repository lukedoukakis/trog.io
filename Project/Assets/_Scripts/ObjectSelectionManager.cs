using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{


    public MeshRenderer renderer;
    public Material mat_none, mat_selecting, mat_selected;

    public bool selecting;
    public bool selected;


    public void SetSelecting(bool b){
        selecting = b;
        if(b){
            renderer.sharedMaterial = mat_selecting;
        }
    }
    public void SetSelected(bool b){
        selected = b;
        if(b){
            renderer.sharedMaterial = mat_selected;
        }
        else{
            renderer.sharedMaterial = mat_none;
        }
    }


    void OnMouseOver(){
        //Debug.Log("mo");

        UIEvents.current.OnUnitMouseOver(this);


    }











    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
