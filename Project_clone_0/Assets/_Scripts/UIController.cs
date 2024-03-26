using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public static bool UImode;
    public static bool cursorActive;



    public Transform canvas;
    public GameObject screen_mainMenu, screen_hud;







    public static UIController instance;
    void Awake()
    {
        instance = this;
        SetUIMode(true);
        SetScreen(screen_mainMenu);
    }



    public void SetScreen(GameObject screen)
    {
        foreach (Transform t in canvas)
        {
            t.gameObject.SetActive(false);
        }
        screen.SetActive(true);
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


    public void OnButtonPress_StartGame()
    {
        StartGame();
    }

    void StartGame()
    {
        SetScreen(screen_hud);
        ClientCommand.instance.OnGameStart();
    }

    




    // Start is called before the first frame update
    void Start()
    {
        //SetUIMode(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
