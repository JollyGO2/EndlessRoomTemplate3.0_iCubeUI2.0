using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using TMPro;

[RequireComponent(typeof(MediaLoader))]
public class SlidesData : MonoBehaviour
{
    public TMP_Text loadText;
    [SerializeField] float loaded;
    [SerializeField] float totalToLoad;
    public Animator anim;
    public bool toLoad;


    public static SlidesData instance;

    public SlideManager[] threeWallManagers;
    public SlideManager[] fourWallManagers;

    [System.Serializable]
    public class Slides
    {
        public List<string> textAnswers = new List<string>() { "", "" };
        public List<string> mediaAnswersPath = new List<string>() { null, null };
        // Save a list of string image names then use the placeholder system to load those images (QuizDataSaveLoad)
        public List<Sprite> mediaAnswerSprites = new List<Sprite>() { null, null };
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
        public List<Slides> slidesData = new List<Slides> { };
    }

    public List<SlidesManage> slideManagersData = new List<SlidesManage> { };


    public SlideManager[] managers;

    public DataAcrossScenes data;

    private void Awake()
    {
        if (instance != null)
        {
            if (instance != this)
            {
                Destroy(this);
                return;
            }
        }

        instance = this;

        if (DataAcrossScenes.instance == null)
        {
            data = FindObjectOfType<DataAcrossScenes>();
        }
        else
        {
            data = DataAcrossScenes.instance;
        }



        managers = fourWallManagers;

        if (data.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            managers = threeWallManagers;
        }

        foreach(SlideManager slideManager in managers)
        {
            slideManagersData.Add(new SlidesManage());
        }
    }
    

    public void LoadData()
    {


        totalToLoad = 0;
        loaded = 0;


        for (int managerNum = 0; managerNum < slideManagersData.Count; managerNum++)
        {
            SlideManager slideManager = managers[managerNum];
            List<Slides> slidesDataList = slideManagersData[managerNum].slidesData;


            totalToLoad += slidesDataList.Count; //bg sprites
            for (int slideNum = 0; slideNum < slidesDataList.Count; slideNum++)
            {
                //totalToLoad += slidesDataList[slideNum].mediaAnswersPath.Count; //answer/blank sprite media
                totalToLoad += slidesDataList[slideNum].mediaAnswerSprites.Count; //answer/blank sprite media
            }

            StartCoroutine(LoadWall(slideManager, slidesDataList));
        }


        StartCoroutine(LoadingScreen());

    }

    IEnumerator LoadingScreen()
    {
        while (loaded < totalToLoad)
        {
            if (loadText.gameObject.activeInHierarchy)
            {
                loadText.text = "Loading... \n" + Mathf.RoundToInt((loaded / totalToLoad) * 100) + "%";
            }
            yield return null;
        }

        FindObjectOfType<WallManager>().OnOffWall();
        StartCoroutine(DoneLoading());


        //Loading Finish

        Debug.Log("Editor loading done");

        if (FindObjectOfType<DuplicateWall>())
        {
            FindObjectOfType<DuplicateWall>().gameObject.SetActive(false);
            FindObjectOfType<WallManager>().OnOffWall();
        }
        yield break;
    }
    IEnumerator DoneLoading()
    {
        yield return new WaitForSeconds(1);
        loadText.text = "Done Loading! \n 100%";
        yield return new WaitForSeconds(2);
        anim.SetBool("DoneLoading", true);
    }



