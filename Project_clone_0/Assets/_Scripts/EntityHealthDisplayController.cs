using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityHealthDisplayController : MonoBehaviour
{


    public Transform healthBar, staminaBar;
    public Transform healthBarScaleT, staminaBarScaleT;


    void Awake()
    {
        healthBar = Utility.FindDeepChild(transform, "HealthBar");
        staminaBar = Utility.FindDeepChild(transform, "StaminaBar");

        healthBarScaleT = Utility.FindDeepChild(healthBar, "ScaleT");
        staminaBarScaleT = Utility.FindDeepChild(staminaBar, "ScaleT");
    }


    public void UpdateDisplay(EntityHandle entityHandle)
    {
        //Debug.Log("Updating display");
        SetHealthBarFillAmount(entityHandle.entityStats.health / entityHandle.entityStats.maxHealth);
        SetStaminaBarFillAmount(entityHandle.entityStats.stamina / entityHandle.entityStats.maxStamina);
        transform.LookAt(Camera.main.transform);
    }

    void SetHealthBarFillAmount(float amount)
    {
        //Debug.Log(amount);
        healthBarScaleT.localScale = new Vector3(amount, 1f, 1f);
    }

    void SetStaminaBarFillAmount(float amount)
    {
        //Debug.Log(amount);
        staminaBarScaleT.localScale = new Vector3(amount, 1f, 1f);
    }


    public void Show()
    {
        transform.localScale = Vector3.one;
    }

    public void Hide()
    {
        transform.localScale = Vector3.one;
    }



}
