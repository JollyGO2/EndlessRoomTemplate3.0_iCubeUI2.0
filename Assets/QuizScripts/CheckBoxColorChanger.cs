using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckBoxColorChanger : MonoBehaviour
{
    [SerializeField] Color checkedColor = Color.green;
    [SerializeField] Color unCheckedColor = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        ChangeColorAccordingly();
    }

    public void ChangeColorAccordingly()
    {
        Toggle thisToggle = GetComponent<Toggle>();
        if (thisToggle.isOn)
        {
            thisToggle.targetGraphic.GetComponent<Image>().color = checkedColor;
        }
        else
        {
            thisToggle.targetGraphic.GetComponent<Image>().color = unCheckedColor;
        }
    }

}
