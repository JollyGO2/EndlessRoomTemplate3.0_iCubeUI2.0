using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PublishPreview : MonoBehaviour
{
    public ThumbnailMaking thumbnailMaking;
    public DataAcrossScenes dataPersistence;
    public SlideManager[] slideManagers;


    [Header ("Walls Config")]
    public QuizManager[] configThreeWalls;
    public GameObject setupThreeWalls;
    public QuizManager[] configFourWalls;
    public QuizManager[] loadedWalls;

    [Header("On/Off Gameobjects for Setup")]
    public GameObject[] turnOffForPreview;
    public GameObject[] turnOnForPreview;
    public GameObject[] turnOffForPublish;
    public GameObject[] turnOnForPublish;

    [Header("Editor Manager")]
    public EditorManager editorManager;

    private void Start()
    {
        if (DataAcrossScenes.instance!= null)
        {
            dataPersistence = DataAcrossScenes.instance;
        }
        else
        {
            dataPersistence = FindObjectOfType<DataAcrossScenes>();
        }
    }
    public void Publish()
    {
        thumbnailMaking.StartCoroutine(thumbnailMaking.RecordFrame());
        StartCoroutine(WaitForThumbnail());


    }

    IEnumerator WaitForThumbnail()
    {
        yield return thumbnailMaking.outputSprite;

        Debug.Log("Publish screen on");

        foreach (GameObject o in turnOnForPublish)
        {
            o.SetActive(true);
        }

        foreach (GameObject o in turnOffForPublish)
        {
            o.SetActive(false);
        }
    }

    public void OffPub()
    {
        foreach (GameObject o in turnOnForPublish)
        {
            o.SetActive(false);
        }
        foreach (GameObject o in turnOffForPublish)
        {
            o.SetActive(true);
        }

    }

    public void OffPre()
    {

        foreach (QuizManager q in loadedWalls)
        {
            q.Resetting();
        }

        foreach (GameObject o in turnOffForPreview)
        {
            o.SetActive(true);
        }
        foreach (GameObject o in turnOnForPreview)
        {
            o.SetActive(false);
        }

        setupThreeWalls.SetActive(false);
        editorManager.GetComponent<AudioSource>().Stop();
    }

    public void Preview()
    {
        
        foreach (GameObject o in turnOffForPreview)
        {
            o.SetActive(false);
        }

        foreach (GameObject o in turnOnForPreview)
        {
            o.SetActive(true);
        }
        if (editorManager.bgm != null)
        {
            editorManager.PlayBGM();
        }

        loadedWalls = configFourWalls;

        List<SlideManager> tempSlideManagers = new List<SlideManager>(slideManagers);

        if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            loadedWalls = configThreeWalls;
            setupThreeWalls.SetActive(true);

            tempSlideManagers = new List<SlideManager>() { slideManagers[0], slideManagers[1], slideManagers[3] };
        }



        for (int i = 0; i < loadedWalls.Length; i++)
        {
            loadedWalls[i].slidesList.Clear();

            foreach (SlideManager.Slides man in tempSlideManagers[i].slidesList)
            {
                
               loadedWalls[i].slidesList.Add(new QuizManager.Slide());

                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].bgSprite = man.bgSprite;


                if (man.slideType == SlideManager.Slides.Type.text)
                {
                    loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].slideType = QuizManager.Slide.SlideType.text;
                }
                else if (man.slideType == SlideManager.Slides.Type.image)
                {
                    Debug.Log("Set as image" + loadedWalls[i].name);
                    loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].slideType = QuizManager.Slide.SlideType.image;
                }
                else if (man.slideType == SlideManager.Slides.Type.both)
                {
                    loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].slideType = QuizManager.Slide.SlideType.both;
                }
                else if (man.slideType == SlideManager.Slides.Type.blank)
                {
                    loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].slideType = QuizManager.Slide.SlideType.blank;

                }

                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].correctAnswers = man.correctAnswers;
                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].textAnswers = man.textAnswers;
                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].videoAnswersPath = man.videoPath;
                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].imageSprites = man.imageSprites;
                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].numberOfAnswers = man.choiceCount;
                loadedWalls[i].slidesList[loadedWalls[i].slidesList.Count - 1].questionText = man.questionText;

            }

            loadedWalls[i].transform.parent.gameObject.SetActive(true);
            loadedWalls[i].Enable();
        }




    }
}
