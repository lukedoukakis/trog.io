using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static bool UImode;
    public static bool cursorActive;


    public static UIController current;
    void Awake()
    {
        current = this;
        SetUIMode(false);
    }

    public void UpdateSelectionMenu(){
        UnitMenuController.current.ClearButtons();
        foreach(EntityHandle handle in GlobalSelectionController.current.SelectedHandles.ToArray()){
            UnitMenuController.current.AddButton(handle);
        }
    }

    public void ClearSelectionMenu(){
        UnitMenuController.current.ClearButtons();

    }

    public void SetUIMode(bool active){
        UImode = active;
        cursorActive = active;

        if(active){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else{
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        
    }
    public void ToggleUIMode(){
        SetUIMode(!UImode);
        CommandEveryoneController.current.OnHornPress();
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
