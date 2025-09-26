using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using UnityEditor;
using Unity.VisualScripting;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ButtonPress;

public class SlotHolderInteract : MonoBehaviour
{
    public Sprite placeholderImage;

    public Image slotImage;
    private bool imageSet = false;

    // Start is called before the first frame update
    void Start()
    {
        var button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);

    }

    private void OnTriggerEnter(Collider collision)
    {
        Debug.Log("Triggered SlotHolder");

        if (!imageSet)
        {
            Sprite image = collision.GetComponent<Image>().sprite;

            if (slotImage.sprite != image)
            {
                imageSet = true;

                UndoRedo.instance.Action();

                slotImage.sprite = image;

                //Saving Image Path
                SaveImage(image);
            }
        }
    }
    void Update()
    {
        // Detect mouse button 0 (left click) up
        if (Input.GetMouseButtonUp(0))
        {
            if (imageSet)
            {
                imageSet = false;
            }
        }
    }

    private void OnClick()
    {
        if (slotImage.sprite != placeholderImage) //imageSet
        {
            imageSet = false;

            UndoRedo.instance.Action();

            slotImage.sprite = placeholderImage;

            //Save Placeholder Image
            resetSaveImage();

        }
    }

    private void SaveImage(Sprite image)
    {
        // Get the project-relative path (e.g. "Assets/Textures/file.png")
        string assetPath = AssetDatabase.GetAssetPath(image);

        Debug.Log("Asset Path: " + assetPath);
        // Convert to absolute system path
        //string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
        string absolutePath = Path.GetFullPath(assetPath);

        Debug.Log("Full Path: " + absolutePath);

        slotImage.gameObject.GetComponent<CanvasOpenVideoAndImage>().TestPath(absolutePath);
    }

    private void resetSaveImage()
    {
        slotImage.gameObject.GetComponent<CanvasOpenVideoAndImage>().RemoveImage();
    }

}
