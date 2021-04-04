using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSelectionManager : MonoBehaviour
{

    public ObjectStats stats;
    public ObjectBehavior behavior;
    public MeshRenderer renderer;
    public Material mat_none, mat_selecting, mat_selected;

    public bool selecting;
    public bool selected;
    public bool tooltip;

    public void Awake(){
        stats = GetComponent<ObjectStats>();
        behavior = GetComponent<ObjectBehavior>();
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
        TooltipController.current.Show(TooltipController.DefaultDelay);
    }
    public void HideTooltip(){
        tooltip = false;
        TooltipController.current.Hide();
    }


    void OnMouseOver(){
        UIEvents.current.OnUnitMouseOver(this);
    }

    void OnMouseExit(){
        UIEvents.current.OnUnitMouseExit(this);
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
