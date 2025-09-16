using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerConfigurationManager : MonoBehaviour
{
    public static PlayerConfigurationManager instance;
    
    [Header("Cameras")]
    public Camera leftWallCamera;
    public Camera middleLeftWallCamera;
    public Camera middleRightWallCamera;
    public Camera rightWallCamera;
    public Camera floorCamera;

    [Header("Canvas Objects")]
    public GameObject leftWallCanvas;
    public GameObject middleLeftWallCanvas;
    public GameObject middleRightWallCanvas;
    public GameObject rightWallCanvas;
    public GameObject floorCanvas;

    public enum PlayerConfig
    {
        //_1w0f,
        //_1w1f,
        _2w0f, 
        //_2w1f, 
        //_2w2f,
        _3w0f,
        _3w1f, 
        //_3w2f,
        _4w0f,
        _4w1f, 
        _4w2f
    };

    [Header("Current Configuration")]
    public PlayerConfig playerConfig = PlayerConfig._3w1f;

    [Header("HotKey (Ctrl + Alt + Key)")]
    [SerializeField] KeyCode _2w0f = KeyCode.F1;
    [SerializeField] KeyCode _3w0f = KeyCode.F2;
    [SerializeField] KeyCode _3w1f = KeyCode.F3;
    [SerializeField] KeyCode _4w0f = KeyCode.F4;
    [SerializeField] KeyCode _4w1f = KeyCode.F5;
    [SerializeField] KeyCode _4w2f = KeyCode.F6;

    static readonly int twoDisplayWidth   = 2560;
    static readonly int threeDisplayWidth = 3840;
    static readonly int fourDisplayWidth  = 5120;
    static readonly int fiveDisplayWidth  = 6400;
    static readonly int sixDisplayWidth   = 7680;

    static readonly int displayHeight = 800;
    
    // Start is called before the first frame update
    void Awake()
    {
        if(instance != null) 
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;

        if(leftWallCamera == null) 
        {
            Debug.Log("leftWallCamera not found. Adding.");
            leftWallCamera = GameObject.Find("LeftWallCamera").GetComponent<Camera>();
        }

        if (middleLeftWallCamera == null)
        {
            Debug.Log("middleLeftWallCamera not found. Adding.");
            middleLeftWallCamera = GameObject.Find("MiddleLeftWallCamera").GetComponent<Camera>();
        }

        if (middleRightWallCamera == null)
        {
            Debug.Log("middleRightWallCamera not found. Adding.");
            middleRightWallCamera = GameObject.Find("MiddleRightWallCamera").GetComponent<Camera>();
        }

        if (rightWallCamera == null)
        {
            Debug.Log("rightWallCamera not found. Adding.");
            rightWallCamera = GameObject.Find("RightWallCamera").GetComponent<Camera>();
        }

        if (floorCamera == null)
        {
            Debug.Log("floorCamera not found. Adding.");
            floorCamera = GameObject.Find("FloorCamera").GetComponent<Camera>();
        }

        // Can do a get config here if loading. Or different method if not initializing config on awake.


        int config = PlayerPrefs.GetInt("Config");

        switch (config)
        {
            case 3:
                playerConfig = PlayerConfig._3w1f;
                break;

            case 4:
                playerConfig = PlayerConfig._4w0f;
                break;
        }

        AdjustCamerasToConfig(playerConfig);


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(_2w0f))
            {
                playerConfig = PlayerConfig._2w0f;
            }

            else if (Input.GetKeyDown(_3w0f))
            {
                playerConfig = PlayerConfig._3w0f;
            }

            else if (Input.GetKeyDown(_3w1f))
            {
                playerConfig = PlayerConfig._3w1f;
                PlayerPrefs.SetInt("Config", 3);
            }

            else if (Input.GetKeyDown(_4w0f))
            {
                playerConfig = PlayerConfig._4w0f;
                PlayerPrefs.SetInt("Config", 4);
            }

            else if (Input.GetKeyDown(_4w1f))
            {
                playerConfig = PlayerConfig._4w1f;
            }

            else if (Input.GetKeyDown(_4w2f))
            {
                playerConfig = PlayerConfig._4w2f;
            }

            else if (Input.GetKeyDown(KeyCode.R))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }

            AdjustCamerasToConfig(playerConfig);

            
        }
    }

    public void AdjustCamerasToConfig(PlayerConfig targetConfig)
    {
        switch (targetConfig)
        {
            case PlayerConfig._2w0f:
                // Activate required camera/canvas
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(false); 
                middleRightWallCanvas.SetActive(false);

                rightWallCamera.gameObject.SetActive(false);
                rightWallCanvas.SetActive(false);

                floorCamera.gameObject.SetActive(false);
                floorCanvas.SetActive(false);

                // Set screen resolution
                Screen.SetResolution(twoDisplayWidth, displayHeight, true);

                // Adjust screen ratio
                leftWallCamera.rect = new Rect(0f, 0f, 0.5f, 1f);
                middleLeftWallCamera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
                break;
            case PlayerConfig._3w0f:
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(false);
                middleRightWallCanvas.SetActive(false);

                rightWallCamera.gameObject.SetActive(true);
                rightWallCanvas.SetActive(true);

                floorCamera.gameObject.SetActive(false);
                floorCanvas.SetActive(false);

                Screen.SetResolution(threeDisplayWidth, displayHeight, true);

                leftWallCamera.rect = new Rect(0f, 0f, 0.333f, 1f);
                middleLeftWallCamera.rect = new Rect(0.333f, 0f, 0.334f, 1f);
                rightWallCamera.rect = new Rect(0.667f, 0f, 0.333f, 1f);
                break;
            case PlayerConfig._3w1f:
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(false);
                middleRightWallCanvas.SetActive(false);

                rightWallCamera.gameObject.SetActive(true);
                rightWallCanvas.SetActive(true);

                floorCamera.gameObject.SetActive(true);
                floorCanvas.SetActive(true);

                Screen.SetResolution(fourDisplayWidth, displayHeight, true);

                leftWallCamera.rect = new Rect(0f, 0f, 0.25f, 1f);
                middleLeftWallCamera.rect = new Rect(0.25f, 0f, 0.25f, 1f);
                rightWallCamera.rect = new Rect(0.5f, 0f, 0.25f, 1f);
                floorCamera.rect = new Rect(0.75f, 0f, 0.25f, 1f);
                break;
            case PlayerConfig._4w0f:
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(true);
                middleRightWallCanvas.SetActive(true);

                rightWallCamera.gameObject.SetActive(true);
                rightWallCanvas.SetActive(true);

                floorCamera.gameObject.SetActive(false);
                floorCanvas.SetActive(false);

                Screen.SetResolution(fourDisplayWidth, displayHeight, true);

                leftWallCamera.rect = new Rect(0f, 0f, 0.25f, 1f);
                middleLeftWallCamera.rect = new Rect(0.25f, 0f, 0.25f, 1f);
                middleRightWallCamera.rect = new Rect(0.5f, 0f, 0.25f, 1f);
                rightWallCamera.rect = new Rect(0.75f, 0f, 0.25f, 1f);
                break;
            case PlayerConfig._4w1f:
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(true);
                middleRightWallCanvas.SetActive(true);

                rightWallCamera.gameObject.SetActive(true);
                rightWallCanvas.SetActive(true);

                floorCamera.gameObject.SetActive(true);
                floorCanvas.SetActive(true);

                Screen.SetResolution(fiveDisplayWidth, displayHeight, true);

                leftWallCamera.rect = new Rect(0f, 0f, 0.2f, 1f);
                middleLeftWallCamera.rect = new Rect(0.2f, 0f, 0.2f, 1f);
                middleRightWallCamera.rect = new Rect(0.4f, 0f, 0.2f, 1f);
                rightWallCamera.rect = new Rect(0.6f, 0f, 0.2f, 1f);
                floorCamera.rect = new Rect(0.8f, 0f, 0.2f, 1f);
                break;
            case PlayerConfig._4w2f:
                leftWallCamera.gameObject.SetActive(true);
                leftWallCanvas.SetActive(true);

                middleLeftWallCamera.gameObject.SetActive(true);
                middleLeftWallCanvas.SetActive(true);

                middleRightWallCamera.gameObject.SetActive(true);
                middleRightWallCanvas.SetActive(true);

                rightWallCamera.gameObject.SetActive(true);
                rightWallCanvas.SetActive(true);

                floorCamera.gameObject.SetActive(true);
                floorCanvas.SetActive(true);

                Screen.SetResolution(sixDisplayWidth, displayHeight, true);

                leftWallCamera.rect = new Rect(0f, 0f, 0.1666f, 1f);
                middleLeftWallCamera.rect = new Rect(0.1666f, 0f, 0.1666f, 1f);
                middleRightWallCamera.rect = new Rect(0.3333f, 0f, 0.1666f, 1f);
                rightWallCamera.rect = new Rect(0.5f, 0f, 0.1666f, 1f);
                floorCamera.rect = new Rect(0.6666f, 0f, 0.3334f, 1f);
                break;
            default:
                Debug.Log($"Unknown Configuration {targetConfig}. Skipping adjustments.");
                break;
        }
    }
}
