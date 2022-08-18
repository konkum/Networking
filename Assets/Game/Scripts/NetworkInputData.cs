using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;

public struct NetworkInputData : INetworkInput
{
    public const byte MOUSEBUTTON0 = 0x01;

    public byte button_0;
    public Vector3 direction;
    public Vector3 camDir;

    public Vector3 firePoint;
    public Vector3 attactDirection;

    //public Camera PlayShootingRay;
}