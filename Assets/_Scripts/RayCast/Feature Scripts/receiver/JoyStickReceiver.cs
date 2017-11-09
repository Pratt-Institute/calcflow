﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NanoVRController;
using VoxelBusters.RuntimeSerialization;

[RuntimeSerializable(typeof(MonoBehaviour), false)]
public class JoyStickReceiver : MonoBehaviour {

    public delegate void JoyStickCallBack(VRController c, ControllerComponentArgs e);

    public event JoyStickCallBack JoyStickTouched;

    public void ReceiverJoystickInput(VRController c, ControllerComponentArgs e)
    {
        if (JoyStickTouched != null)
            JoyStickTouched.Invoke(c, e);
    }
}
