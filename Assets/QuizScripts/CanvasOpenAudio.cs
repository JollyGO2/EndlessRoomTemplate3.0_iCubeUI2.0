using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;
using TMPro;
using System.IO;
using System;
using System.Web;

[RequireComponent(typeof(Button))]
public class CanvasOpenAudio : MonoBehaviour, IPointerDownHandler {

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

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var extensions = new[] { new ExtensionFilter("Audio File", "mp3", "ogg", "wav") };
        var paths = StandaloneFileBrowser.OpenFilePanel("Title", "", extensions, false);


        if (paths.Length > 0) {


            string checkPath = HttpUtility.UrlDecode(new System.Uri(paths[0]).AbsoluteUri);



            if (FilePathHasInvalidChars(checkPath))
            {
                if (FindObjectOfType<EditorManager>())
                {
                    FindObjectOfType<EditorManager>().urlFailWarning.SetActive(true);
                }
                return;
            }

            UndoRedo.instance.Action();
            StartCoroutine(FindObjectOfType<MediaLoader>().LoadAudio(checkPath));
        }
    }
#endif
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


}