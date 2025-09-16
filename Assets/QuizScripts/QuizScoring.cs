using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuizScoring : MonoBehaviour
{
    public GameObject scoringSlide;
    public GameObject quizSlide;
    public QuizManager quizManager;
    public TMP_Text totalScore;
    public TMP_Text scorePercentage;
    public List<GameObject> scoreSlideObjects;
    public GameObject slideButtonsHolder;
    public List<Button> slideButtons;
    public GameObject slideButtonsPrefab;
    public List<int> wrongQuestions;
    public List<AudioSource> audioPlayers;

    public void Resetting()
    {
        for (int i = 0; i < slideButtonsHolder.transform.childCount; i++)
        {
            Destroy(slideButtonsHolder.transform.GetChild(i).gameObject);
        }

        slideButtons.Clear();

        scoringSlide.SetActive(false);
        quizSlide.SetActive(true);

        foreach (AudioSource players in audioPlayers)
        {
            players.enabled = true;
        }
    }

    public void Work(bool finished)
    {
        scoringSlide.SetActive(true);
        quizSlide.SetActive(false);

        if (!finished)
        {
            float total = 0;
        float wrong = 0;
        foreach (QuizManager.Slide s in quizManager.slidesList)
        {
            Debug.Log("Spawn quiz slide button");
            Button b = Instantiate(slideButtonsPrefab, slideButtonsHolder.transform).GetComponentInChildren<Button>();
            slideButtons.Add(b);
            b.onClick.AddListener(delegate { ToSlide(b); });
            b.transform.parent.GetComponentInChildren<TMP_Text>().text = (quizManager.slidesList.IndexOf(s)+1).ToString();

            if (s.slideType != QuizManager.Slide.SlideType.blank)
            {
                total++;

                int i = quizManager.slidesList.IndexOf(s);
                if (wrongQuestions.Contains(i))
                {
                    b.GetComponentInChildren<Image>().color = Color.red;
                    wrong++;
                }
                else
                {
                    b.GetComponentInChildren<Image>().color = Color.green;
                }
            }
            else
            {

                b.GetComponentInChildren<Image>().color = Color.white;
            }
        }

        totalScore.text = (total-wrong) + " / " + total;
        Debug.Log(((total - wrong) / total) + "Percentage" + wrong + " wrong " +  total.ToString()); ;

            int percentageScore = Mathf.RoundToInt(((total - wrong) / total) * 100f);

            if (total == 0)
            {
                percentageScore = 0;
            }

        scorePercentage.text = percentageScore.ToString()+ "%";


            StartCoroutine(TurnOn());
        }
        else
        {
            foreach (AudioSource players in audioPlayers)
            {
                players.enabled = false;
            }

            foreach (GameObject gameObject in scoreSlideObjects)
            {
                gameObject.SetActive(true);
            }
        }
    }    

    IEnumerator TurnOn()
    {
        foreach(GameObject o in scoreSlideObjects)
        {
            o.SetActive(false);
        }

        yield return new WaitForSeconds(0.2f);
        scoreSlideObjects[0].SetActive(true);

        yield return new WaitForSeconds(2.5f);
        scoreSlideObjects[1].SetActive(true);

        yield return new WaitForSeconds(3);
        scoreSlideObjects[2].SetActive(true);

        yield return new WaitForSeconds(1);
        scoreSlideObjects[3].SetActive(true);

        yield return new WaitForSeconds(1);
        scoreSlideObjects[4].SetActive(true);

    }

    public void ToSlide(Button b)
    {

        quizManager.current = slideButtons.IndexOf(b);
        scoringSlide.SetActive(false);
        quizSlide.SetActive(true);
        quizManager.ReloadQN();

    }
}
