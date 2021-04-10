using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DropdownButtonController : MonoBehaviour
{

    public ButtonDropdownController bdc;
    public EntityHandle handle;
    public GameObject referencedObject;
    public EntityStats referencedObjectStats;
    public EntityInfo referencedObjectInfo;
    public Button button;
    public TextMeshProUGUI tmp;






    string label;






    void Awake(){
        button = GetComponent<Button>();
        tmp = button.GetComponentInChildren<TextMeshProUGUI>();
    }



    public void SetFromObject(EntityHandle handle){
        this.handle = handle;
        referencedObject = this.handle.gameObject;
        referencedObjectInfo = referencedObject.GetComponent<EntityInfo>();
        if(referencedObjectInfo.NAME == ""){
            label = referencedObjectInfo.TYPE;
        }
        else{
            label = referencedObjectInfo.NAME;
        }
        UpdateLook();
    }

    void UpdateLook(){
        tmp.text = label;
    }

    public void OnXButtonPress(){
        if(bdc != null){
            bdc.RemoveButton(this.gameObject);
            GlobalSelectionController.current.RemoveFromSelected(handle);
        }
        else{
            Debug.Log("DropdownButtonController: No referenced ButtonDropdownController!");
        }
    }

    public void OnButtonPointerEnter(){
        if(!handle.tooltip){
            handle.ShowTooltip();
        }
    }
    public void OnButtonPointerExit(){
        handle.HideTooltip();
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
