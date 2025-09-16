using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainPopup : MonoBehaviour
{
    public Image image;
    public TMP_Text details;
    public TMP_Text title;
    public MainPageItem hov;

    public GameObject leftcheck;
    public GameObject leftar;
    public GameObject rightcheck;
    public GameObject rightar;

    public void Exit()
    {
        if (hov)
        {
            hov.OffPopup();
        }
    }
}
