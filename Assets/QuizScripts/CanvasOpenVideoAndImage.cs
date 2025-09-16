using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using TMPro;
using UnityEngine.Video;
using System.IO;
using System.Web;
using System;

[RequireComponent(typeof(Button))]
public class CanvasOpenVideoAndImage : MonoBehaviour, IPointerDownHandler
{
    public GameObject targetOutput;
    public enum MediaType { Answer, Blank, BG, Thumbnail };
    public MediaType mediaType;

    public SlideManager slideManager;


#if UNITY_WEBGL && !UNITY_EDITOR
    //
    // WebGL
    //
    [DllImport("__Internal")]
    private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

    public void OnPointerDown(PointerEventData eventData) {
        UploadFile(gameObject.name, "OnFileUpload", ".png, .jpg", false);
    }

    // Called from browser
    public void OnFileUpload(string url) {
        StartCoroutine(OutputRoutine(url));
    }
#else
    //
    // Standalone platforms & editor
    //
    public void OnPointerDown(PointerEventData eventData) { }

    void Start()
    {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {


        UndoRedo.instance.Action();

        var extensions = new[] { new ExtensionFilter("Image File", "png", "jpg", "jpeg"), new ExtensionFilter("Video File", "mp4") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", extensions, false);


        if (paths.Length > 0)
        {
            TestPath(paths[0]);
        }

    }
#endif

    public void TestPath(string pathToCheck)
    {
        string checkPath = HttpUtility.UrlDecode(new System.Uri(pathToCheck).AbsoluteUri);


        Debug.Log(checkPath);

        if (FilePathHasInvalidChars(checkPath))
        {
            if (FindObjectOfType<EditorManager>())
            {
                FindObjectOfType<EditorManager>().urlFailWarning.SetActive(true);
            }
            return;
        }

        OutputRoutine(checkPath);
    }

    public static bool FilePathHasInvalidChars(string path)
    {
        bool ret = false;
        if (!string.IsNullOrEmpty(path))
        {
            try
            {
                // Careful!
                //    Path.GetDirectoryName("C:\Directory\SubDirectory")
                //    returns "C:\Directory", which may not be what you want in
                //    this case. You may need to explicitly add a trailing \
                //    if path is a directory and not a file path. As written, 
                //    this function just assumes path is a file path.
                string fileName = System.IO.Path.GetFileName(path);
                string fileDirectory = System.IO.Path.GetDirectoryName(path);

                // we don't need to do anything else,
                // if we got here without throwing an 
                // exception, then the path does not
                // contain invalid characters
            }
            catch (ArgumentException)
            {
                // Path functions will throw this 
                // if path contains invalid chars
                ret = true;
            }
        }
        return ret;
    }



    private void OutputRoutine(string url)
    {


        if (url.Contains("%"))
        {
            Debug.LogError("Unreadable url, " + url);
            if (FindObjectOfType<EditorManager>())
            {
                FindObjectOfType<EditorManager>().urlFailWarning.SetActive(true);
            }
            return;
        }



        string from = url.Substring(8); //removing file:///
        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(from);
        //string fileName = url.Substring(Path.GetDirectoryName(url).Length + 3);
        string fileType = "Image";

        if (Path.GetExtension(url) == ".mp4")
        {
            fileType = "Video";
        }


        string destination = Path.Combine(Application.persistentDataPath, DataAcrossScenes.instance.projectName, "Assets", fileType, fileName);
        Debug.Log(destination);

        //MediaLoader.CreateImage(MediaLoader.LoadSpritePath(from), destination);
        //Debug.Log("Image created in " + destination);


        Debug.Log("Copying from " + from + " to " + destination);
        File.Copy(from, destination);

        if (mediaType == MediaType.Blank)
        {
            slideManager.BlankImage(fileName);
        }
        else if (mediaType == MediaType.Answer)
        {
            Debug.Log("Assigning media to an answer");
            Debug.Log("File name: " + fileName);
            slideManager.IMGANS(fileName, transform.parent.GetSiblingIndex());
        }
        else if (mediaType == MediaType.BG)
        {
            slideManager.PressChangeBG(fileName);
        }
        else if (mediaType == MediaType.Thumbnail)
        {
            FindObjectOfType<Publishing>().thumbnailImage.sprite = MediaLoader.LoadSprite(Path.Combine("Assets", "Image", fileName), true);
        }
    }



}




