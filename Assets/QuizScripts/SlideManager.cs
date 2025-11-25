using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;


public class SlideManager : MonoBehaviour
{
    public DataAcrossScenes dataPersistence;

    [Header("Slides data")]

    public List<Slides> slidesList;

    [System.Serializable]
    public class Slides
    {
        public List<string> textAnswers = new List<string>() { "", "" };
        public List<string> videoPath = new List<string>() { null, null };
        public List<Sprite> imageSprites = new List<Sprite>() { null, null };
        public List<bool> correctAnswers = new List<bool>() { true, false };
        public string questionText;
        public enum Type { text, image, both, blank };
        public Type slideType = Type.both;
        public int choiceCount = 3;

        public Sprite bgSprite;
    }

    bool firstaction;
    public bool skipaction;

    [Header("Showing Slides data QUESTION")]

    public GameObject textChoices;
    public GameObject imageChoices;
    public GameObject bothChoices;
    public GameObject quizLayout;
    public TMP_InputField questionInput;

    [Header("Showing Slides data BLANK")]
    public GameObject blankLayout;
    public GameObject blankImageInput;
    public GameObject blankAddImageButton;
    public TMP_InputField blankTextInput;
    public GameObject blankDeleteButton;


    [Header("Left bar slides buttons")]
    public List<Button> slideButtonsList;
    public GameObject slideButtonsHolder;
    public GameObject slideButtonsPrefab;


    [Header("Slide to Show")]
    public int currentSlide;


    [Header("Navigation buttons")]
    public GameObject upButton;
    public GameObject downButton;
    public GameObject addSlideButton;
    public GameObject deleteSlideButton;


    [Header("Quiz Navigation buttons")]
    public GameObject[] quizNavButtons;

    [Header("Customization Dropdowns")]
    public TMP_Dropdown answerCount;
    public TMP_Dropdown answerType;
    public ChangeAnsType changeAnsType;



    [Header("Background Customization")]
    public Image bgPreview;
    public Sprite defaultBG;
    public GameObject bgPreviewText;
    public Image bgImage;

    [Header("Audio Customization")]
    public GameObject removeBGMButton;
    public TMP_Text bgmText;

    [Header("Addons")]
    public GameObject dubWallButton;
    public TMP_Text timer;
    public TMP_InputField timerInput;
    public TMP_InputField passingTextInput;
    [SerializeField] GameObject newSlideButton;



    private void Start()
    {
        if (DataAcrossScenes.instance != null)
        {
            dataPersistence = DataAcrossScenes.instance;
        }

        // setting choices number
        for (int i = 0; i < 8; i++)
        {
            textChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
            imageChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
            bothChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
        }
        dubWallButton.SetActive(false);
        answerType.gameObject.SetActive(false);
    }



    // image saving
    public void IMGANS(string fileName, int answerNumber)
    {
        SlidesData.instance.MediaAnsUpdate(this, currentSlide, answerNumber, fileName);

        if (Path.GetExtension(fileName) == ".mp4")
        {
            slidesList[currentSlide].videoPath[answerNumber] = fileName;
            slidesList[currentSlide].imageSprites[answerNumber] = null;


        }
        else
        {
            slidesList[currentSlide].imageSprites[answerNumber] = MediaLoader.LoadSprite(fileName, false);
            slidesList[currentSlide].videoPath[answerNumber] = null;
        }

        ReloadSlide();
    }
    public void RemoveAudio()
    {
        FindObjectOfType<EditorManager>().bgm = null;
        FindObjectOfType<EditorManager>().bgmFileName = null;
        FindObjectOfType<WallManager>().AudioCheck();
    }

    //question saving
    public void SaveQN(TMP_InputField i)
    {
        if (slidesList[currentSlide].questionText != i.text)
        {
            UndoRedo.instance.Action();
        }
        slidesList[currentSlide].questionText = i.text;

        ReloadSlide();
    }

    public void UndoRedoSlideSave()
    {

        //UndoRedo.instance.Action();
    }

