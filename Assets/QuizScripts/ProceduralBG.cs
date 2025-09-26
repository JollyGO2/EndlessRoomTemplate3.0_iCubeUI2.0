using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ProceduralBG : MonoBehaviour
{
    
    public int lineWidth = 8;
    public Color lineColor;
    public List<Color> colors = new List<Color>() { Color.black, Color.white };
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<VerticalLayoutGroup>().spacing = lineWidth;
        foreach(HorizontalLayoutGroup layout in GetComponentsInChildren<HorizontalLayoutGroup>())
        {
            layout.spacing = lineWidth;
            foreach(Outline line in layout.GetComponentsInChildren<Outline>())
            {
                line.effectDistance = new Vector2(lineWidth, -lineWidth);
                line.effectColor = lineColor;
            }
        }
    }

    public void RandomiseBG()
    {
        GetComponent<Image>().color = colors[UnityEngine.Random.Range(0, colors.Count)]; //Change BG color

        foreach (HorizontalLayoutGroup layout in GetComponentsInChildren<HorizontalLayoutGroup>())
        {
            foreach (Image i in layout.GetComponentsInChildren<Image>())
            {
                int r = UnityEngine.Random.Range(0, colors.Count);

                i.color = colors[r];
            }
        }
    }
}
