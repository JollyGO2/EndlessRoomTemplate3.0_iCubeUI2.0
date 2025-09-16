using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using System.IO;
using System.Web;
using System;
using TMPro;

public class PlayerLoadData : MonoBehaviour
{
    float totalToLoad;
    float loaded;
    public TMP_Text loading;
    bool loadDone;

    [SerializeField] List<QuizManager> quizManagers = new List<QuizManager>();
    public class SlidesRawData
    {
        public List<string> textAnswers = new List<string>() { "", "" };
        public List<string> mediaAnswersPath = new List<string>() { null, null };
        public List<bool> correctAnswers = new List<bool>() { true, false };
        public string questionText;
        public enum Type { text, image, both, blank };
        public Type slideType;
        public int choiceCount = 2;

        public string bgSpritePath;
    }

    [System.Serializable]
    public class SlidesManage
    {
        public List<SlidesRawData> slidesData = new List<SlidesRawData> { };
    }

    public List<SlidesManage> slideManagersData = new List<SlidesManage> { };
    public AudioSource bgm;

    public string folder;

    public void OnPointerDown(PointerEventData eventData) { }

    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.O) && Input.GetKeyDown(KeyCode.P))
        {
            OnClick();
        }

        if (loading.gameObject.activeInHierarchy)
        {
            if (!loadDone)
            {
                if (loaded >= totalToLoad)
                {
                    Debug.Log("Player Loading Done");
                    loadDone = true;
                    StartCoroutine(DoneLoading());
                }
                else
                {
                    loading.text = "Loading... \n" + Mathf.RoundToInt((loaded / totalToLoad) * 100) + "%";
                    //loading.text = "Loading... " + loaded + "/" + totalToLoad;
                }
            }
        }
    }

    IEnumerator DoneLoading()
    {
        yield return new WaitForSeconds(1);
        loading.text = "Done Loading! \n 100%";
        yield return new WaitForSeconds(2);
        loading.transform.parent.gameObject.SetActive(false);
    }

    private void OnClick()
    {
        var paths = StandaloneFileBrowser.OpenFolderPanel("Open iCUBE Quiz Folder", "", false);

        
        if (paths.Length > 0)
        {


            loading.transform.parent.gameObject.SetActive(true);
            string checkPath = HttpUtility.UrlDecode(new System.Uri(paths[0]).AbsoluteUri);


            Debug.Log(checkPath);

            Load(checkPath);
        }
    }

    

    void Load(string folderPath)
    {
        loadDone = false;
        quizManagers.Clear();


        switch (PlayerConfigurationManager.instance.playerConfig)
        {
            case PlayerConfigurationManager.PlayerConfig._2w0f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                break;

            case PlayerConfigurationManager.PlayerConfig._3w0f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleLeftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.rightWallCanvas.GetComponentInChildren<QuizManager>());
                break;

            case PlayerConfigurationManager.PlayerConfig._3w1f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleLeftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.rightWallCanvas.GetComponentInChildren<QuizManager>());
                break;

            case PlayerConfigurationManager.PlayerConfig._4w0f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleLeftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleRightWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.rightWallCanvas.GetComponentInChildren<QuizManager>());
                break;

            case PlayerConfigurationManager.PlayerConfig._4w1f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleLeftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleRightWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.rightWallCanvas.GetComponentInChildren<QuizManager>());
                break;

            case PlayerConfigurationManager.PlayerConfig._4w2f:
                quizManagers.Add(PlayerConfigurationManager.instance.leftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleLeftWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.middleRightWallCanvas.GetComponentInChildren<QuizManager>());
                quizManagers.Add(PlayerConfigurationManager.instance.rightWallCanvas.GetComponentInChildren<QuizManager>());
                break;

        }




        string fileName = folderPath.Substring(Path.GetDirectoryName(folderPath).Length + 3);
        string filePath = (Path.Combine(folderPath, fileName + ".quiz")).Substring(8);

        folder = folderPath.Substring(8);

        Debug.Log(filePath);

        string wallConfigString = ES3.Load<string>("WallConfig", filePath);
        Debug.Log("Number of walls = " + wallConfigString);
        int wallConfig = Int32.Parse(wallConfigString);


        //Loading screen checking
        totalToLoad = 1;
        loaded = 0;
        for (int wallLoad = 0; wallLoad < wallConfig; wallLoad++)
        {
            List<SlidesData.Slides> slidesDataList = ES3.Load<List<SlidesData.Slides>>("Wall" + wallLoad.ToString(), filePath);
            totalToLoad += slidesDataList.Count; //background images

            QuizManager quizManager = quizManagers[wallLoad];

            StartCoroutine(LoadWall(quizManager, slidesDataList));

            for (int slideNum = 0; slideNum < slidesDataList.Count; slideNum++)
            {
                SlidesData.Slides currentDataSlide = slidesDataList[slideNum];


                totalToLoad += currentDataSlide.mediaAnswersPath.Count; //media answer/blank slide images
            }
        }

        

        string bgmPath = ES3.Load<string>("BGM", filePath);
        StartCoroutine(LoadAudio(bgmPath));

    }

    IEnumerator LoadWall(QuizManager quizManager, List<SlidesData.Slides> slidesDataList)
    {

        while (quizManager.slidesList.Count < slidesDataList.Count)
        {
            quizManager.slidesList.Add(new QuizManager.Slide());

        }

        for (int slideNum = 0; slideNum < slidesDataList.Count; slideNum++)
        {

            QuizManager.Slide currentQuizSlide = quizManager.slidesList[slideNum];
            SlidesData.Slides currentDataSlide = slidesDataList[slideNum];

            currentQuizSlide.textAnswers = new List<string>(currentDataSlide.textAnswers);
            currentQuizSlide.correctAnswers = new List<bool>(currentDataSlide.correctAnswers);
            currentQuizSlide.questionText = currentDataSlide.questionText;
            currentQuizSlide.numberOfAnswers = currentDataSlide.choiceCount;
            currentQuizSlide.videoAnswersPath = new List<string>();
            currentQuizSlide.imageSprites = new List<Sprite>();



            if (currentDataSlide.slideType == SlidesData.Slides.Type.text)
            {
                currentQuizSlide.slideType = QuizManager.Slide.SlideType.text;
                yield return null;
            }
            else if (currentDataSlide.slideType == SlidesData.Slides.Type.image)
            {
                currentQuizSlide.slideType = QuizManager.Slide.SlideType.image;
                yield return null;
            }
            else if (currentDataSlide.slideType == SlidesData.Slides.Type.both)
            {
                currentQuizSlide.slideType = QuizManager.Slide.SlideType.both;
                yield return null;
            }
            else
            {
                currentQuizSlide.slideType = QuizManager.Slide.SlideType.blank;
                yield return null;
            }

            for (int mediaNum = 0; mediaNum < currentDataSlide.mediaAnswersPath.Count; mediaNum++)
            {
                string mediaPath = currentDataSlide.mediaAnswersPath[mediaNum];

                while (currentQuizSlide.videoAnswersPath.Count < currentDataSlide.mediaAnswersPath.Count)
                {
                    currentQuizSlide.videoAnswersPath.Add(null);
                }
                while (currentQuizSlide.videoAnswersPath.Count > currentDataSlide.mediaAnswersPath.Count)
                {
                    currentQuizSlide.videoAnswersPath.RemoveAt(currentQuizSlide.videoAnswersPath.Count - 1);
                }

                while (currentQuizSlide.imageSprites.Count < currentDataSlide.mediaAnswersPath.Count)
                {
                    currentQuizSlide.imageSprites.Add(null);
                }
                while (currentQuizSlide.imageSprites.Count < currentDataSlide.mediaAnswersPath.Count)
                {
                    currentQuizSlide.imageSprites.RemoveAt(currentQuizSlide.imageSprites.Count - 1);
                }


                if (Path.GetExtension(mediaPath) == ".mp4")
                {
                    loaded++;
                    Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
                    currentQuizSlide.videoAnswersPath[mediaNum] = mediaPath;
                    yield return null;
                }
                else
                {
                    if (!String.IsNullOrEmpty(mediaPath))
                    {
                        //load in image sprite
                        StartCoroutine(LoadSprite(mediaPath, currentQuizSlide, mediaNum));
                        yield return null;
                    }
                    else
                    {
                        Debug.Log("Image file name is empty.");
                        loaded++;
                        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
                        yield return null;
                    }

                }
                yield return null;
            }

            StartCoroutine(LoadBGSprite(currentDataSlide.bgSpritePath, currentQuizSlide));
            yield return null;
        }

        quizManager.Enable();
        yield return null;
    }

    


