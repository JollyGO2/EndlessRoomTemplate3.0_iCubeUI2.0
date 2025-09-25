using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System;

public class ButtonPressHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Image indicator;
    public float maxPressTime;


    [Serializable]
    public class ActionAfterPress : UnityEvent { }; 
    public ActionAfterPress afterPress = new ActionAfterPress();

    private bool buttonPressed;


    public void OnPointerDown(PointerEventData eventData)
    {
        buttonPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;

        OffIndicator();
    }


    void OffIndicator()
    {
        if (indicator) //destroy indicator if long press was released
        {
            buttonPressed = false;
            indicator.fillAmount = 0;
        }
    }

    private void Update()
    {
        if (buttonPressed == true)
        {
            indicator.fillAmount += Time.deltaTime / maxPressTime;

            if (indicator.fillAmount == 1) //when pressed long enough, do whatever event wanted
            {
                afterPress.Invoke();
                //do something
                OffIndicator();
            }
        }

    }
}