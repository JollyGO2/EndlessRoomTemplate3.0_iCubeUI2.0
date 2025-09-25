using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.Serialization;
using System;

public class ButtonPress : MonoBehaviour
{
    [Serializable]
    public class whenPressed : UnityEvent { };
    public whenPressed whenPress = new whenPressed();

    private bool buttonPressed;


    public void OnPointerDown(PointerEventData eventData)
    {
        if (!buttonPressed)
        {
            buttonPressed = true;
            whenPress.Invoke();
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        buttonPressed = false;
    }
}
