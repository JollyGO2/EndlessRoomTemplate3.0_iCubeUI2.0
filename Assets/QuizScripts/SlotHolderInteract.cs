using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotHolderInteract : MonoBehaviour
{
    public Sprite placeholderImage;

    public Image slotImage;
    private bool imageSet = false;

    public void OnPointerDown(PointerEventData eventData) { }

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
            UndoRedo.instance.Action();

            Sprite image = collision.GetComponent<Image>().sprite;
            slotImage.sprite = image;
            imageSet = true;

            //Saving Image Path
            Debug.Log("THIS IMAGE COLLIDE:" + image.name);
            SaveImage(image);
        }
    }

    private void OnClick()
    {
        if (imageSet)
        {
            UndoRedo.instance.Action();

            slotImage.sprite = placeholderImage;
            imageSet = false;

            //Save Placeholder Image
            SaveImage(placeholderImage);

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


}
