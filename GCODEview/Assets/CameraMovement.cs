using UnityEngine;
using UnityEngine.UI;
using System.Collections;


public class CameraMovement : MonoBehaviour


{
    [SerializeField]
    private float lookSpeedH = 2f;

    [SerializeField]
    private float lookSpeedV = 2f;

    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float dragSpeed = 10.0f;

    private float yaw = 0f;
    private float pitch = 0f;

    Transform startposition;

    private void Start()
    {
        // Button homebutton = Button.Find("button");
        startposition = this.transform;
        // Initialize the correct initial rotation
        this.yaw = this.transform.eulerAngles.y;
        this.pitch = this.transform.eulerAngles.x;

        // Button btn = homebutton.GetComponent<Button>();
        // btn.onClick.AddListener(TaskOnClick);
    }

    private void Update()
    {
        // Only work with the Left Alt pressed
        //  if (Input.GetKey(KeyCode.LeftAlt))
        //  {
        //Look around with Left Mouse
        // if (Input.GetMouseButton(0))
        // {
        //     this.yaw += this.lookSpeedH * Input.GetAxis("Mouse X");
        //     this.pitch -= this.lookSpeedV * Input.GetAxis("Mouse Y");

        //     this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);
        // }

        //drag camera around with Middle Mouse
        if (Input.GetMouseButton(2))
        {
            transform.Translate(-Input.GetAxisRaw("Mouse X") * 1000 * Time.deltaTime * dragSpeed, -Input.GetAxisRaw("Mouse Y") * 1000 * Time.deltaTime * dragSpeed, 0);
        }

        if (Input.GetMouseButton(1))
        {
            //Zoom in and out with Right Mouse
            //  this.transform.Translate(0, 0, Input.GetAxisRaw("Mouse X") * 1000 * this.zoomSpeed * .07f, Space.Self);
            this.yaw += this.lookSpeedH * Input.GetAxis("Mouse X");
            this.pitch -= this.lookSpeedV * Input.GetAxis("Mouse Y");

            this.transform.eulerAngles = new Vector3(this.pitch, this.yaw, 0f);
        }

        //Zoom in and out with Mouse Wheel
        this.transform.Translate(0, 0, Input.GetAxis("Mouse ScrollWheel") * 100 * this.zoomSpeed, Space.Self);
    }
    void TaskOnClick(){
		Debug.Log ("You have clicked the button!");
	}
    //  }
}
