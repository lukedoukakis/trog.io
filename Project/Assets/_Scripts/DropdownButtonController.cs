using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropdownButtonController : MonoBehaviour
{

    public ButtonDropdownController bdc;
    public ObjectSelectionManager referencedOSM;



    public void SetFromObject(ObjectSelectionManager refOSM){
        referencedOSM = refOSM;
    }

    public void OnXButtonPress(){
        if(bdc != null){
            bdc.RemoveButton(this.gameObject);
            GlobalSelectionController.current.RemoveFromSelected(referencedOSM);
        }
        else{
            Debug.Log("DropdownButtonController: No referenced ButtonDropdownController!");
        }
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