    IEnumerator LoadWall(SlideManager slideManager, List<Slides> slidesDataList)
    {
        while (slideManager.slidesList.Count > slidesDataList.Count)
        {
            slideManager.slidesList.RemoveAt(slideManager.slidesList.Count-1);
            yield return null;
        }



        for (int slideNum = 0; slideNum < slidesDataList.Count; slideNum++)
        {
            if (slideManager.slidesList.Count - 1 < slideNum)
            {
                slideManager.slidesList.Add(new SlideManager.Slides());
                yield return null;
            }
            

            SlideManager.Slides currentSlide = slideManager.slidesList[slideNum];
            SlidesData.Slides slideData = slidesDataList[slideNum];

            currentSlide.textAnswers = new List<string>(slideData.textAnswers);
            currentSlide.correctAnswers = new List<bool>(slideData.correctAnswers);
            currentSlide.questionText = slideData.questionText;
            currentSlide.choiceCount = slideData.choiceCount;

            //Debug.Log(currentSlide.slideType + " data, wall " + slideManager.name + " slide " + slideNum);

            if (slideData.slideType == Slides.Type.text)
            {
                currentSlide.slideType = SlideManager.Slides.Type.text;
                yield return null;
            }
            else if (slideData.slideType == Slides.Type.image)
            {
                currentSlide.slideType = SlideManager.Slides.Type.image;
                yield return null;
            }
            else if (slideData.slideType == Slides.Type.both)
            {
                currentSlide.slideType = SlideManager.Slides.Type.both;
                yield return null;
            }
            else
            {
                currentSlide.slideType = SlideManager.Slides.Type.blank;
                yield return null;
            }


            Debug.Log("Checking slideData assigned to slideType");
            /*
            for (int mediaNum = 0; mediaNum < slideData.mediaAnswersPath.Count; mediaNum++)
            {
                string mediaPath = slideData.mediaAnswersPath[mediaNum];

                while (currentSlide.videoPath.Count < slideData.mediaAnswersPath.Count)
                {
                    currentSlide.videoPath.Add(null);
                    yield return null;
                }
                while (currentSlide.videoPath.Count > slideData.mediaAnswersPath.Count)
                {
                    currentSlide.videoPath.RemoveAt(currentSlide.videoPath.Count - 1);
                    yield return null;
                }

                while (currentSlide.imageSprites.Count < slideData.mediaAnswersPath.Count)
                {
                    currentSlide.imageSprites.Add(null);
                    yield return null;
                }
                while (currentSlide.imageSprites.Count < slideData.mediaAnswersPath.Count)
                {
                    currentSlide.imageSprites.RemoveAt(currentSlide.imageSprites.Count - 1);
                    yield return null;
                }




                if (Path.GetExtension(mediaPath) == ".mp4")
                {
                    currentSlide.videoPath[mediaNum] = mediaPath;
                    loaded++;
                    yield return null;
                }
                else
                {
                    if (!String.IsNullOrEmpty(mediaPath))
                    {
                        //currentSlide.imageSprites[mediaNum] = MediaLoader.LoadSprite(mediaPath, false);

                        StartCoroutine(LoadSprite(mediaPath, currentSlide, mediaNum));
                        yield return null;
                    }
                    else
                    {
                        loaded++;
                        currentSlide.imageSprites[mediaNum] = null;
                        yield return null;
                    }
                    yield return null;

                }
                yield return null;
            }*/
            for (int mediaNum = 0; mediaNum < slideData.mediaAnswerSprites.Count; mediaNum++)
            {
                Sprite thisSprite = slideData.mediaAnswerSprites[mediaNum];

                while (currentSlide.videoPath.Count < slideData.mediaAnswerSprites.Count)
                {
                    currentSlide.videoPath.Add(null);
                    yield return null;
                }
                while (currentSlide.videoPath.Count > slideData.mediaAnswerSprites.Count)
                {
                    currentSlide.videoPath.RemoveAt(currentSlide.videoPath.Count - 1);
                    yield return null;
                }

                while (currentSlide.imageSprites.Count < slideData.mediaAnswerSprites.Count)
                {
                    currentSlide.imageSprites.Add(null);
                    yield return null;
                }
                while (currentSlide.imageSprites.Count < slideData.mediaAnswerSprites.Count)
                {
                    currentSlide.imageSprites.RemoveAt(currentSlide.imageSprites.Count - 1);
                    yield return null;
                }




                if (!thisSprite)
                {
                    //currentSlide.imageSprites[mediaNum] = MediaLoader.LoadSprite(mediaPath, false);

                    StartCoroutine(SpecialLoadSprite(thisSprite, currentSlide, mediaNum));
                    yield return null;
                }
                else
                {
                    loaded++;
                    currentSlide.imageSprites[mediaNum] = null;
                    yield return null;
                }
                yield return null;
            }

            //currentSlide.bgSprite = MediaLoader.LoadSprite(slideData.bgSpritePath, false);
            StartCoroutine(LoadBGSprite(slideData.bgSpritePath, currentSlide));
            yield return null;

        }

        yield return null;
    }

    public void Duplicate(SlideManager copyFrom, SlideManager copyTo)
    {
        int copyFromThis = Array.IndexOf(managers, copyFrom);
        int copyToThis = Array.IndexOf(managers, copyTo);

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Duplicating.ER"); //.quiz
        ES3.Save("Duplicating", slideManagersData[copyFromThis].slidesData,path);

        slideManagersData[copyToThis].slidesData = ES3.Load <List<Slides>>("Duplicating",path);

        File.Delete(path);

        LoadData();
    }

