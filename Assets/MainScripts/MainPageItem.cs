using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.IO;

public class MainPageItem : MonoBehaviour
{
    public GameObject popupPrefab;
    public GameObject canvasParent;
    public MainPopup popup;
    public float hoverTime;
    public bool hovering;

    public Sprite thumbnailSprite;
    public string projectName;
    public string details;
    public bool isTemplate;


    public TMP_Text titleText;


    public int ID;
    public bool favourited;
    public Toggle favouriteToggle;
    public bool templateIconRemoved;

    public List<GameObject> hoveringObjects;
    bool toggleAssignedFunction;

    public bool ignoreFavourite;

    public GameObject deletePopup;

    public void Awake()
    {
        if (!toggleAssignedFunction)
        {
            favouriteToggle.onValueChanged.AddListener(delegate { FindObjectOfType<MainManager>().Favourite(favouriteToggle, this); });
            toggleAssignedFunction = true;
        }

        canvasParent = GameObject.Find("Canvas");
        titleText.text = projectName;
        gameObject.GetComponent<Image>().sprite = thumbnailSprite;
    }


    public void StartCall()
    {
        if (!toggleAssignedFunction)
        {
            favouriteToggle.onValueChanged.AddListener(delegate { FindObjectOfType<MainManager>().Favourite(favouriteToggle, this); });
            toggleAssignedFunction = true;
        }

        string favPath = Path.Combine(Application.persistentDataPath, "Favourites.txt");
        if (File.Exists(favPath))
        {
            List<string> favourites = ES3.Load<List<string>>("Favourites", favPath);
            if (favourites.Contains(projectName))
            {
                favouriteToggle.isOn = true;    
                Debug.Log("Favourite Toggle on " + projectName);

            }
        }

        if (!isTemplate)
        {
            if (!templateIconRemoved)
            {
                GetComponent<Button>().onClick.AddListener(delegate { FindObjectOfType<MainFilesManager>().LoadProject(projectName); });
                Debug.Log("Added LoadFunction to " + projectName);

                Destroy(hoveringObjects[2]);
                hoveringObjects.Remove(hoveringObjects[2]);
                templateIconRemoved = true;
            }
        }
        else
        {
            if (hoveringObjects[3])
            {
                Destroy(hoveringObjects[3]);
                hoveringObjects.Remove(hoveringObjects[3]);
            }
        }

        titleText.text = projectName;
        gameObject.GetComponent<Image>().sprite = thumbnailSprite;




    }

    public void Delete()
    {
        DeleteProject del = Instantiate(deletePopup, canvasParent.transform).GetComponent<DeleteProject>();
        del.projectName = projectName;
        del.item = this;
        Debug.Log("delete clicked");
    }

    public void Hovering()
    {
        hoverTime = 0;
        hovering = true;
        foreach(GameObject o in hoveringObjects)
        {
            o.SetActive(true);
        }
    }

    private void Update()
    {
        if (hovering)
        {
            hoverTime += Time.deltaTime;

            if (hoverTime >= 0.75f)
            {
                hovering = false;
                HoverPopup(this.gameObject);
            }
        }
    }

    public void HoverPopup(GameObject a)
    {
        if (popup != null)
        {
            return;
        }

        if (FindObjectOfType<MainPopup>())
        {
            foreach (MainPopup m in FindObjectsOfType<MainPopup>())
            {
                m.Exit();
            }
        }

        Vector3 i = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        float spawnhere = 0;

        if (i.x > 0.5f)
        {
            spawnhere = -400;
        }
        else
        {
            spawnhere = 400;
        }



        popup = Instantiate(popupPrefab, new Vector3(a.GetComponent<RectTransform>().position.x, a.GetComponent<RectTransform>().position.y, 0), Quaternion.identity, canvasParent.transform).GetComponent<MainPopup>();
        popup.GetComponent<RectTransform>().anchoredPosition += new Vector2(spawnhere, 0);
        

        if (i.y < 0.45f)
        {
            popup.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition += new Vector2(0, 100);
        }


        popup.hov = this;

        if (i.x > 0.5f)
        {
            popup.leftcheck.SetActive(false);
            popup.leftar.SetActive(false);
        }
        else
        {
            popup.rightcheck.SetActive(false);
            popup.rightar.SetActive(false);
        }


            popup.image.sprite = thumbnailSprite;
            popup.details.text = details;
            popup.title.text = projectName;
           

    }

    public void ResetTime()
    {

        hovering = false;
        hoverTime = 0;


        foreach (GameObject o in hoveringObjects)
        {
            o.SetActive(false);
        }
    }
    public void OffPopup()
    {
        ResetTime();

        if (popup)
        {

            Destroy(popup.gameObject);
            popup = null;
        }

    }
}
