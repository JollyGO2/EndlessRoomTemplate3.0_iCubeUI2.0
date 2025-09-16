using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Text;

public class MainManager : MonoBehaviour
{
    public List<MainPageItem> existing;
    public List<GameObject> favourited;
    public Transform favholder;

    public TMP_Text favouriteTitle;
    public Image favouriteButton;
    public GameObject favouriteScreen;

    public TMP_Text catalogueTitle;
    public Image catalogueButton;
    public GameObject catalogueScreen;

    public Color green;
    public Color white;

    public int current;

    public GameObject newfromtem;

    public Transform allHolder;
    public GameObject recentholder;
    public GameObject itemprefab;

    public MainFilesManager fileManagement;
    public TMP_Text namingWarning;

    public void StartCall()
    {


        ClickCata();
        foreach (Scrollbar b in FindObjectsOfType<Scrollbar>())
        {
            if (b.name.Contains("Vertical"))
            {
                b.value = 1;
                Debug.Log("Set scrollbar vertical");
            }
            else
            {
                b.value = 0;
            }
        }
        if (DataAcrossScenes.instance != null)
        {
            if (DataAcrossScenes.instance.projectDetails != null)
            {
                DataAcrossScenes.instance.projectThumbnail = null;
                DataAcrossScenes.instance.projectName = null;
                DataAcrossScenes.instance.projectDetails = null;
            }
        }

        foreach (MainPageItem p in FindObjectsOfType<MainPageItem>())
        {
            if (!existing.Contains(p))
            {
                existing.Add(p);
            }
        }

        foreach (MainPageItem p in existing)
        {
            p.ID = existing.IndexOf(p);
            //p.GetComponent<Button>().onClick.AddListener(()=>newfromtem.SetActive(true));

            if (recentholder.transform.childCount == 0)
            {
                recentholder.transform.parent.parent.parent.gameObject.SetActive(false);
            }


            Debug.Log("StartCalling on item " + existing.IndexOf(p) + " " + p.projectName);
            p.StartCall();
        }
    }


    public void Favourite(Toggle t, MainPageItem item)
    {
        if (item.ignoreFavourite)
        {
            item.ignoreFavourite = false;
            return;
        }

        if (t.isOn)
        {
            GameObject o = Instantiate(t.transform.parent.parent.gameObject, favholder); //duplicating favourite item
            favourited.Add(o);
            o.GetComponent<MainPageItem>().favourited = true;

            foreach (GameObject g in o.GetComponent<MainPageItem>().hoveringObjects)
            {
                g.SetActive(false);
            }

            string favPath = Path.Combine(Application.persistentDataPath, "Favourites.txt");

            if (!File.Exists(favPath))
            {
                ES3.Save("Favourites", new List<string>(), favPath);
            }

            List<string> favourites = ES3.Load<List<string>>("Favourites", favPath);

            if (!favourites.Contains(item.projectName))
            {
                favourites.Add(item.projectName);
                ES3.Save("Favourites", favourites, favPath);
            }

            Debug.Log("Favourited " + item.projectName);

            foreach (MainPageItem p in existing)
            {
                if (p.projectName == item.projectName)
                {
                    p.favourited = true;
                    p.ignoreFavourite = true;
                    p.favouriteToggle.isOn = true;
                }
            }

        }
        else
        {


            string s = t.transform.parent.parent.GetComponent<MainPageItem>().projectName;

            foreach (MainPageItem p in existing)
            {
                if (p.projectName == s)
                {
                    p.favourited = false;
                    p.favouriteToggle.isOn = false;
                }
            }

            for (int i = 0; i<favourited.Count; i++)
            {
                GameObject o = favourited[i];

                if (o.GetComponent<MainPageItem>().projectName == s)
                {
                    o.GetComponent<MainPageItem>().OffPopup();
                    favourited.Remove(o);
                    Destroy(o);
                    return;
                }
            }

            string favPath = Path.Combine(Application.persistentDataPath, "Favourites.txt");
            List<string> favourites = ES3.Load<List<string>>("Favourites", favPath);

            if (favourites.Contains(item.projectName))
            {
                favourites.Remove(item.projectName);
                ES3.Save("Favourites", favourites, favPath);
            }

            Debug.Log("Unfavourited " + item.projectName);

        }
    }

    public void NewFromTemplate(TMP_InputField f)
    {
        if (!String.IsNullOrEmpty(f.text))
        {
            if (Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", f.text)))
            {
                namingWarning.gameObject.SetActive(true);
                namingWarning.text = "Another project has the name '" + f.text + "'.";
                return;
            }

            fileManagement.CreateNewFile(f.text);
        }
        else
        {
            namingWarning.gameObject.SetActive(true);
            namingWarning.text = "The project needs a name.";
        }

    }

    public void Hover(GameObject i)
    {
        i.GetComponent<Image>().color = white;
        i.GetComponentInChildren<TMP_Text>().color = green;
    }

    public void OffHover(GameObject i)
    {
        if (i == favouriteButton.gameObject && current == 1)
        {
            return;
        }

        if (i == catalogueButton.gameObject && current == 0)
        {
            return;
        }

        i.GetComponent<Image>().color = green;
        i.GetComponentInChildren<TMP_Text>().color = white;

    }

    public void ClickFav()
    {
        favouriteScreen.SetActive(true);
        favouriteTitle.color = green;
        favouriteButton.color = white;

        catalogueScreen.SetActive(false);
        catalogueTitle.color = white;
        catalogueButton.color = green;

        current = 1;
    }

    public void ClickCata()
    {
        catalogueScreen.SetActive(true);
        catalogueTitle.color = green;
        catalogueButton.color = white;

        favouriteScreen.SetActive(false);
        favouriteTitle.color = white;
        favouriteButton.color = green;

        current = 0;
    }

    private void Update()
    {
        if (recentholder.transform.childCount == 0)
        {
            recentholder.transform.parent.parent.parent.gameObject.SetActive(false);
        }
    }

}
