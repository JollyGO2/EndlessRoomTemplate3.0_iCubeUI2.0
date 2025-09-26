using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using TouchScript.Examples.Colors;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEngine.GraphicsBuffer;

public class ScoringVis : MonoBehaviour
{
    public ProceduralBG proceduralBG;
    // Start is called before the first frame update

    public List<GameObject> exclude = new List<GameObject>();

    private List<GameObject> sq = new List<GameObject>();

    public Color correctColor = Color.green;
    public Color wrongColor = Color.red;
    private Color originalColor;

    public RectTransform QuizWall;

    private Vector2 startPos;
    private bool doOnce = false;
    private float scaleFactor;

    private void Start()
    {
        originalColor = GetComponent<Image>().color;
    }
    private void BeforeStart()
    {
        foreach (HorizontalLayoutGroup layout in proceduralBG.GetComponentsInChildren<HorizontalLayoutGroup>())
        {
            foreach (Image obj in layout.GetComponentsInChildren<Image>())
            {
                sq.Add(obj.gameObject);
            }
        }

        foreach (GameObject ex in exclude)
        {
            if (sq.Contains(ex))
            {
                sq.Remove(ex);
            }
        }

    }

    public void ResetPlacement()
    {
        if (!doOnce)
        {
            BeforeStart();
            doOnce = true;
        }
        ProcessPlacement();
    }

    private void ProcessPlacement()
    {
        //Pick a random Square
        RectTransform rectTransform = GetComponent<RectTransform>();

        int ChosenSq = UnityEngine.Random.Range(0, sq.Count);
        RectTransform sqRectTransform = sq[ChosenSq].GetComponent<RectTransform>();


        // Match size
        rectTransform.sizeDelta = sqRectTransform.rect.size;

        StartCoroutine(FindPos(sqRectTransform));
    }
    IEnumerator FindPos(RectTransform sqRectTransform)
    {
        yield return null;  // wait one frame

        transform.position = sqRectTransform.position;
        startPos = transform.GetComponent<RectTransform>().localPosition;

        //Find Size
        Vector2 canvasSize = transform.parent.GetComponent<RectTransform>().rect.size;
        Vector2 initialSize = transform.GetComponent<RectTransform>().rect.size;

        float scaleX = canvasSize.x / initialSize.x;
        float scaleY = canvasSize.y / initialSize.y;

        if(scaleX > scaleY)
        {
            scaleFactor = scaleX;
        }
        else
        {
            scaleFactor = scaleY;
        }

        Debug.Log("ScaleFactor: " + scaleFactor);
    }

    public void Score(float s)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();

        rectTransform.localPosition = Vector2.Lerp(startPos, Vector2.zero, s); //Move object 
        if (s > 0)
        {
            transform.localScale = Vector2.Lerp(Vector2.one, new Vector2(scaleFactor, scaleFactor), s);
        }
        else
        {
            transform.localScale = Vector2.Lerp(Vector2.one, Vector2.zero, Mathf.Abs(s));
        }
    }

    public void ColorIndiciator(bool isWrong)
    {
        GetComponent<Image>().color = isWrong ? wrongColor : correctColor;
    }

    public void ResetColor()
    {
        GetComponent<Image>().color = originalColor;
    }
}
