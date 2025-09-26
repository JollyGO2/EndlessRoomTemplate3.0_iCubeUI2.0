using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class QuizDataSaveLoad : MonoBehaviour
{
    DataAcrossScenes dataAcrossScenes;
    [SerializeField] string originalName;


    public List<Sprite> placeholderImages = new List<Sprite>();

    void Start()
    {
        StartCoroutine(WaitForInstances());
    }

    IEnumerator WaitForInstances()
    {
        yield return DataAcrossScenes.instance;
        Debug.Log("Got datapersistence instance");
        yield return SlidesData.instance;
        Loading();

        
    }

    public void Loading()
    {
        originalName = DataAcrossScenes.instance.projectName;
        string userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName);

        string dataDir  = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName);

        if (!Directory.Exists(dataDir))
        {
            Directory.CreateDirectory(dataDir);
            Directory.CreateDirectory(Path.Combine(dataDir, "Assets"));
            Directory.CreateDirectory(Path.Combine(dataDir, "Assets", "Image"));
            Directory.CreateDirectory(Path.Combine(dataDir, "Assets", "Video"));
            Directory.CreateDirectory(Path.Combine(dataDir, "Assets", "Audio"));
        }

        string saveFile = Path.Combine(userDir, DataAcrossScenes.instance.projectName + ".ER"); //Originally .quiz
        Debug.Log("Save file path: " + saveFile);

        if (!File.Exists(saveFile))
        {


            foreach (SlideManager slideManager in SlidesData.instance.managers)
            {
                SlidesData.instance.DataUpdate(slideManager);
            }


            for (int i = 0; i < SlidesData.instance.slideManagersData.Count; i++)
            {
                for (int a = 0; a < placeholderImages.Count; a++) // a < 4
                {
                    if(placeholderImages[a] == null)
                    {
                        continue;
                    }

                    StartCoroutine(SavePlaceholderImage(placeholderImages[a]));
                    SlidesData.instance.slideManagersData[i].slidesData[1].mediaAnswersPath[a] = placeholderImages[a].name + ".png";
                }
            } //Placeholders images



            SlidesData.instance.LoadData();
            Debug.Log("No save file for Project" + DataAcrossScenes.instance.projectDetails + " found.");

        }
        else
        {
            SlidesData.instance.toLoad = true;
            //slidesData.slidesDataList = ES3.Load<List<SlidesData.Slides>>("SlidesData", saveFile);
            for (int managerNum = 0; managerNum < SlidesData.instance.slideManagersData.Count; managerNum++)
            {
                SlidesData.instance.slideManagersData[managerNum].slidesData = ES3.Load<List<SlidesData.Slides>>("Wall" + managerNum.ToString(), saveFile);
            }

            Debug.Log(ES3.Load<string>("BGM", saveFile));
            FindObjectOfType<EditorManager>().bgmFileName = ES3.Load<string>("BGM", saveFile);

            CopyFiles(userDir, dataDir);

            SlidesData.instance.LoadData();

            FindObjectOfType<EditorManager>().LoadAudioClip();
        }


        FindObjectOfType<EditorManager>().titleText.text = DataAcrossScenes.instance.projectName;

    }

    IEnumerator SavePlaceholderImage(Sprite sprite)
    {
            Sprite itemBGSprite = sprite;
            Texture2D itemBGTex = itemBGSprite.texture;
            byte[] itemBGBytes = itemBGTex.EncodeToPNG();

            string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Image", sprite.name + ".png");
            Debug.Log(path);

            File.WriteAllBytes(path, itemBGBytes);

        yield return File.Exists(path);
        

    }

    public void Save()
    {

        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE")))
        {
            Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE"));
            Debug.Log("User iCUBE file does not exist, creating...");
        }

        string userDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName);
        string saveDir = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName);
        Debug.Log(saveDir);

        if (!Directory.Exists(saveDir))
        {
                Directory.CreateDirectory(saveDir);
                Directory.CreateDirectory(Path.Combine(saveDir, "Assets"));
                Directory.CreateDirectory(Path.Combine(saveDir, "Assets", "Image"));
                Directory.CreateDirectory(Path.Combine(saveDir, "Assets", "Video"));
                Directory.CreateDirectory(Path.Combine(saveDir, "Assets", "Audio"));
            
        }


        if (!Directory.Exists(userDir))
        {
            
                Directory.CreateDirectory(userDir);
                Directory.CreateDirectory(Path.Combine(userDir, "Assets"));
                Directory.CreateDirectory(Path.Combine(userDir, "Assets", "Image"));
                Directory.CreateDirectory(Path.Combine(userDir, "Assets", "Video"));
                Directory.CreateDirectory(Path.Combine(userDir, "Assets", "Audio"));
            
        }

        //ES3.Save("SlidesData", slidesData.slidesDataList, Path.Combine(saveDir, DataAcrossScenes.instance.projectName + ".ER"));

        for (int managerNum = 0; managerNum < SlidesData.instance.slideManagersData.Count; managerNum++)
        {
            ES3.Save("Wall" + managerNum.ToString(), SlidesData.instance.slideManagersData[managerNum].slidesData, Path.Combine(saveDir, DataAcrossScenes.instance.projectName + ".ER"));
            Debug.Log("Saved wall " + managerNum);
        }


        if (DataAcrossScenes.instance.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            ES3.Save("WallConfig", "3", Path.Combine(saveDir, DataAcrossScenes.instance.projectName + ".ER"));
        }
        else if (DataAcrossScenes.instance.wallsConfig == DataAcrossScenes.WallsConfig.Four)
        {
            ES3.Save("WallConfig", "4", Path.Combine(saveDir, DataAcrossScenes.instance.projectName + ".ER"));
        }


        ES3.Save("BGM", FindObjectOfType<EditorManager>().bgmFileName, Path.Combine(saveDir, DataAcrossScenes.instance.projectName + ".ER"));

        CopyFiles(saveDir, userDir);

        //string thumbnailPath = Path.Combine(saveDir, "Thumbnail.png");

    }

    public void CopyFiles(string fromDir, string toDir)
    {


        string copyQuizFrom = Path.Combine(fromDir, DataAcrossScenes.instance.projectName + ".ER");
        string copyQuizTo = Path.Combine(toDir, DataAcrossScenes.instance.projectName + ".ER");

        if (File.Exists(copyQuizTo))
        {
            File.Delete(copyQuizTo);
        }

        File.Copy(copyQuizFrom, copyQuizTo);




        //Image saving
        string imagePath = Path.Combine(fromDir, "Assets", "Image");

        List<string> imageNames = new List<string>();

        foreach (string imagePaths in Directory.GetFiles(imagePath))
        {
            string name = imagePaths.Substring(imagePath.Length + 1);
            //Debug.Log(name);

            imageNames.Add(name);
        }

        foreach (string imageFile in imageNames)
        {
            string copyFrom = Path.Combine(fromDir, "Assets", "Image", imageFile);
            string copyTo = Path.Combine(toDir, "Assets", "Image", imageFile);

            if (!File.Exists(copyTo))
            {
                File.Copy(copyFrom, copyTo);
            }
        }

        //Video saving
        string videoPath = Path.Combine(fromDir, "Assets", "Video");

        List<string> videoNames = new List<string>();

        foreach (string videoPaths in Directory.GetFiles(videoPath))
        {
            string name = videoPaths.Substring(videoPath.Length + 1);
            //Debug.Log(name);

            videoNames.Add(name);
        }

        foreach (string videoFile in videoNames)
        {
            string copyFrom = Path.Combine(fromDir, "Assets", "Video", videoFile);
            string copyTo = Path.Combine(toDir, "Assets", "Video", videoFile);

            if (!File.Exists(copyTo))
            {
                File.Copy(copyFrom, copyTo);
            }
        }

        //Audio saving
        string audioPath = Path.Combine(fromDir, "Assets", "Audio");

        List<string> audioNames = new List<string>();

        foreach (string audioPaths in Directory.GetFiles(audioPath))
        {
            string name = audioPaths.Substring(audioPath.Length + 1);
            //Debug.Log(name);

            audioNames.Add(name);
        }

        foreach (string audioFile in audioNames)
        {
            string copyFrom = Path.Combine(fromDir, "Assets", "Audio", audioFile);
            string copyTo = Path.Combine(toDir, "Assets", "Audio", audioFile);

            if (!File.Exists(copyTo))
            {
                File.Copy(copyFrom, copyTo);
            }
        }
    }

    public void SaveThumbnail()
    {

        StartCoroutine(FindObjectOfType<ThumbnailMaking>().RecordFrame());
        StartCoroutine(WaitForThumbnail());

    }

    IEnumerator WaitForThumbnail()
    {
        yield return FindObjectOfType<ThumbnailMaking>().outputSprite;

        Sprite thumbnail = FindObjectOfType<ThumbnailMaking>().outputSprite;
        Texture2D spriteTex = thumbnail.texture;
        byte[] spriteTexBytes = spriteTex.EncodeToPNG();

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Thumbnail.png");
        Debug.Log("Thumbnail created at " + path);

        File.WriteAllBytes(path, spriteTexBytes);

        string copyFrom = Path.Combine(path);
        string copyTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName, "Thumbnail.png");

        if (File.Exists(copyTo))
        {
            File.Delete(copyTo);
        }

        File.Copy(copyFrom, copyTo);
    }

    private void OnApplicationQuit()
    {

        //string dataPath = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName);

        // StartCoroutine(DeleteDir(dataPath));

        DeleteDir();

    }

    public void DeleteDir()
    {
        string originalPathToDelete = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName);
        foreach (string fileName in Directory.GetFiles(originalPathToDelete))
        {
            File.Delete(Path.Combine(originalPathToDelete, fileName));
        }

        foreach(string assetsname in Directory.GetDirectories(originalPathToDelete))
        {
            string assetsPath = Path.Combine(originalPathToDelete, assetsname);

            foreach(string dirname in Directory.GetDirectories(assetsPath))
            {
                string innerAssetsPath = Path.Combine(assetsPath, dirname);

                foreach(string assetFileName in Directory.GetFiles(innerAssetsPath))
                {
                    File.Delete(Path.Combine(innerAssetsPath, assetFileName));
                }

                Directory.Delete(innerAssetsPath);
            }

            Directory.Delete(assetsPath);
        }

        Directory.Delete(originalPathToDelete);

    }
}
