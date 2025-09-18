using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;
using System;
using System.Xml.Schema;

public class EditorManager : MonoBehaviour
{
    DataAcrossScenes dataPersistence;
    public GameObject threeWallChoosing;
    public GameObject fourWallChoosing;

    public GameObject threeWallDuplicate;
    public GameObject fourWallDuplicate;

    public AudioClip bgm;
    public string bgmFileName;
    public TMP_InputField titleText;

    public GameObject urlFailWarning;
    public GameObject namingFailWarning;

    public KeyCode undoKey;

    [Header("Addon")]
    public int totalTime;

    // Start is called before the first frame update
    void Start()
    {
        if (DataAcrossScenes.instance != null)
        {
            dataPersistence = DataAcrossScenes.instance;
        }
        else
        {
            dataPersistence = FindObjectOfType<DataAcrossScenes>();
        }

        if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            threeWallChoosing.SetActive(true);
            fourWallChoosing.SetActive(false);
            // Iterate through each child Transform
            foreach (Transform childTransform in threeWallChoosing.transform)
            {
                // Access the GameObject of the child Transform
                GameObject childGameObject = childTransform.gameObject;

                // Perform actions with the child GameObject
                childGameObject.SetActive(false);
            }
        }
        else if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Four)
        {
            threeWallChoosing.SetActive(false);
            fourWallChoosing.SetActive(true);
            // Iterate through each child Transform
            foreach (Transform childTransform in fourWallChoosing.transform)
            {
                // Access the GameObject of the child Transform
                GameObject childGameObject = childTransform.gameObject;

                // Perform actions with the child GameObject
                childGameObject.SetActive(false);
            }
        }

        titleText.text = DataAcrossScenes.instance.projectName;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                FindObjectOfType<QuizDataSaveLoad>().Save();
            }

            else if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(undoKey))
                {
                    UndoRedo.instance.Redo();
                }
            }


            else if (Input.GetKeyDown(undoKey))
            {
                UndoRedo.instance.Undo();
            }
        }
    }

    public void ChangeName()
    {
        string path = Path.Combine(Application.persistentDataPath, titleText.text);

        if (titleText.text == DataAcrossScenes.instance.projectName)
        {
            return;
        }

        if (Directory.Exists(path))
        {
            Debug.LogError(titleText.text + " file exists");
            namingFailWarning.SetActive(true);
            namingFailWarning.GetComponentInChildren<TMP_Text>().text = "Another project has the name '" + titleText.text + "'.";
            titleText.text = DataAcrossScenes.instance.projectName;
            return;
        }
        string originalName = DataAcrossScenes.instance.projectName;
        DataAcrossScenes.instance.projectName = titleText.text;




        Debug.Log("Original Name of project: " + originalName);

        string moveDirFrom = Path.Combine(Application.persistentDataPath, originalName);
        string moveDirTo = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName);

        if (!Directory.Exists(moveDirFrom))
        {
            Debug.LogError(moveDirFrom + " does not exist");
        }

        Directory.Move(moveDirFrom, moveDirTo);

        string moveFileFrom = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, originalName + ".quiz");
        string moveFileTo = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, DataAcrossScenes.instance.projectName + ".quiz");

        if (!File.Exists(moveFileFrom))
        {
            Debug.LogError(moveFileFrom + " does not exist");
        }
        else
        {
            File.Move(moveFileFrom, moveFileTo);
        }

        string recentFile = Path.Combine(Application.persistentDataPath, "Recent.txt");
        List<string> recent = ES3.Load<List<string>>("Recent", recentFile);
        int i = recent.IndexOf(originalName);
        recent[i] = DataAcrossScenes.instance.projectName;
        ES3.Save("Recent", recent, recentFile);

        Debug.Log(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE"));
        if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE")))
        {
            return;
        }

        Debug.Log("Original Name of project: " + originalName);
        string moveUserDirFrom = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", originalName);
        string moveUserDirTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName);
        Directory.Move(moveUserDirFrom, moveUserDirTo);

        string moveUserFileFrom = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName, originalName + ".quiz");
        string moveUserFileTo = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "iCUBE", DataAcrossScenes.instance.projectName, DataAcrossScenes.instance.projectName + ".quiz");

        if (!File.Exists(moveUserFileFrom))
        {
            Debug.LogError(moveUserFileFrom + " does not exist");
        }
        else
        {
            File.Move(moveUserFileFrom, moveUserFileTo);
        }
    }

    public void OpenDuplicate(SlideManager man)
    {
        if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            threeWallDuplicate.SetActive(true);
            fourWallDuplicate.SetActive(false);

            threeWallDuplicate.GetComponent<DuplicateWall>().CopyFrom(man);
        }
        else if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Four)
        {
            threeWallDuplicate.SetActive(false);
            fourWallDuplicate.SetActive(true);

            fourWallDuplicate.GetComponent<DuplicateWall>().CopyFrom(man);
        }
    }

    public void PlayBGM()
    {
        GetComponent<AudioSource>().PlayOneShot(bgm);
    }

    public void LoadAudioClip()
    {

        if (String.IsNullOrEmpty(bgmFileName))
        {
            FindObjectOfType<WallManager>().AudioCheck();
            return;
        }

        string path = Path.Combine("file:///", Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Audio", bgmFileName);
        FindObjectOfType<MediaLoader>().StartCoroutine(FindObjectOfType<MediaLoader>().LoadAudio(path));

        FindObjectOfType<WallManager>().AudioCheck();
    }

    public void AssignBGM(AudioClip bgmClip, string clipName)
    {
        Debug.Log("Assigned bgm");
        bgm = bgmClip;
        bgmFileName = clipName;
        FindObjectOfType<WallManager>().AudioCheck();
    }

    public void RemoveAudio()
    {
        bgm = null;
        bgmFileName = null;
        FindObjectOfType<WallManager>().AudioCheck();
    }

    public void UpdateTimer(int time)
    {

    }
}
