using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragUI : MonoBehaviour, IDragHandler
{

    public Canvas parentCanvasOfImageToMove;
    public Image imageToMove;

    public bool moving;
    public int originalIndex;


    public Transform lastTouch;

    [Header("Addons")]
    public bool isTool;
    private bool cloneSpawn = false;
    public GameObject clonePrefab;
    private GameObject cloneRef;
    [SerializeField] PulsingUI pulsingUI;

    private void Start()
    {
        GetComponent<BoxCollider>().isTrigger = false;
        imageToMove = GetComponent<Image>();

        Transform check = this.transform;

        while (parentCanvasOfImageToMove == null)
        {
            if (!check.parent.GetComponent<Canvas>())
            {
                check = check.parent;
            }
            else
            {
                parentCanvasOfImageToMove = check.parent.GetComponent<Canvas>();
            }

        }

    }

    public void OnMouseDown()
    {
        FindObjectOfType<SlideManager>().currentSlide = transform.GetSiblingIndex();
        FindObjectOfType<SlideManager>().ReloadSlide();
        if(pulsingUI != null)
            pulsingUI.SetPulsing(false);
    }

    public void OnDrag(PointerEventData data)
    {
        if (!isTool)
        {
            if (FindObjectOfType<SlideManager>().currentSlide != transform.GetSiblingIndex())
            {
                FindObjectOfType<SlideManager>().currentSlide = transform.GetSiblingIndex();
                FindObjectOfType<SlideManager>().ReloadSlide();
            }

            originalIndex = transform.GetSiblingIndex();
            moving = true;
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasOfImageToMove.transform as RectTransform, data.position, parentCanvasOfImageToMove.worldCamera, out pos);
            Vector3 movePos = parentCanvasOfImageToMove.transform.TransformPoint(pos);
            imageToMove.transform.position = new Vector3(imageToMove.transform.position.x, movePos.y, imageToMove.transform.position.z);
            GetComponent<BoxCollider>().isTrigger = true;
        }
        else
        {

            originalIndex = transform.GetSiblingIndex();
            moving = true;
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvasOfImageToMove.transform as RectTransform, data.position, parentCanvasOfImageToMove.worldCamera, out pos);
            Vector3 movePos = parentCanvasOfImageToMove.transform.TransformPoint(pos);
            imageToMove.transform.position = new Vector3(movePos.x, movePos.y, imageToMove.transform.position.z);

            if (!cloneSpawn)
            {
                cloneRef = Instantiate(clonePrefab, this.transform.root);

                //Set Image
                cloneRef.GetComponent<Image>().sprite = transform.Find("ToolMainButton").GetComponent<Image>().sprite; ;
                cloneSpawn = true;
                Debug.Log(cloneRef.GetComponent<Image>().sprite.name);
            }
            cloneRef.transform.position = new Vector3(movePos.x, movePos.y, imageToMove.transform.position.z);
        }

    }

    //public void OnMouseEnter()
    //{
    //    Debug.Log("Mouse over");
    //    foreach(DragUI drag in FindObjectsOfType<DragUI>())
    //    {
    //        if (drag.moving)
    //        {
    //            if (drag!= this)
    //            {
    //                Debug.Log("Triggered assign");
    //                drag.lastTouch = this.transform;
    //            }
    //        }
    //    }
    //}
    public void OnTriggerEnter(Collider collision)
    {
        //Debug.Log("Touched");
        if (!moving)
        {
            return;
        }
        //Debug.Log("Triggered");

        lastTouch = collision.transform;


    }

    private void Update()
    {
        if (moving)
        {
            if (Input.GetMouseButtonUp(0))
            {
                MouseUp();
            }
        }
    }
    public void MouseUp()
    {
        GetComponent<BoxCollider>().isTrigger = true;
        moving = false;
        Debug.Log("Released mouse");

        if (pulsingUI != null)
            pulsingUI.SetPulsing(true);

        if (!isTool)
        {
            if (!lastTouch)
            {
                gameObject.SetActive(false);
                gameObject.SetActive(true);
                transform.SetSiblingIndex(originalIndex);

                return;
            }

            int oldIndex = originalIndex;
            int newIndex = 0;
            int here = 0;

            if (originalIndex > lastTouch.GetSiblingIndex())
            {
                if (lastTouch.position.y > transform.position.y)
                {

                    here = lastTouch.GetSiblingIndex() + 1;
                    Debug.Log("move down to " + here);


                }
                else
                {
                    here = lastTouch.GetSiblingIndex();
                    Debug.Log("move up to " + here);


                }
            }
            else
            {
                if (lastTouch.position.y > transform.position.y)
                {

                    here = lastTouch.GetSiblingIndex();
                    Debug.Log("move down to " + here);

                }
                else
                {
                    here = lastTouch.GetSiblingIndex() + 1;
                    Debug.Log("move up to " + here);

                }
            }


            if (here >= transform.parent.childCount)
            {
                transform.SetAsLastSibling();
                Debug.Log("Set as last");
                newIndex = transform.parent.childCount - 1;
            }
            else if (here <= 0)
            {
                transform.SetAsFirstSibling();
                Debug.Log("Set as first");
            }
            else
            {
                transform.SetSiblingIndex(here);
                newIndex = here;
            }

            gameObject.SetActive(false);
            gameObject.SetActive(true);

            lastTouch = null;

            FindObjectOfType<SlideManager>().MovedSlides(oldIndex, newIndex);

        }
        else
        {
            gameObject.SetActive(false);
            gameObject.SetActive(true);
            transform.SetSiblingIndex(originalIndex);
        }

        if (cloneSpawn)
        {
            Destroy(cloneRef);
            cloneSpawn = false;
        }
    }

}
