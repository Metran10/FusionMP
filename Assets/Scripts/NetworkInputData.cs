using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON1 = 0x01;
    public const byte MOUSEBUTTON2 = 0x02;

    public byte buttons;


    public Vector2 movementInput;
    public float rotationInput;

    public Vector3 direction;
    public NetworkBool isJumping;
    public Vector3 lookVector;
    public Vector3 moveVector;
}
