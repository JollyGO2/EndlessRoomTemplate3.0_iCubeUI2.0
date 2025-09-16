using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.Video;
using System.IO;

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

            navButtons[3].SetActive(true);
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
                    case 4:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false, false };
                        break;
                    case 6:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false, false, false, false };
                        break;
                    case 8:
                        answerTracker[answerTracker.Count - 1].options = new List<bool> { false, false, false, false, false, false, false, false };
                        break;
                }

            }
            else
            {//if blank slide, answertracker for it is just a placeholder
                answerTracker[answerTracker.Count - 1].use = false;
            }
        }
        ReloadQN();
    }

    public void QNNext()
    {
        if (slidesList.Count == 0)
        {
            return;
        }

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
        bgImage.sprite = slidesList[current].bgSprite;


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
                    Transform mediaAnsHolder = mediaans.GetChild(i).GetChild(2);

                    if (!String.IsNullOrEmpty(slidesList[current].videoAnswersPath[i]))
                    {
                        StartCoroutine(MediaLoader.SetupVideo(mediaAnsHolder, slidesList[current].videoAnswersPath[i]));

                     }
                    else
                    {

                        if (slidesList[current].imageSprites[i] != null)
                        {
                            StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, slidesList[current].imageSprites[i]));
                        }
                        else
                        {
                            StartCoroutine(MediaLoader.SetupImage(mediaAnsHolder, defaultSprite));

                            whole.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic = mediaAnsHolder.GetComponent<Image>();
                        }

                    }
                }

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
                            g.interactable = true;
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

    public void Answer(Toggle b)
    {
        if (answerTracker.Count <= 0)
        {
            return;
        }

        int correct = 0;
        int correctcheck = 0;

        //if (b.isOn)
        //{



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

        //    if (correct >= correctcheck)
        //    {
        //        b.isOn = false;
        //        return;
        //    }
        //}




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

        if (slidesList[current].correctAnswers[check])
        {
            Debug.Log("Correct ans");
        }

        answerTracker[current].options[check] = b.isOn;

        correct = 0;
        correctcheck = 0;

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
                    t.interactable = false;

                    t.targetGraphic.color = new Color(0.75f, 0.75f, 0.75f);

                }
            }
        }
        else
        {
            foreach (Toggle t in o.GetComponentsInChildren<Toggle>())
            {
                if (!t.isOn)
                {
                    t.interactable = true;

                    t.targetGraphic.color = Color.white;

                }
            }
        }
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
            textChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;


            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.7882354f, 0f);
            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = Color.white;
            imageChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;
            //imagetextschoices.transform.GetChild(i).GetChild(2).GetComponent<Image>().sprite = null;



            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().isOn = false;
            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().graphic.color = new Color(1f, 0.7882354f, 0f);
            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().targetGraphic.color = Color.white;
            bothChoices.transform.GetChild(i).GetComponentInChildren<Toggle>().interactable = true;
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
