using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WallManager : MonoBehaviour
{
    public List<Button> buts;
    public List<GameObject> walls;
    public int current;

    public int prev;

    public TMP_InputField title;

    //private void Start()
    //{
    //    if(DataAcrossScenes.instance!= null)
    //    {
    //        title.text = DataAcrossScenes.instance.projectName;
    //        //DataAcrossScenes.instance.projectName = "";
    //    }

        
    //    OnOffWall();
    //}

    public void WallChange(int i)
    {
        UndoRedo.instance.Action();
        current = i;
        OnOffWall();
    }


    public void OnOffWall()
    {
        foreach (SlideManager slideWall in FindObjectsOfType<SlideManager>())
        {
            slideWall.transform.parent.gameObject.SetActive(false);
        }


        foreach (Button b in buts)
        {
            b.image.color = Color.white;
        }

        walls[current].SetActive(true);
        walls[current].GetComponentInChildren<SlideManager>().ReloadSlide();
        buts[current].image.color = new Color(0.75f, 0.75f, 0.75f);
        AudioCheck();
    }

    public void AudioCheck()
    {

        if (String.IsNullOrEmpty(FindObjectOfType<EditorManager>().bgmFileName))
        {
            walls[current].GetComponentInChildren<SlideManager>().bgmText.text = "Add Audio...";
            walls[current].GetComponentInChildren<SlideManager>().removeBGMButton.SetActive(false);
        }
        else
        {
            walls[current].GetComponentInChildren<SlideManager>().bgmText.text = FindObjectOfType<EditorManager>().bgmFileName;
            walls[current].GetComponentInChildren<SlideManager>().removeBGMButton.SetActive(true);
        }
    }
}
