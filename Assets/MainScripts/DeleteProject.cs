using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using TMPro;

public class DeleteProject : MonoBehaviour
{
    public string projectName;
    public TMP_Text warning;
    public MainPageItem item;

    private void Start()
    {
        warning.text = "Are you sure you want to delete '" + projectName + "'?";
    }


    public void DestroyPopup()
    {
        Destroy(gameObject);
    }

    public  void DeleteDir()
    {

        string originalPathToDelete = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", projectName);

        foreach (string fileName in Directory.GetFiles(originalPathToDelete))
        {
            File.Delete(Path.Combine(originalPathToDelete, fileName));
        }

        foreach (string assetsname in Directory.GetDirectories(originalPathToDelete))
        {
            string assetsPath = Path.Combine(originalPathToDelete, assetsname);

            foreach (string dirname in Directory.GetDirectories(assetsPath))
            {
                string innerAssetsPath = Path.Combine(assetsPath, dirname);

                foreach (string assetFileName in Directory.GetFiles(innerAssetsPath))
                {
                    File.Delete(Path.Combine(innerAssetsPath, assetFileName));
                }

                Directory.Delete(innerAssetsPath);
            }

            Directory.Delete(assetsPath);
        }

        Directory.Delete(originalPathToDelete);


        //destroying mainpage item

        item.favouriteToggle.isOn = false;

        List<MainPageItem> removeThese = new List<MainPageItem>();

        foreach (MainPageItem itemCheck in FindObjectOfType<MainManager>().existing)
        {
            if (itemCheck.projectName == projectName)
            {
                removeThese.Add(itemCheck);
            }
        }

        for (int i = 0; i < removeThese.Count; i++)
        {
            removeThese[i].OffPopup();
            FindObjectOfType<MainManager>().existing.Remove(removeThese[i]);
            Destroy(removeThese[i].gameObject);
        }

        if (FindObjectOfType<MainFilesManager>().recents.Contains(projectName))
        {
            FindObjectOfType<MainFilesManager>().recents.Remove(projectName);
        }

        ES3.Save("Recent", FindObjectOfType<MainFilesManager>().recents, Path.Combine(Application.persistentDataPath, "Recent.txt"));

        DestroyPopup();


    }
}
