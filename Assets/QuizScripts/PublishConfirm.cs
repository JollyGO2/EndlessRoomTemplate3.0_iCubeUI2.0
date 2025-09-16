using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using System;

public class PublishConfirm : MonoBehaviour
{
    public Image thumbnailImage;
    public TMP_InputField detailsInput;
    
    public void SaveThumbnail()
    {
        Sprite itemBGSprite = thumbnailImage.sprite;
        Texture2D itemBGTex = itemBGSprite.texture;
        byte[] itemBGBytes = itemBGTex.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Thumbnail.png");
        Debug.Log(path);

        File.WriteAllBytes(path, itemBGBytes);
    }

    public void SaveDetails()
    {
        string savePath = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "ProjectDetails.txt");
        ES3.Save("Details", detailsInput.text, savePath);

        string copyDetailFrom = savePath;
        string copyDetailTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName ,"ProjectDetails.txt");

        if (File.Exists(copyDetailTo))
        {
            File.Delete(copyDetailTo);
        }

        File.Copy(copyDetailFrom, copyDetailTo);
    }

    public void PublishSave()
    {

    }
}
