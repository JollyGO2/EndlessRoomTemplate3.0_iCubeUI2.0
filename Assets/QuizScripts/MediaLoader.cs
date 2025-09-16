using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class MediaLoader : MonoBehaviour
{

    public static MediaLoader instance;

    private void Start()
    {
        instance = this;
    }
    public  void LoadVideo(VideoPlayer videoOutput, string fileName)
    {
        StartCoroutine(SetupVideo(videoOutput.transform, fileName));

        Debug.Log("Video assigned to " + videoOutput.name);
    }


    public static void CreateImage(Sprite sprite, string destinationPath)
    {
            Sprite itemBGSprite = sprite;
            Texture2D itemBGTex = itemBGSprite.texture;
            byte[] itemBGBytes = itemBGTex.EncodeToPNG();

            Debug.Log(destinationPath);

            File.WriteAllBytes(destinationPath, itemBGBytes);
        
    }

    public static Sprite LoadSprite(string fileName, bool isThumbnail)
    {
        if (String.IsNullOrEmpty(fileName))
        {
            return null;
        }

        string path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Image", fileName);

        if (isThumbnail)
        {
            path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, fileName);
        }
        

        Texture2D texture = ES3.LoadImage(@path);
        texture.SetPixels(texture.GetPixels());
        texture.Apply();
        Sprite imageSprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Debug.Log("Image sprite created " + fileName);

        return (imageSprite);

    }


    public IEnumerator LoadAudio(string url)
    {
        var loader = new WWW(url);
        yield return loader;

        string dir = Path.GetDirectoryName(url);
        string clipName = url.Substring(dir.Length + 1);
        if (url.Contains("file:///"))
        {
            clipName = url.Substring(dir.Length + 3);
        }
        


        Debug.Log("Directory: " + dir + " ||| clipName: " + clipName);


        string copyTo = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Audio", clipName);
        string copyFrom = url;

        if (url.Contains("file:///"))
        {
            copyFrom = url.Substring(8);
        }

        Debug.Log("Copy from " + copyFrom + ", Copy to " + copyTo);

        if (!File.Exists(copyTo))
        {
            File.Copy(copyFrom, copyTo);
        }

        AudioClip clip = loader.GetAudioClip();
        clip.name = clipName;
        FindObjectOfType<EditorManager>().AssignBGM(clip, clipName);
    }

    public static IEnumerator SetupVideo(Transform transform, string fileName)
    {
        Debug.Log("Start video setup");
        
            if (transform.TryGetComponent(out Image image))
            {
            Debug.Log(transform.parent.name + " has Image, need RawImage");
                Destroy(image);

                if (transform.TryGetComponent(out VideoPlayer vidPlayer))
                {
                    vidPlayer.enabled = true;
                }
                else
                {
                    transform.gameObject.AddComponent<VideoPlayer>();
                }

                yield return new WaitForEndOfFrame();

                transform.gameObject.AddComponent<RawImage>();

                var videoTexture = new RenderTexture(Mathf.RoundToInt(transform.GetComponent<RectTransform>().rect.width), Mathf.RoundToInt(transform.GetComponent<RectTransform>().rect.height), 16);


                transform.GetComponent<VideoPlayer>().isLooping = true;
                transform.GetComponent<VideoPlayer>().targetTexture = videoTexture;
                transform.GetComponent<RawImage>().texture = videoTexture;


                if (transform.TryGetComponent(out Button thisButton))
                {
                    thisButton.targetGraphic = transform.GetComponent<RawImage>();
                }

                if (transform.parent.TryGetComponent(out Toggle thisToggle))
                {
                    thisToggle.targetGraphic = transform.GetComponent<RawImage>();
                }
            }

        //Debug.Log("VideoPlayer put on " + transform.parent.name);
        string path = null;

        if (FindObjectOfType<PlayerLoadData>())
        {
            path = Path.Combine(FindObjectOfType<PlayerLoadData>().folder, "Assets", "Video", fileName);
        }
        else
        {
            path = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", "Video", fileName);
        }

        Debug.Log("video path: " + @path);
        transform.GetComponent<VideoPlayer>().url = @path;



        Debug.Log("End video setup");
    }

    public static IEnumerator SetupImage(Transform output, Sprite sprite)
    {
        //Debug.Log("Start image setup for " + output.parent.name);
        if (output == null)
        {
            Debug.LogError("No output assigned to setup image");
            yield break;
        }

        if (sprite == null)
        {
            Debug.LogError("No sprite assigned to setup image");
            yield break;

        }

            if (output.TryGetComponent(out RawImage rawImage))
            {
                Destroy(rawImage);
                
                if (output.TryGetComponent(out VideoPlayer vidPlayer))
                {
                    vidPlayer.enabled = false;
                }

                yield return new WaitForEndOfFrame();

            output.gameObject.AddComponent<Image>();
            }

        //Debug.Log("Sprite assigned for " + output.parent.name);
        output.GetComponent<Image>().sprite = sprite;
        output.GetComponent<Image>().type = Image.Type.Sliced;
        if (output.TryGetComponent(out Button but))
        {
            but.targetGraphic = output.GetComponent<Image>();
        }

        //Debug.Log("End image setup for " + output.parent.name);
    }
}
