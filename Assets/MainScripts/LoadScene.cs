using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadScene : MonoBehaviour
{
    public int scenetoload;

    public void Load()
    {
        SceneManager.LoadScene(scenetoload);
    }
}