    public void DataUpdate(SlideManager slideManager)
    {
        List<Slides> slideList =  slideManagersData[Array.IndexOf(managers, slideManager)].slidesData;
        //Debug.Log("Data Update Called, Slide data slides count is " + slideManagersData[Array.IndexOf(managers, slideManager)].slidesData.Count + ", Slide manager count" + slideManager.slidesList.Count);
      

        while (slideManagersData[Array.IndexOf(managers, slideManager)].slidesData.Count < slideManager.slidesList.Count)
        {
            
            slideManagersData[Array.IndexOf(managers, slideManager)].slidesData.Add(new Slides());
        }
        

        for (int i = 0; i < slideManager.slidesList.Count; i++)
        {
            slideList[i].correctAnswers = slideManager.slidesList[i].correctAnswers;

            slideList[i].textAnswers = slideManager.slidesList[i].textAnswers;
            slideList[i].questionText = slideManager.slidesList[i].questionText;

            //Debug.Log("aaaa before dataupdate" + slideList[i].slideType + " slide " + i);
            if (slideManager.slidesList[i].slideType == SlideManager.Slides.Type.text)
            {
                slideList[i].slideType = Slides.Type.text;
            }
            else if (slideManager.slidesList[i].slideType == SlideManager.Slides.Type.image)
            {
                slideList[i].slideType = Slides.Type.image;
            }
            else if (slideManager.slidesList[i].slideType == SlideManager.Slides.Type.both)
            {
                slideList[i].slideType = Slides.Type.both;
            }
            else
            {
                slideList[i].slideType = Slides.Type.blank;
            }
            //Debug.Log("aaaa dataupdate" + slideList[i].slideType + " slide " + i);


            slideList[i].choiceCount = slideManager.slidesList[i].choiceCount;

            //Old Code
            /*
            while (slideList[i].mediaAnswersPath.Count < slideList[i].choiceCount)
            {
                slideList[i].mediaAnswersPath.Add(null);
            }
            while (slideList[i].mediaAnswersPath.Count > slideList[i].choiceCount)
            {
                slideList[i].mediaAnswersPath.RemoveAt(slideList[i].mediaAnswersPath.Count-1);
            }
            */
            while (slideList[i].mediaAnswerSprites.Count < slideList[i].choiceCount)
            {
                slideList[i].mediaAnswerSprites.Add(null);
            }
            while (slideList[i].mediaAnswerSprites.Count > slideList[i].choiceCount)
            {
                slideList[i].mediaAnswerSprites.RemoveAt(slideList[i].mediaAnswerSprites.Count - 1);
            }

            while (slideList[i].textAnswers.Count < slideList[i].choiceCount)
            {
                slideList[i].textAnswers.Add(null);
            }
            while (slideList[i].textAnswers.Count > slideList[i].choiceCount)
            {
                slideList[i].textAnswers.RemoveAt(slideList[i].textAnswers.Count - 1);
            }

            while (slideList[i].correctAnswers.Count < slideList[i].choiceCount)
            {
                slideList[i].correctAnswers.Add(false);
            }
            while (slideList[i].correctAnswers.Count > slideList[i].choiceCount)
            {
                slideList[i].correctAnswers.RemoveAt(slideList[i].correctAnswers.Count - 1);
            }
        }
    }

    public void MoveSlides(SlideManager slideManager, int whichMoved, int toWhere)
    {
        SlidesManage manager = slideManagersData[Array.IndexOf(managers, slideManager)];


        Debug.Log("moving slides for " + name);
        Debug.Log("From " + whichMoved + " To " + toWhere);
        Slides slide = manager.slidesData[whichMoved];
        List<Slides> tempSlideList = new List<Slides>();

        for (int i = 0; i < manager.slidesData.Count; i++)
        {
            if (i == toWhere)
            {
                tempSlideList.Add(slide);
                continue;
            }



            if (toWhere < whichMoved)
            {
                if (i <= whichMoved && i > toWhere)
                {
                    tempSlideList.Add(manager.slidesData[i - 1]);
                }
                else
                {
                    tempSlideList.Add(manager.slidesData[i]);
                }
            }
            else
            {
                if (i >= whichMoved && i < toWhere)
                {
                    tempSlideList.Add(manager.slidesData[i + 1]);
                }
                else
                {

                    tempSlideList.Add(manager.slidesData[i]);
                }
            }
        }

        manager.slidesData = tempSlideList;
    }

