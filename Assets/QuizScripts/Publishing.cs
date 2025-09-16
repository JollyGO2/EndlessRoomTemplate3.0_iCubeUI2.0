using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;


public class Publishing : MonoBehaviour
{
    public string details;
    public TMP_InputField detailsInput;
    public TMP_InputField nameInput;
    public Image thumbnailImage;

    private void OnEnable()
    {
        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Thumbnail.png");


        thumbnailImage.sprite = FindObjectOfType<ThumbnailMaking>().outputSprite;
    }

    public void Enter()
    {
        details = detailsInput.text;
        
        //donotdestroy and bring back to main page?
    }

    public void Publish()
    {
        
        DataAcrossScenes.instance.projectDetails = details;

        SceneManager.LoadScene(0);
    }

}
