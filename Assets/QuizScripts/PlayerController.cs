using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public Transform playerCamera;
    public bool RMB;
    public bool freeMovement;

    [Header("Free Movement")]
    public Vector2 movementInput;
    public Rigidbody rb;
    public float speed;
    public float groundDrag;
    public float jump;
    public LayerMask groundLayer;
    public float minDistanceToGround;
    //camera
    public Transform player;
    public float mouseSensitivity = 2f;
    float cameraVerticalRotation = 0;


    [Header ("Fixed Movement")]
    public GameObject fixedMovementButtons;
    public GameObject fixedRotationSliders;

    public Slider rotateLeftRight;
    public Slider rotateUpDown;
    public DataAcrossScenes dataPersistence;


    // Start is called before the first frame update
    void OnEnable()
    {
        if (DataAcrossScenes.instance!= null)
        {
            dataPersistence = DataAcrossScenes.instance;
        }
        else
        {
            dataPersistence = FindObjectOfType < DataAcrossScenes>();
        }

        if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Three)
        {
            fixedMovementButtons.SetActive(false);
        }
        else
        {
            fixedMovementButtons.SetActive(true);
        }

        if (!freeMovement)
        {
            rotateLeftRight.value = 0;
            rotateUpDown.value = 0.5f;
        }
    }

    public void SliderSets()
    {
        rotateLeftRight.value = (player.localEulerAngles.y + 90f) / 180f;
        rotateUpDown.value = (player.localEulerAngles.x + 90f) / 180f;
    }

    public void ClickMove(bool left)
    {
        if (left)
        {
            player.localPosition = new Vector3(-6.25f, player.localPosition.y, -0.5f);
        }
        else
        {
            player.localPosition = new Vector3(6.25f, player.localPosition.y, -0.5f);
        }
    }

    // Update is called once per frame
    void Update()
    {
       if (freeMovement)
        {
            FreeMove();
        }

       if (Input.GetKey(KeyCode.Space) && Input.GetKey(KeyCode.LeftShift))
        {
            if (Input.GetKeyDown(KeyCode.W))
            {
                freeMovement = true;

                fixedMovementButtons.SetActive(false);
                fixedRotationSliders.SetActive(false);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                freeMovement = false;
                fixedRotationSliders.SetActive(true);
                SliderSets();
                ClickMove(true);

                if (dataPersistence.wallsConfig == DataAcrossScenes.WallsConfig.Three)
                {
                    fixedMovementButtons.SetActive(false);
                }
                else
                {
                    fixedMovementButtons.SetActive(true);
                }
            }
        }

    }

    public void RotationLR(Slider s)
    {
        player.rotation = Quaternion.Euler(player.localEulerAngles.x, s.value / s.maxValue * 180f -90, player.localEulerAngles.z);

    }

    public void RotationUD(Slider s)
    {
        playerCamera.rotation = Quaternion.Euler(-(s.value / s.maxValue * 180f-90), player.localEulerAngles.y, player.localEulerAngles.z);
    }

    public void FreeMove()
    {
        movementInput.x = Input.GetAxisRaw("Horizontal");
        movementInput.y = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = player.forward * movementInput.y + player.right * movementInput.x;

        rb.AddForce(moveDir * speed * Time.deltaTime * 1000, ForceMode.Force);
        rb.drag = groundDrag;

        if (Input.GetMouseButtonDown(1))
        {
            RMB = true;
        }

        if (Input.GetMouseButtonUp(1))
        {
            RMB = false;
        }

        //jump handling
        if (Physics.Raycast(transform.position, Vector3.down, minDistanceToGround, groundLayer))
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                rb.AddForce(new Vector2(0, jump));
            }
        }
        else
        {
            rb.AddForce(new Vector2(0, -Time.deltaTime * 10000));
        }


        if (RMB)
        {
            Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = true;



            float inputX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float inputY = Input.GetAxis("Mouse Y") * mouseSensitivity;


            //vertical rotation up/down
            cameraVerticalRotation -= inputY;
            cameraVerticalRotation = Mathf.Clamp(cameraVerticalRotation, -90, 90);
            playerCamera.localEulerAngles = Vector3.right * cameraVerticalRotation;

            player.Rotate(Vector3.up * inputX);
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
        }
    }
}
