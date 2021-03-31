using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{

    ObjectStats stats;
    public MeshRenderer renderer;
    public Material mat_none, mat_selecting, mat_selected;

    public bool selecting;
    public bool selected;
    public bool tooltip;

    public void Awake(){
        stats = GetComponent<ObjectStats>();
    }


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

    public void ShowTooltip(){
        tooltip = true;
        TooltipController.current.SetText(stats.CreateStatsList());
        TooltipController.current.Show(-1);
    }


    void OnMouseOver(){
        //Debug.Log("mo");

        UIEvents.current.OnUnitMouseOver(this);


    }

    void OnMouseExit(){
        tooltip = false;
        TooltipController.current.Hide();
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
