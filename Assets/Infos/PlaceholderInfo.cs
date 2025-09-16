using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName ="Info")]

public class PlaceholderInfo : ScriptableObject
{
    public string title;
    public string details;
    public Sprite popupThumbnail;
    public TMP_FontAsset font;
    public bool isTemplate;
}
