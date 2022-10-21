using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{

    Vector2 moveInput = Vector3.zero;
    Vector2 viewInput = Vector2.zero;

    bool isJumpingButtonPressed = false;

    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }


    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }


    private void Update()
    {
        if (!player.Object.HasInputAuthority)
            return;

        viewInput.x = Input.GetAxis("Mouse X");
        viewInput.y = Input.GetAxis("Mouse Y") * -1;

        moveInput.x = Input.GetAxis("Horizontal");
        moveInput.y = Input.GetAxis("Vertical");

        if (Input.GetButtonDown("Jump"))
            isJumpingButtonPressed = true;


    }


    public NetworkInputData GetNetworkInput()
    {
        NetworkInputData networkInputData = new NetworkInputData();

        networkInputData.lookVector = viewInput;
        networkInputData.moveVector = moveInput;
        networkInputData.isJumping = isJumpingButtonPressed;



        isJumpingButtonPressed = false;

        return networkInputData;

    }


}