    //text saving
    public void SaveANS(GameObject a)
    {
        TMP_InputField inputField = a.GetComponentInChildren<TMP_InputField>();

        if (string.IsNullOrEmpty(a.GetComponentInChildren<TMP_InputField>().text))
        {
            Debug.LogWarning("Input rejected and defaulted to 1");
            inputField.text = "1";
            inputField.onEndEdit.Invoke(inputField.text);
            return;
        }
        else
        {
            int pScore;
            string t = a.GetComponentInChildren<TMP_InputField>().text;
            if (int.TryParse(t, out pScore))
            {
                Debug.Log("Converted to int: " + pScore);

            }
            else
            {
                Debug.LogWarning("Text is not a valid integer: " + t);
            }

            if(pScore < 1)
            {
                Debug.LogWarning("Input rejected and defaulted to 1");
                inputField.text = "1";
                inputField.onEndEdit.Invoke(inputField.text);
                return;
            }
        }

        GameObject o = textChoices;

        for (int t = 0; t < slidesList[currentSlide].textAnswers.Count; t++)
        {
            if (slidesList[currentSlide].textAnswers[t] != o.transform.GetChild(t).GetComponentInChildren<TMP_InputField>().text)
            {
                UndoRedo.instance.Action();
            }
        }


        if (slidesList[currentSlide].slideType == Slides.Type.both)
        {
            o = bothChoices;
        }


        for (int t = 0; t < slidesList[currentSlide].textAnswers.Count; t++)
        {

            slidesList[currentSlide].textAnswers[t] = o.transform.GetChild(t).GetComponentInChildren<TMP_InputField>().text;
            Debug.Log("Asnwer save");
        }

        SlidesData.instance.DataUpdate(this);


    }

