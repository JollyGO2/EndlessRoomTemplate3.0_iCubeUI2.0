using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotHolderInteract : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Sprite placeholderImage;

    public Image slotImage;
    private bool imageSet = false;
    [SerializeField] GameObject hoverObject;
    [SerializeField] GameObject toolTipObject;
    [SerializeField] PulsingUI pulsingUi;
    [SerializeField] PulsingUI interactUIPulsing;
    private Sprite spriteToCopy;

    // Start is called before the first frame update
    void Start()
    {
        var button = this.GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        hoverObject.SetActive(false);
        toolTipObject.SetActive(false); 
    }
    public void SetInteractableRemoveButton(bool interactable)
    {
        GetComponent<Button>().interactable = interactable; //This needs to change 
    }

    private void OnTriggerEnter(Collider collision)
    {
        spriteToCopy = collision.GetComponent<Image>().sprite;

        if(interactUIPulsing != null) 
            interactUIPulsing.SetPulsing(true);

        if (pulsingUi != null)
            pulsingUi.SetPulsing(false);
    }

    private void OnTriggerExit(Collider collision)
    {
        spriteToCopy = null;

        if (interactUIPulsing != null)
            interactUIPulsing.SetPulsing(false);

        if (pulsingUi != null)
            pulsingUi.SetPulsing(true);
    }

    void Update()
    {
        // Detect mouse button 0 (left click) up
        if (Input.GetMouseButtonUp(0))
        {
            //Debug.LogWarning("Mouse Up");
            if (imageSet)
            {
                imageSet = false;
            }

            if (!imageSet && spriteToCopy)
            {
                Sprite image = spriteToCopy;

                if (slotImage.sprite != image)
                {
                    imageSet = true;
                    spriteToCopy = null;
                    SetInteractableRemoveButton(true);

                    if (interactUIPulsing != null)
                        interactUIPulsing.SetPulsing(false);

                    if (pulsingUi != null)
                        pulsingUi.SetPulsing(false);

                    UndoRedo.instance.Action();

                    slotImage.sprite = image;

                    //Saving Image Path
                    SaveImage(image);
                }
            }
        }

        if (toolTipObject.activeInHierarchy)
        {
            toolTipObject.GetComponent<RectTransform>().position = Input.mousePosition; // offset
        }
    }

    private void OnClick()
    {
        if (slotImage.sprite != placeholderImage) //imageSet
        {
            imageSet = false;
            SetInteractableRemoveButton(false);

            if (pulsingUi != null)
                pulsingUi.SetPulsing(true);

            hoverObject.SetActive(false);
            toolTipObject.SetActive(false);

            UndoRedo.instance.Action();

            slotImage.sprite = placeholderImage;

            //Save Placeholder Image
            resetSaveImage();

        }
    }

    private void SaveImage(Sprite image)
    {
        /*
        // Get the project-relative path (e.g. "Assets/Textures/file.png")
        string assetPath = AssetDatabase.GetAssetPath(image);

        Debug.Log("Asset Path: " + assetPath);
        // Convert to absolute system path
        //string fullPath = Path.Combine(Application.dataPath, assetPath.Replace("Assets/", ""));
        string absolutePath = Path.GetFullPath(assetPath);

        Debug.Log("Full Path: " + absolutePath);

        slotImage.gameObject.GetComponent<CanvasOpenVideoAndImage>().TestPath(absolutePath);
        */

        slotImage.gameObject.GetComponent<CanvasOpenVideoAndImage>().SpriteToFilePath(image);
    }

    private void resetSaveImage()
    {
        slotImage.gameObject.GetComponent<CanvasOpenVideoAndImage>().RemoveImage();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (GetComponent<Button>().interactable)
        {
            hoverObject.SetActive(true);
            toolTipObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hoverObject.SetActive(false);
        toolTipObject.SetActive(false);
    }
}
