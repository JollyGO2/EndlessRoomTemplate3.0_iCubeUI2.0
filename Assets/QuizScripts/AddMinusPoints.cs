using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
using Button = UnityEngine.UI.Button;

public class AddMinusPoints : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] Button plusButton;
    [SerializeField] Button minusButton;


    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LateStart());
    }

    IEnumerator LateStart()
    {
        yield return new WaitForSeconds(1f);
        int currentScore = GetPoints();
        ChangeButtonInteractions(currentScore);
        yield return new WaitForSeconds(1f);
    }

    public void AddPoints()
    {
        int currentScore = GetPoints();
        currentScore++;
        if (currentScore < 10) //Max 9
        {
            inputField.text = currentScore.ToString();
            inputField.onEndEdit.Invoke(inputField.text);

            ChangeButtonInteractions(currentScore);
        }
    }

    public void MinusPoints()
    {
        int currentScore = GetPoints();
        currentScore--;
        if (currentScore > 0) //min 1
        {
            inputField.text = currentScore.ToString();
            inputField.onEndEdit.Invoke(inputField.text);

            ChangeButtonInteractions(currentScore);
        }
    }

    private int GetPoints()
    {
        int pScore;
        string t = inputField.text;

        if (int.TryParse(t, out pScore))
        {
            Debug.Log("Converted to int: " + pScore);
        }
        else
        {
            Debug.LogWarning("Text is not a valid integer: " + t);
        }

        return pScore;
    }

    private void ChangeButtonInteractions(int currentScore)
    {
        if(currentScore == 9)
        {
            plusButton.interactable = false;
            Debug.LogWarning("PLUS score is : " + currentScore);
        }
        else if (currentScore == 1 )
        {
            minusButton.interactable = false;
            Debug.LogWarning("Minus score is : " + currentScore);
        }
        else
        {
            plusButton.interactable = true;
            minusButton.interactable = true;
        }
    }
}