    public void SavePassingScore(GameObject a)
    {

        int pScore;
        string t = a.GetComponent<TMP_InputField>().text;

        if (int.TryParse(t, out pScore))
        {
            Debug.Log("Converted to int: " + pScore);
            UndoRedo.instance.Action(); //Save previous state first

            FindObjectOfType<EditorManager>().passingScore = pScore;

        }
        else
        {
            Debug.LogWarning("Text is not a valid integer: " + t);
        }

    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    //text saving
    public void SaveTimer(GameObject a)
    {

        int time;
        string t = a.GetComponent<TMP_InputField>().text;

        if (int.TryParse(t, out time))
        {
            Debug.Log("Converted to int: " + time);
            UndoRedo.instance.Action(); //Save previous state first

            SetTimer(time);

        }
        else
        {
            Debug.LogWarning("Text is not a valid integer: " + t);
        }

    }

    public void SetTimer(int time)
    {
        //Update Timer
        FindObjectOfType<EditorManager>().totalTime = time;
        ReloadSlide();
    }

    public void UpdateTimer(int time)
    {
        timer.text = FormatTime(time);
        timerInput.SetTextWithoutNotify(time + " s");
    }

    //correct/wrong saving
    public void ANSToggle(GameObject a)
    {
        if (!firstaction)
        {
            firstaction = true;
        }
        else if (!skipaction)
        {
            UndoRedo.instance.Action();
        }


        GameObject o = textChoices;

        if (slidesList[currentSlide].slideType == Slides.Type.image)
        {
            o = imageChoices;
        }
        else if (slidesList[currentSlide].slideType == Slides.Type.both)
        {
            o = bothChoices;
        }



        if (!a.GetComponentInChildren<Toggle>().isOn)
        {
            int correctnum = 0;
            int savecorrect = 0;
            for (int i = 0; i < slidesList[currentSlide].correctAnswers.Count; i++)
            {
                if (slidesList[currentSlide].correctAnswers[i] == true)
                {
                    correctnum++;
                    savecorrect = i;
                }
            }

            if (correctnum == 1)
            {
                for (int t = 0; t < slidesList[currentSlide].choiceCount; t++)
                {
                    if (a == o.transform.GetChild(t).gameObject)
                    {
                        if (slidesList[currentSlide].correctAnswers[t] == true)
                        {
                            a.GetComponentInChildren<Toggle>().isOn = true;
                            return;
                        }
                    }
                }
            }
        }




        for (int t = 0; t < slidesList[currentSlide].choiceCount; t++)
        {
            if (a == o.transform.GetChild(t).gameObject)
            {
                slidesList[currentSlide].correctAnswers[t] = o.transform.GetChild(t).GetComponentInChildren<Toggle>().isOn;
                return;
            }
        }

        SlidesData.instance.DataUpdate(this);

    }

    public void BlankImage(string path)
    {
        UndoRedo.instance.Action();
        SlidesData.instance.MediaAnsUpdate(this, currentSlide, 0, path);


        if (Path.GetExtension(path) == ".mp4")
        {
            slidesList[currentSlide].videoPath[0] = path;
            slidesList[currentSlide].imageSprites[0] = null;
        }
        else
        {
            if (slidesList[currentSlide].imageSprites.Count == 0)
            {
                slidesList[currentSlide].imageSprites.Add(null);
            }

            slidesList[currentSlide].imageSprites[0] = MediaLoader.LoadSprite(path, false);
            slidesList[currentSlide].videoPath[0] = null;
        }

        slidesList[currentSlide].questionText = "";
        ReloadSlide();
    }

    public void PressChangeBG(string fileName)
    {
        UndoRedo.instance.Action();
        SlidesData.instance.BGUpdate(this, currentSlide, fileName);

        slidesList[currentSlide].bgSprite = MediaLoader.LoadSprite(fileName, false);
        bgPreviewText.SetActive(false);

        ReloadSlide();

    }


    public void PressBGAll()
    {
        UndoRedo.instance.Action();
        Sprite s = slidesList[currentSlide].bgSprite;

        foreach (Slides o in slidesList)
        {
            o.bgSprite = s;
            SlidesData.instance.BGUpdateAll(this, slidesList.IndexOf(o), currentSlide);
        }
    }

    public void AddSlide(bool blank)
    {
        if (slidesList.Count >= 99)
        {
            return;
        }

        if (!skipaction)
        {
            UndoRedo.instance.Action();
            skipaction = true;
            slidesList.Add(new Slides());


            if (blank)
            {
                slidesList[slidesList.Count - 1].slideType = Slides.Type.blank;
            }

            if (slidesList[currentSlide].slideType != Slides.Type.text)
            {
                changeAnsType.ignoreAction = true;
            }

            currentSlide = slidesList.Count - 1;

        }

        Button b = Instantiate(slideButtonsPrefab, slideButtonsHolder.transform).GetComponentInChildren<Button>();
        slideButtonsList.Add(b);
        b.onClick.AddListener(delegate { ToSlide(b); });
        b.transform.parent.GetComponentInChildren<TMP_Text>().text = (currentSlide - 1).ToString();
        //upButton.SetActive(true);

        SlidesData.instance.DataUpdate(this);

        //ReloadSlide();

        //MOVE
        newSlideButton.transform.SetAsLastSibling();
    }

    public void UpSlide()
    {
        UndoRedo.instance.Action();
        skipaction = true; Debug.Log("skip action");
        currentSlide--;

        ReloadSlide();
    }
    public void DownSlide()
    {
        UndoRedo.instance.Action();
        skipaction = true; Debug.Log("skip action");
        currentSlide++;

        ReloadSlide();
    }

    public void ToSlide(Button b)
    {
        UndoRedo.instance.Action();
        skipaction = true; Debug.Log("skip action");
        int slide = slideButtonsList.IndexOf(b);
        currentSlide = slide;
        ReloadSlide();
    }

    public void AnswerType(TMP_Dropdown d)
    {
        changeAnsType.gameObject.SetActive(true);

        changeAnsType.WantChange(d, this);

    }

    public void ChangeAns(TMP_Dropdown d)
    {
        ChangeAnsType(d.value);
    }

    public void ChangeAnsType(int type)
    {
        skipaction = true;
        UndoRedo.instance.Action();

        Slides man = slidesList[currentSlide];

        GameObject[] items = new GameObject[] { blankLayout, textChoices, imageChoices, bothChoices };

        foreach (GameObject g in items)
        {
            g.SetActive(false);
        }

        GameObject o = textChoices;
        if (currentSlide == 1)
        {
            Debug.Log("aaa3 " + currentSlide);
        }
        switch (type)
        {
            case 0:
                man.slideType = Slides.Type.text;
                break;
            case 1:
                man.slideType = Slides.Type.image;
                o = imageChoices;
                break;
            case 2:

                man.slideType = Slides.Type.both;
                o = bothChoices;
                break;
        }
        if (currentSlide == 1)
        {
            Debug.Log("aaa4 " + currentSlide);
        }

        man.textAnswers.Clear();
        man.imageSprites.Clear();
        man.videoPath.Clear();
        SlidesData.instance.ChangeAnswerClear(this, currentSlide);
        o.SetActive(true);

        slidesList[currentSlide].correctAnswers.Clear();
        ReloadSlide();

    }


    public void AnswerNum(TMP_Dropdown d)
    {
        ChangeAnswerNum(d.value);
    }

    private void ChangeAnswerNum(int num)
    {
        if (!skipaction)
        {
            UndoRedo.instance.Action();
        }

        switch (num)
        {

            case 0:
                slidesList[currentSlide].choiceCount = 2;
                break;
            case 1:
                slidesList[currentSlide].choiceCount = 3;
                break;
            case 2:
                slidesList[currentSlide].choiceCount = 4;
                break;
            case 3:
                slidesList[currentSlide].choiceCount = 5;
                break;
        }


        NumAns();
        ReloadSlide();

    }

    void NumAns()
    {


        Slides man = slidesList[currentSlide];

        while (man.textAnswers.Count > man.choiceCount)
        {
            man.textAnswers.Remove(slidesList[currentSlide].textAnswers[man.textAnswers.Count - 1]);
        }

        while (man.textAnswers.Count < man.choiceCount)
        {
            man.textAnswers.Add("");

        }

        while (man.correctAnswers.Count > man.choiceCount)
        {
            man.correctAnswers.RemoveAt(man.correctAnswers.Count - 1);
        }

        while (man.correctAnswers.Count < man.choiceCount)
        {
            man.correctAnswers.Add(false);
        }


        while (man.videoPath.Count > man.choiceCount)
        {
            man.videoPath.Remove(man.videoPath[man.videoPath.Count - 1]);
        }

        while (man.videoPath.Count < man.choiceCount)
        {
            man.videoPath.Add("");
        }

        while (man.imageSprites.Count > man.choiceCount)
        {
            man.imageSprites.Remove(man.imageSprites[man.imageSprites.Count - 1]);
        }

        while (man.imageSprites.Count < man.choiceCount)
        {
            man.imageSprites.Add(null);
        }



        GameObject[] items = new GameObject[] { blankLayout, textChoices, imageChoices, bothChoices };
        foreach (GameObject g in items)
        {
            g.SetActive(false);
        }

        if (man.slideType != Slides.Type.blank)
        {
            GameObject o = textChoices;

            quizLayout.SetActive(true);

            if (man.slideType == Slides.Type.image)
            {
                o = imageChoices;
            }
            else if (man.slideType == Slides.Type.both)
            {
                o = bothChoices;

            }

            o.SetActive(true);


            for (int i = 0; i < 8; i++)
            {
                if (i < slidesList[currentSlide].choiceCount)
                {
                    o.transform.GetChild(i).gameObject.SetActive(true);
                    if (man.correctAnswers[i] == true)
                    {
                        //skipaction = true; Debug.Log("skip action");
                        o.transform.GetChild(i).GetChild(1).GetComponent<Toggle>().isOn = true;
                    }
                    else
                    {
                        //skipaction = true; Debug.Log("skip action");
                        o.transform.GetChild(i).GetChild(1).GetComponent<Toggle>().isOn = false;
                    }
                }
                else
                {

                    o.transform.GetChild(i).gameObject.SetActive(false);
                    o.transform.GetChild(i).GetChild(1).GetComponent<Toggle>().isOn = false;
                }


            }


            //checking for any correct toggle. if none, default 1st option as correct
            for (int i = 0; i < slidesList[currentSlide].choiceCount; i++)
            {
                //Debug.Log("Checking for any correct choices");
                if (slidesList[currentSlide].correctAnswers[i] == true)
                {
                    //Debug.Log("Answer " + i + " is correct");
                    break;
                }
                else
                {
                    if (i == slidesList[currentSlide].correctAnswers.Count - 1)
                    {
                        //Debug.Log("Default 1st correct");
                        slidesList[currentSlide].correctAnswers[0] = true;
                        ANSToggle(o.transform.GetChild(0).gameObject);
                    }
                }
            }

        }
        else if (man.slideType == Slides.Type.blank)
        {
            blankLayout.SetActive(true);
        }

        //Debug.Log(this.name + slidesList[1].slideType + " numans from managerrr");

        SlidesData.instance.DataUpdate(this);

    }


    public void Delete()
    {
        UndoRedo.instance.Action();
        skipaction = true;

        slidesList.Remove(slidesList[currentSlide]);
        Destroy(slideButtonsList[currentSlide].transform.parent.gameObject);
        slideButtonsList.Remove(slideButtonsList[currentSlide]);

        SlidesData.instance.DataUpdateAfterDeletion(this, currentSlide); //Update data after deletion

        if (currentSlide != 0)
        {
            currentSlide--;
        }

        ReloadSlide();
    }

    public void ReloadSlide()
    {
        while (slideButtonsList.Count < slidesList.Count)
        {
            skipaction = true; Debug.Log("skip action reload addslide");
            AddSlide(slidesList[slideButtonsList.Count].slideType == Slides.Type.blank);

        }


        //Debug.Log("Reloading Slide");

        Slides man = slidesList[currentSlide];

        questionInput.text = man.questionText;

        changeAnsType.ignoreAction = true;
        if (man.bgSprite != null)
        {
            bgPreview.sprite = man.bgSprite;
            bgImage.sprite = man.bgSprite;
            bgPreviewText.SetActive(false);
        }
        else
        {
            bgPreviewText.SetActive(true);
            bgPreview.sprite = defaultBG;
            bgImage.sprite = null;
        }

        NumAns();


        //Resetting slide to blank slate (turn off everything so nothing that should be on will be on
        GameObject[] items = new GameObject[] { quizLayout, blankLayout, textChoices, imageChoices, bothChoices };
        foreach (GameObject g in items)
        {
            g.SetActive(false);
        }
        //****************************

        if (man.slideType != Slides.Type.blank)
        {
            quizLayout.SetActive(true);


            //Getting choice layouts
            GameObject o = textChoices;
            Transform textans = textChoices.transform;
            Transform mediaans = imageChoices.transform;

            if (man.slideType == Slides.Type.image)
            {
                o = imageChoices;
            }
            else if (man.slideType == Slides.Type.both)
            {
                o = bothChoices;
                textans = bothChoices.transform;
                mediaans = bothChoices.transform;

            }

            o.SetActive(true);

            for (int i = 0; i < textans.childCount; i++)
            {
                if (i < man.choiceCount)
                {
                    if (textans.gameObject.activeInHierarchy)
                    {
                        if(textans.transform.GetChild(i).GetComponentInChildren<TMP_InputField>() != null)
                        {
                            textans.transform.GetChild(i).GetComponentInChildren<TMP_InputField>().text = man.textAnswers[i];
                        }
                    }
                }
                else
                {
                    if (textans == bothChoices.transform)
                    {
                        textans.GetChild(i).GetChild(3).GetComponentInChildren<TMP_InputField>().text = "";

                    }
                    else
                    {
                        textans.GetChild(i).GetChild(2).GetComponentInChildren<TMP_InputField>().text = "";
                    }
                }

                if (i < man.choiceCount)
                {
                    if (mediaans.gameObject.activeInHierarchy)
                    {
                        Transform mediaAnsHolder = mediaans.GetChild(i).GetChild(2);

                        if (String.IsNullOrEmpty(man.videoPath[i]))
                        {

                            if (man.imageSprites[i] == null)
                            {
                                //Debug.Log("No media for answer " + i);
                                StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, defaultBG));
                                mediaAnsHolder.GetChild(0).gameObject.SetActive(true);
                                mediaAnsHolder.Find("RemoveButton").GetComponent<SlotHolderInteract>().SetInteractableRemoveButton(false);
                            }
                            else
                            {
                                mediaAnsHolder.GetChild(0).gameObject.SetActive(false);
                                StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, man.imageSprites[i]));
                                mediaAnsHolder.Find("RemoveButton").GetComponent<SlotHolderInteract>().SetInteractableRemoveButton(true);
                            }

                        }
                        else
                        {
                            mediaAnsHolder.GetChild(0).gameObject.SetActive(false);
                            StartCoroutine(MediaLoader.SetupVideo(mediaAnsHolder, man.videoPath[i]));

                        }
                    }
                }
                else
                {
                    StartCoroutine(MediaLoader.SetupImage(mediaans.GetChild(i).GetChild(2), defaultBG));
                }
            }

            for (int i = 0; i < man.correctAnswers.Count; i++)
            {
                if (man.correctAnswers[i] == true)
                {
                    o.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = true;
                }
                else
                {
                    o.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
                }
            }
        }
        else
        {
            blankLayout.SetActive(true);
            IfBlank(man);
        }

        //ChangeNavButtons();



        //changing slide buttons indicator
        foreach (Button b in slideButtonsList)
        {
            b.GetComponent<Image>().color = Color.white;
            b.transform.parent.GetComponentInChildren<TMP_Text>().text = (slideButtonsList.IndexOf(b) + 1).ToString();
        }
        slideButtonsList[currentSlide].GetComponent<Image>().color = new Color(0.75f, 0.75f, 0.75f);


        //getting answer number and types
        switch (man.choiceCount)
        {
            case 2:
                answerCount.value = 0;
                break;
            case 3:
                answerCount.value = 1;
                break;
            case 4:
                answerCount.value = 2;
                break;
            case 5:
                answerCount.value = 3;
                break;

        }

        if (man.slideType == Slides.Type.image)
        {
            answerType.value = 1;
        }
        else if (man.slideType == Slides.Type.text)
        {
            answerType.value = 0;
        }
        else if (man.slideType == Slides.Type.both)
        {
            answerType.value = 2;
        }

        if (man.slideType == Slides.Type.blank)
        {
            answerCount.gameObject.SetActive(false);
            answerType.gameObject.SetActive(false);
        }
        else
        {
            answerCount.gameObject.SetActive(true);
            //answerType.gameObject.SetActive(true);
        }


        changeAnsType.ignoreAction = false;




        while (slidesList.Count != slideButtonsList.Count)
        {
            if (slidesList.Count > slideButtonsList.Count)
            {
                //Debug.Log(slidesList.Count);
                Button b = Instantiate(slideButtonsPrefab, slideButtonsHolder.transform).GetComponentInChildren<Button>();
                slideButtonsList.Add(b);
                b.onClick.AddListener(delegate { ToSlide(b); });
                b.transform.parent.GetComponentInChildren<TMP_Text>().text = (currentSlide - 1).ToString();

            }

            else if (slidesList.Count < slideButtonsList.Count)
            {
                Destroy(slideButtonsList[slideButtonsList.Count - 1].transform.parent.gameObject);
                slideButtonsList.Remove(slideButtonsList[slideButtonsList.Count - 1]);
            }
        }

        UpdateTimer(FindObjectOfType<EditorManager>().totalTime);
        passingTextInput.SetTextWithoutNotify(FindObjectOfType<EditorManager>().passingScore.ToString());

        skipaction = false;
    }

    public void MovedSlides(int whichMoved, int toWhere)
    {


        UndoRedo.instance.Action();

        Debug.Log("moving slides for " + name);
        Debug.Log("From " + whichMoved + " To " + toWhere);
        Slides slide = slidesList[whichMoved];
        List<Slides> tempSlideList = new List<Slides>();

        for (int i = 0; i < slidesList.Count; i++)
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
                    tempSlideList.Add(slidesList[i - 1]);
                }
                else
                {
                    tempSlideList.Add(slidesList[i]);
                }
            }
            else
            {
                if (i >= whichMoved && i < toWhere)
                {
                    tempSlideList.Add(slidesList[i + 1]);
                }
                else
                {

                    tempSlideList.Add(slidesList[i]);
                }
            }
        }

        slidesList = tempSlideList;




        Button but = slideButtonsList[whichMoved];
        List<Button> tempButtonList = new List<Button>();


        for (int i = 0; i < slideButtonsList.Count; i++)
        {
            if (i == toWhere)
            {
                tempButtonList.Add(but);
                continue;
            }

            if (toWhere < whichMoved)
            {
                if (i <= whichMoved && i > toWhere)
                {
                    tempButtonList.Add(slideButtonsList[i - 1]);
                }
                else
                {
                    tempButtonList.Add(slideButtonsList[i]);
                }
            }
            else
            {
                if (i >= whichMoved && i < toWhere)
                {
                    tempButtonList.Add(slideButtonsList[i + 1]);
                }
                else
                {

                    tempButtonList.Add(slideButtonsList[i]);
                }
            }
        }

        slideButtonsList = tempButtonList;

        SlidesData.instance.MoveSlides(this, whichMoved, toWhere);
        currentSlide = toWhere;
        ReloadSlide();
    }

    void ChangeNavButtons()
    {
        int buttonon = 0;
        //next, finish, back buttons
        if (slidesList.Count == 1)
        {
            buttonon = 3;
            //addSlideButton.SetActive(true);
            downButton.SetActive(false);
            upButton.SetActive(false);


            //deleteSlideButton.SetActive(false);
            deleteSlideButton.GetComponent<Button>().interactable = false;
        }
        else
        {
            deleteSlideButton.SetActive(true);
            deleteSlideButton.GetComponent<Button>().interactable = true;
            if (currentSlide == 0)
            {
                buttonon = 0;
                //addSlideButton.SetActive(false);
                downButton.SetActive(true);
                upButton.SetActive(false);

            }
            else if (currentSlide == slidesList.Count - 1)
            {
                buttonon = 2;
                downButton.SetActive(false);
                upButton.SetActive(true);

                if (slidesList.Count >= 99)
                {
                    //addSlideButton.SetActive(false);
                }
                else
                {
                    //addSlideButton.SetActive(true);
                }
            }
            else
            {
                buttonon = 1;
                //addSlideButton.SetActive(false);
                downButton.SetActive(true);
                upButton.SetActive(true);

            }
        }

        foreach (GameObject b in quizNavButtons)
        {
            b.SetActive(false);
        }

        //quizNavButtons[buttonon].SetActive(true);
    }

    void IfBlank(SlideManager.Slides man)
    {
        quizLayout.SetActive(false);
        blankLayout.SetActive(true);
        textChoices.SetActive(false);
        imageChoices.SetActive(false);
        bothChoices.SetActive(false);

        if (!String.IsNullOrEmpty(man.videoPath[0]))
        {
            Debug.Log("Video here in blank");
            blankTextInput.gameObject.SetActive(false);
            blankAddImageButton.SetActive(true);
            blankDeleteButton.SetActive(true);

            StartCoroutine(MediaLoader.SetupVideo(blankImageInput.transform, man.videoPath[0]));

        }
        else
        {
            if (man.imageSprites[0] != null)
            {
                blankTextInput.gameObject.SetActive(false);
                blankAddImageButton.SetActive(true);
                blankDeleteButton.SetActive(true);

                StartCoroutine(MediaLoader.SetupImage(blankImageInput.transform, man.imageSprites[0]));
            }
            else
            {
                blankTextInput.gameObject.SetActive(true);
                blankDeleteButton.SetActive(false);

                StartCoroutine(MediaLoader.SetupImage(blankImageInput.transform, null));

                blankTextInput.text = man.questionText;

                if (!String.IsNullOrEmpty(man.questionText))
                {
                    Debug.Log("Text here in blank");
                    blankAddImageButton.SetActive(false);
                }
                else
                {
                    blankAddImageButton.SetActive(true);
                }
            }

        }
    }

    public void DeleteBlankMedia()
    {
        slidesList[currentSlide].imageSprites[0] = null;
        slidesList[currentSlide].videoPath[0] = null;

        SlidesData.instance.MediaAnsUpdate(this, currentSlide, 0, null);

        ReloadSlide();
    }
}