using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class MainFilesManager : MonoBehaviour
{
    public TMP_Text loadText;
    float totalToLoad;
    float loaded;
    bool loadDone;
    public Animator anim;



    public List<MainPageItem> mainPageItems;

    public List<string> recents = new List<string>();
    public GameObject itemPrefab;

    public MainManager mainManager;

    private void Awake()
    {
        totalToLoad = 0;
        mainManager = GetComponent<MainManager>();

        if (mainManager == null)
        {
            Debug.Log("No main manager component attached.");
        }

        string userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE");

        if (Directory.Exists(userPath))
        {
            string[] foldersCheck = Directory.GetDirectories(userPath);


            if (foldersCheck.Length == 0)
            {
                loadDone = true;
                StartCoroutine(DoneLoading());
            }

            totalToLoad = foldersCheck.Length;



            foreach (string checkPath in foldersCheck)
            {
                string fileName = checkPath.Substring(Path.GetDirectoryName(checkPath).Length + 1);
                string checkFile = Path.Combine(checkPath, fileName + ".ER");

                Debug.Log("Checking for " + checkFile);

                if (File.Exists(checkFile))
                {
                    MainPageItem newItem = Instantiate(mainManager.itemprefab, Vector3.zero, Quaternion.identity, mainManager.allHolder).GetComponent<MainPageItem>();
                    mainManager.existing.Add(newItem);
                    mainPageItems.Add(newItem);
                    newItem.projectName = fileName;

                    if (File.Exists(Path.Combine(checkPath, "Thumbnail.png")))
                    {
                        StartCoroutine(LoadSprite(fileName, newItem));
                    }
                    else
                    {
                        loaded++;
                    }

                    if (File.Exists(Path.Combine(checkPath, "ProjectDetails.txt")))
                    {
                        newItem.details = ES3.Load<string>("Details", Path.Combine(checkPath, "ProjectDetails.txt"));
                    }

                    newItem.transform.SetSiblingIndex(newItem.transform.parent.childCount - 2);


                    Debug.Log("Assigned details to item of " + newItem.projectName);
                }
                else
                {
                    loaded++;
                }


            }
        }
        else
        {
            Directory.CreateDirectory(userPath);
            Debug.Log("New iCUBE diretory " + userPath + " created");
            loadDone = true;
            StartCoroutine(DoneLoading());
        }

        //finding recents

        string path = Path.Combine(Application.persistentDataPath, "Recent.txt");
        if (!File.Exists(path))
        {
            Debug.Log("No recents.txt file found, creating");
            ES3.Save("Recent", recents, path);
        }

        recents = ES3.Load<List<string>>("Recent", path);

        List<string> removeRecent = new List<string>();
        for (int recentNum = 0; recentNum < recents.Count; recentNum++)
        {
            for (int itemNum = 0; itemNum < mainPageItems.Count; itemNum++)
            {

                MainPageItem mpi = mainPageItems[itemNum];

                if (mpi.projectName == recents[recentNum])
                {
                    GameObject item = Instantiate(mpi.gameObject, Vector3.zero, Quaternion.identity, mainManager.recentholder.transform);
                    item.transform.SetAsFirstSibling();
                    Debug.Log("There is " + recents[recentNum] + " MainPageItem found for recents");
                }
                else
                {
                    if (itemNum == mainManager.allHolder.childCount - 1)
                    {
                        removeRecent.Add(recents[recentNum]);
                        Debug.Log("No " + recents[recentNum] + " MainPageItem found");
                    }
                }
            }

        }

        foreach (string remove in removeRecent)
        {
            recents.Remove(remove);
        }

        ES3.Save("Recent", recents, path);



        Debug.Log("Calling Main Manager start call");
        FindObjectOfType<MainManager>().StartCall();



    }

    private void Update()
    {
        if (totalToLoad > 0)
        {
            if (loaded >= totalToLoad)
            {
                if (!loadDone)
                {
                    loadDone = true;
                    StartCoroutine(DoneLoading());
                }
            }
            else
            {
                loadText.text = "Loading... \n" + Mathf.RoundToInt((loaded/totalToLoad)*100) + "%";
            }
        }
    }

    IEnumerator DoneLoading()
    {
        yield return new WaitForSeconds(1);
        loadText.text = "Done Loading! \n 100%";
        yield return new WaitForSeconds(2);
        anim.SetBool("DoneLoading", true);
    }

    public IEnumerator LoadSprite(string fileName, MainPageItem item)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            yield break;
        }

        string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", fileName, "Thumbnail.png");
        
        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        item.thumbnailSprite = imageSprite;
        loaded++;
        yield return null;
    }

    public void CreateNewFile(string projectName)
    {
        LoadProject(projectName);
    }

    public void LoadProject(string projectName)
    {
        if (!recents.Contains(projectName))
        {
            if (recents.Count > 5)
            {
                recents.RemoveAt(0);
            }

        }
        else
        {
            recents.RemoveAt(recents.IndexOf(projectName));
            
        }

        recents.Add(projectName);

        string path = Path.Combine(Application.persistentDataPath, "Recent.txt");
        ES3.Save("Recent", recents, path);
        DataAcrossScenes.instance.projectName = projectName;

        SceneManager.LoadScene(1);
    }
}
