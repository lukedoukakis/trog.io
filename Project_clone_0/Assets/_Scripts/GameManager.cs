using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GameManager : MonoBehaviour
{

    public static GameManager instance;
    public int gameId;
    public GameObject localPlayer;
    public EntityHandle localPlayerHandle;



    // options
    public static int GAME_SETTINGS_CAMERA_MODE = 1;
    public static bool GAME_SETTINGS_AUTO_ATTACK = true;



    void Awake()
    {
        instance = this;
    }

    public void SetLocalPlayer(GameObject obj)
    {
        localPlayer = obj;
        localPlayerHandle = obj.GetComponent<EntityHandle>();
    }

    public void TransferPlayerStatus(EntityHandle newPlayerHandle)
    {
        SetLocalPlayer(newPlayerHandle.gameObject);
        CameraController.instance.SetPlayerTransform(newPlayerHandle.transform);

        // handle components
        EntityUserInput eui = gameObject.AddComponent<EntityUserInput>();
        ClientCommand cc = gameObject.AddComponent<ClientCommand>();


    }



    
 

}