    public void MediaAnsUpdate(SlideManager slideManager, int slideNum, int answerNum, string fileName)
    {
        List<Slides> slideList = slideManagersData[Array.IndexOf(managers, slideManager)].slidesData;


        while (slideList[slideNum].mediaAnswersPath.Count < slideList[slideNum].choiceCount)
        {
            slideList[slideNum].mediaAnswersPath.Add(null);
        }
        while (slideList[slideNum].mediaAnswersPath.Count > slideList[slideNum].choiceCount)
        {
            slideList[slideNum].mediaAnswersPath.RemoveAt(slideList[slideNum].mediaAnswersPath.Count - 1);
        }

        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].mediaAnswersPath[answerNum] = fileName;
    }

    public void SpecialMediaAnsUpdate(SlideManager slideManager, int slideNum, int answerNum, Sprite sprite)
    {
        List<Slides> slideList = slideManagersData[Array.IndexOf(managers, slideManager)].slidesData;


        while (slideList[slideNum].mediaAnswerSprites.Count < slideList[slideNum].choiceCount)
        {
            slideList[slideNum].mediaAnswerSprites.Add(null);
        }
        while (slideList[slideNum].mediaAnswerSprites.Count > slideList[slideNum].choiceCount)
        {
            slideList[slideNum].mediaAnswerSprites.RemoveAt(slideList[slideNum].mediaAnswerSprites.Count - 1);
        }

        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].mediaAnswerSprites[answerNum] = sprite;
    }

    public void BGUpdate(SlideManager slideManager, int slideNum, string fileName)
    {

        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].bgSpritePath = fileName;
    }
    public void BGUpdateAll(SlideManager slideManager, int slideNum, int copyFromSlide)
    {

        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].bgSpritePath = 
            slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[copyFromSlide].bgSpritePath;
    }

    public void ChangeAnswerClear(SlideManager slideManager, int slideNum)
    {

        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].textAnswers.Clear();
        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].mediaAnswersPath.Clear();

        Slides.Type slideType = Slides.Type.text;
        SlideManager.Slides slide = slideManager.slidesList[slideNum];

        if (slide.slideType == SlideManager.Slides.Type.text)
        {
            slideType = Slides.Type.text;
        }
        else if (slide.slideType == SlideManager.Slides.Type.image)
        {
            slideType = Slides.Type.image;
        }
        else if (slide.slideType == SlideManager.Slides.Type.both)
        {
            slideType = Slides.Type.both;
        }
        else
        {
            slideType = Slides.Type.blank;
        }
        Debug.Log("aaaa change answer thing");
        slideManagersData[Array.IndexOf(managers, slideManager)].slidesData[slideNum].slideType = slideType;
    }

    IEnumerator LoadBGSprite(string fileName, SlideManager.Slides slideManagerSlide)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            slideManagerSlide.bgSprite = null;
            Debug.Log("Image file name is empty.");
            loaded++;
            Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
            yield break;
        }

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Image", fileName);

        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        loaded++;
        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt(loaded / totalToLoad) + "%";

        slideManagerSlide.bgSprite = imageSprite;
    }

    IEnumerator LoadSprite(string fileName, SlideManager.Slides slideManagerSlide, int ansNumber)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            slideManagerSlide.imageSprites[ansNumber] = null;
            Debug.Log("Image file name is empty.");
            loaded++;
            Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
            yield break;
        }

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Image", fileName);

        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        loaded++;
        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt(loaded / totalToLoad) + "%";

        slideManagerSlide.imageSprites[ansNumber] = imageSprite;
    }

    IEnumerator SpecialLoadSprite(Sprite sprite, SlideManager.Slides slideManagerSlide, int ansNumber)
    {
        if (sprite == null)
        {
            slideManagerSlide.imageSprites[ansNumber] = null;
            Debug.Log("Image sprite is empty.");
            loaded++;
            Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
            yield break;
        }

        Sprite imageSprite = sprite;

        loaded++;
        Debug.Log(loaded + "/" + totalToLoad + "% of player loading done");
        //loading.text = Mathf.RoundToInt(loaded / totalToLoad) + "%";

        slideManagerSlide.imageSprites[ansNumber] = imageSprite;
    }
}
