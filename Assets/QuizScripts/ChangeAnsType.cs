using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChangeAnsType : MonoBehaviour
{
    public TMP_Text changePopupText;
    public TMP_Dropdown changeDropdown;
    public SlideManager slideManager;
   public bool ignoreAction;

    public void WantChange(TMP_Dropdown d, SlideManager m)
    {
        if (ignoreAction)
        {
            ignoreAction = false;
            gameObject.SetActive(false);
            return;
        }

        Debug.Log(d.value + " ans type");
        Debug.Log("current words = " + changePopupText.text);

        changeDropdown = d;
        slideManager = m;

        SlideManager.Slides man = slideManager.slidesList[slideManager.currentSlide];


        switch (d.value)

        {
            case 0:
                changePopupText.text = "Change Answer Type to Text?";
                break;

            case 1:
                changePopupText.text = "Change Answer Type to Media?";
                break;

            case 2:
                changePopupText.text = "Change Answer Type to Text & Media?";
                break;

        }

        


    }

    public void ClickYes()
    {
        slideManager.ChangeAns(changeDropdown);
        gameObject.SetActive(false);
    }

    public void ClickNo()
    {
        SlideManager.Slides man = slideManager.slidesList[slideManager.currentSlide];

        if (man.slideType == SlideManager.Slides.Type.text)
        {
            changeDropdown.value = 0;
        }
        else if (man.slideType == SlideManager.Slides.Type.image)
        {
            changeDropdown.value = 1;
        }
        else if (man.slideType == SlideManager.Slides.Type.both)
        {
            changeDropdown.value = 2;
        }

        slideManager = null;
        changeDropdown = null;
        gameObject.SetActive(false);
    }
}
