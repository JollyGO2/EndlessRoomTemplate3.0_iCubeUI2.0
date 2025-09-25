using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.U2D;
using UnityEngine.UI;
using UnityEngine.Video;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class QuizManager : MonoBehaviour
{

    public List<Slide> slidesList;
    public List<AnswerTracking> answerTracker;

    [System.Serializable]
    public class Slide
    {
        public enum SlideType { text, image, both, blank };
        public SlideType slideType;
        public List<bool> correctAnswers;
        public List<string> textAnswers;
        public List<string> videoAnswersPath;
        public List<Sprite> imageSprites;
        public string questionText;
        public int numberOfAnswers;
        public Sprite bgSprite;
    }

    [System.Serializable]
    public class AnswerTracking
    {
        public bool use;
        public List<bool> options;
    }

    public List<GameObject> navButtons;

    [Header ("Layouts, Choices")]
    public TMP_Text questionInput;
    public GameObject textChoices;
    public GameObject imageChoices;
    public GameObject bothChoices;
    public GameObject quizLayout;

    [Header("Blank Slide items")]
    public GameObject blankLayout;
    public TMP_Text blankText;
    public GameObject blankImage;

    [Header("Current Slide")]
    public int current;

    public bool quizFinished;
    public QuizScoring quizScoring;
    public Sprite defaultSprite;
    public Image bgImage;

    [Header("Addons")]
    public TextMeshProUGUI ScoreText;
    private int score = 0;

    public TextMeshProUGUI TimerText;
    private float timeRemaining = 0;
    private bool startTimer = false;
    public float totalTime = 10;
    public int NumOfRoomsVisited { get; private set; }
    public int correctVisits { get; private set; }

    public ProceduralBG proceduralBG;

    void Update()
    {
        if (startTimer)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime; // subtract time

                TimerText.text = FormatTime(timeRemaining);
            }
            else
            {
                Debug.Log("Timer finished!");
                Scoring();
                StartTimer(false);
            }
        }
    }

    public string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);

        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private void StartTimer(bool start)
    {
        startTimer = start;
        timeRemaining = totalTime;
    }
    public void Enable()
    {

        // setting choices number
        for (int i = 0; i < 8; i++)
        {
            textChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
            imageChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
            bothChoices.transform.GetChild(i).GetChild(0).GetComponent<TMP_Text>().text = (i + 1).ToString() + ".";
        }

        //if only one slide, "next" button is "finish" button
        if (slidesList.Count == 1)
        {
            foreach (GameObject o in navButtons)
            {
                o.SetActive(false);
            }

            //navButtons[3].SetActive(true);
        }

        //adding an answertracker for each question slide
        answerTracker.Clear();
        foreach (Slide s in slidesList)
        {
            answerTracker.Add(new AnswerTracking());
            if (s.slideType != Slide.SlideType.blank)
            {
                answerTracker[answerTracker.Count - 1].use = true;

                //adding number of options to track depending on number of choices
                switch (s.numberOfAnswers)
                {
                    case 2:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false };
                        break;
                    case 3:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false};
                        break;
                    case 4:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false, false};
                        break;
                    case 5:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false, false, false};
                        break;
                }

            }
            else
            {//if blank slide, answertracker for it is just a placeholder
                answerTracker[answerTracker.Count - 1].use = false;
            }
        }

        ScoreText.transform.parent.gameObject.SetActive(true);
        TimerText.transform.parent.gameObject.SetActive(true);
        correctVisits = 0;
        NumOfRoomsVisited = 0;
        totalTime = FindObjectOfType<EditorManager>().totalTime;
        StartTimer(true);
        ReloadQN();
    }

    public void QNNext(GameObject thisSlotCalled)
    {
        if (slidesList.Count == 0)
        {
            return;
        }

        thisSlotCalled.GetComponentInChildren<Toggle>().isOn = true;

        int scoreModifier;
        string t = thisSlotCalled.transform.GetChild(2).GetChild(1).GetComponent<TextMeshProUGUI>().text;

        if (int.TryParse(t, out scoreModifier))
        {
            Debug.Log("Converted to int: " + scoreModifier);
        }
        else
        {
            Debug.LogWarning("Text is not a valid integer: " + t);
            scoreModifier = 1; //Default to 1 if error
        }

        //Check with reference to index
        int index = thisSlotCalled.transform.GetSiblingIndex();

        if (answerTracker[current].options[index] && slidesList[current].correctAnswers[index])
        {
            Debug.Log("Answered correctly");
            correctVisits++;

            if (scoreModifier >= 0)
            {
                score += scoreModifier;
            }
        }
        else if (answerTracker[current].options[index] && !slidesList[current].correctAnswers[index])
        {
            Debug.Log("Answered Wrong");
            if (scoreModifier >= 0)
            {
                score -= scoreModifier;
            }
        }

        NumOfRoomsVisited++;
        UpdateScore(score);

        //Pick Random Room
        thisSlotCalled.GetComponentInChildren<Toggle>().isOn = false;
        PickRandomRoom(current);
        ReloadQN();

        //Old Code
        /*
        if (current < slidesList.Count - 1)
        {
            current++;

            ReloadQN();
        }
        else
        {
            Debug.Log("Finish");
            Scoring();

        }
        */
    }
    private void UpdateScore(int num)
    {
        //Formate Score
        ScoreText.text = FormatLargeNumber(num);
    }
    private string FormatLargeNumber(long num)
    {
        bool isNegative = num < 0;

        num = Math.Abs(num);

        string toReturn = num.ToString();
        if (num >= 1_000_000_000)
            toReturn = (num / 1_000_000_000f).ToString("0.##") + "B";

        else if (num >= 1_000_000)
            toReturn = (num / 1_000_000f).ToString("0.##") + "M";

        else if(num >= 1_000)
            toReturn = (num / 1_000f).ToString("0.##") + "K";

        if (isNegative)
            toReturn = "-" + toReturn;

        return toReturn;
    }

    private void PickRandomRoom(int dontPick)
    {
        List<int> roomlist = new List<int>();
        for (int i = 0; i < slidesList.Count; i++)
        {
            if (i != dontPick)
            {
                roomlist.Add(i);
            }
        }
        current = roomlist[UnityEngine.Random.Range(0, roomlist.Count)];
    }

    public void QNBack()
    {
        if (quizScoring.scoringSlide.activeInHierarchy)
        {
            quizScoring.scoringSlide.SetActive(false);
            quizScoring.quizSlide.SetActive(true);
            current = slidesList.Count - 1;
        }
        else
        {
            current--;
        }

        ReloadQN();
    }

    public void ReloadQN()
    {
        //bgImage.sprite = slidesList[current].bgSprite;
        proceduralBG.RandomiseBG();

        //get type of slide for current slide
        Slide.SlideType t = slidesList[current].slideType;

        //turning off choices and layouts not needed
        GameObject[] allLayouts = new GameObject[] { blankLayout, quizLayout, imageChoices, textChoices, bothChoices };

        foreach (GameObject layouts in allLayouts)
        {
            layouts.SetActive(false);
        }

        if (t != Slide.SlideType.blank)
        {
            quizLayout.SetActive(true);
            questionInput.text = slidesList[current].questionText;

            //setup answer type
            Transform whole = textChoices.transform;

            if (t == Slide.SlideType.image)
            {
                whole = imageChoices.transform;
            }
            else if (t == Slide.SlideType.both)
            {
                whole = bothChoices.transform;
            }

            whole.gameObject.SetActive(true);

            //setup number of choices
            for (int i = 0; i < 8; i++)
            {

                if (i >= slidesList[current].numberOfAnswers)
                {
                    whole.transform.GetChild(i).gameObject.SetActive(false);
                }
                else
                {
                    whole.transform.GetChild(i).gameObject.SetActive(true);
                }
            }

            //setup  choices
            for (int i = 0; i < slidesList[current].numberOfAnswers; i++)
            {

                Transform textans = textChoices.transform;
                Transform mediaans = imageChoices.transform;


                if (slidesList[current].slideType == Slide.SlideType.image)
                {
                    whole = imageChoices.transform;
                }
                else if (slidesList[current].slideType == Slide.SlideType.both)
                {
                    whole = bothChoices.transform;
                    textans = bothChoices.transform;
                    mediaans = bothChoices.transform;

                }
                if (textans.gameObject.activeInHierarchy)
                {
                    textans.GetChild(i).GetChild(2).GetComponentInChildren<TMP_Text>().text = slidesList[current].textAnswers[i];
                }

                if (mediaans.gameObject.activeInHierarchy)
                {
                    Transform mediaAnsHolder = mediaans.GetChild(i).GetChild(2).Find("ShowButton");
                    Transform mediaOutline = mediaans.GetChild(i).GetChild(2).Find("ButtonOutline"); //Addon
                    Transform indicator = mediaans.GetChild(i).GetChild(2).Find("Indicator"); //Addon

                    if (!String.IsNullOrEmpty(slidesList[current].videoAnswersPath[i]))
                    {
                        StartCoroutine(MediaLoader.SetupVideo(mediaAnsHolder, slidesList[current].videoAnswersPath[i]));

                        //StartCoroutine(MediaLoader.SetupVideo(mediaOutline, slidesList[current].videoAnswersPath[i]));
                        //indicator.GetComponent<Image>().sprite = mediaOutline.GetComponent<Image>().sprite;

                    }
                    else
                    {

                        if (slidesList[current].imageSprites[i] != null)
                        {
                            StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, slidesList[current].imageSprites[i]));

                            StartCoroutine(MediaLoader.SetupImage(mediaOutline, slidesList[current].imageSprites[i])); //addon
                            indicator.GetComponent<Image>().sprite = mediaOutline.GetComponent<Image>().sprite;
                        }
                        else
                        {
                            StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, defaultSprite));

                            StartCoroutine(MediaLoader.SetupImage(mediaOutline, defaultSprite));
                            indicator.GetComponent<Image>().sprite = mediaOutline.GetComponent<Image>().sprite;


                            //whole.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic = mediaAnsHolder.GetComponent<Image>();
                        }

                    }

                }

                //mediaans.GetChild(i).GetChild(2).Find("ShowButton").gameObject.SetActive(false);

                if (quizFinished)
                {
                    whole.GetChild(i).GetComponentInChildren<Toggle>().interactable = false;

                }
            }




            if (quizFinished) // if finished quiz already, going back/reloading questions will have visuals/non interactable
            {
                for (int i = 0; i < slidesList[current].correctAnswers.Count; i++)
                {
                    if (slidesList[current].correctAnswers[i] == true)
                    {
                        whole.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = new Color(0.5f, 1f, 0.5f);

                        whole.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(0, 1f, 0, 0.75f);
                    }
                    else
                    {
                        if (answerTracker[current].options[i] == true)
                        {

                            whole.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.25f, 0.25f);
                            whole.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = new Color(1f, 0.25f, 0.25f);
                        }
                        else
                        {
                            whole.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);
                        }
                    }
                }
            }


            //showing previously answered/saved answers
            
            for (int n = 0; n < answerTracker[current].options.Count; n++)
            {
                whole.transform.GetChild(n).GetComponentInChildren<Toggle>().isOn = answerTracker[current].options[n];
            }
            
            //checking if previous answers maxed out correct answers
            int correct = 0;
            int correctcheck = 0;

            for (int i = 0; i < answerTracker[current].options.Count; i++)
            {
                if (answerTracker[current].options[i] == true)
                {
                    correct++;
                }

                if (slidesList[current].correctAnswers[i] == true)
                {
                    correctcheck++;
                }
            }

            if (!quizFinished)
            {
                if (correct >= correctcheck)
                {
                    foreach (Toggle g in whole.GetComponentsInChildren<Toggle>())
                    {
                        //disable and turn off usused answers if limit is reached
                        if (!g.isOn)
                        {
                            g.interactable = false;
                            g.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);

                        }
                    }
                }
                else
                {
                    foreach (Toggle g in whole.GetComponentsInChildren<Toggle>())
                    {
                        if (!g.isOn)
                        {
                            //g.interactable = true;
                            g.targetGraphic.color = Color.white;
                        }
                    }
                }
            }
        }
        else
        {
            //if blank slide, turn on blank layout and turn off quiz layouts
            blankLayout.SetActive(true);


            if (!String.IsNullOrEmpty(slidesList[current].videoAnswersPath[0]))
            {
                blankText.gameObject.SetActive(false);
                StartCoroutine(MediaLoader.SetupVideo(blankImage.transform, slidesList[current].videoAnswersPath[0]));
            }
            else
            {

                if (slidesList[current].imageSprites[0] != null)
                {
                    blankText.gameObject.SetActive(false);
                    StartCoroutine(MediaLoader.SetupImage(blankImage.transform, slidesList[current].imageSprites[0]));
                }
                else
                {
                    StartCoroutine(MediaLoader.SetupImage(blankImage.transform, defaultSprite));

                    blankText.gameObject.SetActive(true);
                    blankText.text = slidesList[current].questionText;
                }

            }

        }

        //setting up navigation buttons
        /*
        if (quizFinished)
        {
            //ScoreText.transform.parent.gameObject.SetActive(false);
            //TimerText.transform.parent.gameObject.SetActive(false);

            //getting which top button(s) to use
            foreach (GameObject o in navButtons)
            {
                o.SetActive(false);
            }

            //if only 1 slide, finish button
            if (slidesList.Count == 1)
            {
                navButtons[3].SetActive(true);
            }
            else
            {
                if (current == 0)
                {
                    //if more than one slide and this is first, next button
                    navButtons[0].SetActive(true);
                }
                else if (current == slidesList.Count - 1)
                {
                    //if more than one slide and this is last, finish and back button
                    navButtons[2].SetActive(true);
                }
                else
                {
                    //if more than one slide and this is not last, next and back button
                    navButtons[1].SetActive(true);
                }
            }
        }
        */
    }

    public void Answer(Toggle b)
    {
        if (answerTracker.Count <= 0)
        {
            return;
        }

        int check = 0;

        GameObject o = textChoices;

        if (slidesList[current].slideType == Slide.SlideType.image)
        {
            o = imageChoices;
        }
        else if (slidesList[current].slideType == Slide.SlideType.both)
        {
            o = bothChoices;
        }

        foreach (Toggle but in o.GetComponentsInChildren<Toggle>())
        {
            if (but == b)
            {
                check = Array.IndexOf(o.GetComponentsInChildren<Toggle>(), b);
            }
        }

        if (slidesList[current].correctAnswers[check]) //Correct answer
        {
            Debug.Log("Correct ans");
        }

        answerTracker[current].options[check] = b.isOn;

        int correct = 0;
        int correctcheck = 0;

        for (int i = 0; i < answerTracker[current].options.Count; i++)
        {
            if (answerTracker[current].options[i] == true)
            {
                correct++;
            }

            if (slidesList[current].correctAnswers[i] == true)
            {
                correctcheck++;
            }
        }

        if (quizFinished)
        {
            return;
        }

        if (correct >= correctcheck)
        {
            foreach (Toggle t in o.GetComponentsInChildren<Toggle>())
            {

                if (!t.isOn)
                {
                    //t.interactable = false;

                    t.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);

                    //Hide button
                    OnOffSlotToggle(false, t);
                    if (t == b)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }
                }
                else
                {
                    if(t != b)
                    {
                        t.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);

                        t.SetIsOnWithoutNotify(false);
                        answerTracker[current].options[t.transform.parent.GetSiblingIndex()] = false;

                        OnOffSlotToggle(false, t);
                    }
                    else 
                    {
                        t.targetGraphic.color = Color.white;

                        //Show Button
                        OnOffSlotToggle(true, t);
                    }
                }
            }
        }
        else
        {
            foreach (Toggle t in o.GetComponentsInChildren<Toggle>())
            {
                if (!t.isOn)
                {
                    //t.interactable = true;

                    t.targetGraphic.color = Color.white;

                    OnOffSlotToggle(false, t);

                    if (t == b)
                    {
                        EventSystem.current.SetSelectedGameObject(null);
                    }
                }
                else
                {
                    t.targetGraphic.color = Color.white;

                    //Show Button
                    OnOffSlotToggle(true, t);

                }
            }
        }
    }
    private void OnOffSlotToggle(bool x, Toggle b)
    {
        //b.transform.Find("ShowButton").gameObject.SetActive(x);
        //b.gameObject.GetComponent<ButtonPressHold>().enabled = x;
        b.transform.Find("Enter Text").gameObject.SetActive(x); //Hide Hold to Enter text
    }

    public void Scoring()
    {
        List<int> wrongqn = new List<int>();

        foreach (Slide s in slidesList)
        {
            if (s.slideType == Slide.SlideType.blank)
            {
                continue;
            }

            int wrong = 0;

            for (int i = 0; i < s.correctAnswers.Count; i++)
            {
                if (s.correctAnswers[i] != answerTracker[slidesList.IndexOf(s)].options[i])
                {
                    wrong++;
                    wrongqn.Add(slidesList.IndexOf(s));
                }


            }
        }

        quizScoring.wrongQuestions = wrongqn;
        quizScoring.Work(quizFinished);

        if (!quizFinished)
        {
            quizFinished = true;
        }
    }

    public void Resetting()
    {
        score = 0; //reset score
        UpdateScore(score);

        StartTimer(false);

        correctVisits = 0;
        NumOfRoomsVisited = 0;

        quizFinished = false;
        //slides.Clear();
        answerTracker.Clear();
        current = 0;
        for (int i = 0; i < 8; i++)
        {
            textChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
            //textchoices.transform.GetChild(i).GetChild(2).GetComponentInChildren<TMP_Text>().text = "";
            textChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.7882354f, 0f);
            textChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = Color.white;
            //textChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;


            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.7882354f, 0f);
            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = Color.white;
            //imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;
            //imagetextschoices.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = null;



            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.7882354f, 0f);
            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = Color.white;
            //bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;
            //imagetextschoices.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = null;

            bothChoices.transform.GetChild(i).GetChild(2).GetComponentInChildren<TMP_Text>().text = "";
        }

        quizScoring.Resetting();

        if (gameObject.activeInHierarchy)
        {
            Enable();
        }
    }


}