IEnumerator LoadBGSprite(string fileName, QuizManager.Slide quizManagerSlide)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            Debug.Log("Image file name is empty.");
            loaded++;
            Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
            yield break;
        }

        string path = Path.Combine(folder, "Assets", "Image", fileName);

        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        loaded++;
        Debug.Log(loaded +"/"+ totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt(loaded / totalToLoad) + "%";

        quizManagerSlide.bgSprite = imageSprite;
        yield return imageSprite;
    }

    IEnumerator LoadSprite(string fileName, QuizManager.Slide quizManagerSlide, int ansNumber)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            Debug.Log("Image file name is empty.");
            loaded++;
            Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
            yield break;
        }

        string path = Path.Combine(folder, "Assets", "Image", fileName);

        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        loaded++;
        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt(loaded / totalToLoad) + "%";

        quizManagerSlide.imageSprites[ansNumber] = imageSprite;
        yield return imageSprite;
    }

     IEnumerator LoadAudio(string fileName)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            Debug.Log("Audio file name is empty.");
            loaded++;
            yield break;
        }
        string filePath = Path.Combine(folder, "Assets", "Audio", fileName);
        var loader = new WWW(filePath);
        yield return loader;


        bgm.clip = loader.GetAudioClip();
        bgm.Play();

        loaded++;
        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt((loaded / totalToLoad) * 100) + "%";


    }
}
