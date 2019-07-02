/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// Reset the camera position behind the player.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamReset : IStates
{
    private PlayerCamera user;
    private Vector3 resetSV;
    public float timer;
    private float timerDefault = 0.65f;
    private float valueToDesiredPitch;
    private float valueToDesiredYaw;

    public PlayerCamReset(PlayerCamera user)
    {
        this.user = user;
    }

    public void Enter()
    {
        if (user.stateDebugLog) Debug.Log("sCamReset <color=yellow>Enter<color>");
        timer = timerDefault;
        InputsManager.instance.cameraInputsAreDisabled = true;
    }

    public void IfStateChange()
    {

    }

    public void StateUpdate()
    {
        if (user.stateDebugLog) Debug.Log("sCamReset <color=blue>Update</color>");
        timer -= 1.0f * Time.deltaTime;
        if (timer <= 0)
        {
            if (user.stateDebugLog) Debug.Log("From sReset to sDefault <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.defaultState);
        }

        user.pitch = Mathf.Lerp(user.pitch, 15.0f, 0.10f);
        user.yaw = Mathf.LerpAngle(user.yaw, user.camTarget.tr.rotation.eulerAngles.y, 0.10f);
        user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * user.desiredDollyDst, ref resetSV, 0.025f);
    }

    public void Exit()
    {
        InputsManager.instance.cameraInputsAreDisabled = false;
        if (user.stateDebugLog) Debug.Log("sCamReset <color=yellow>Exit<color>");
    }
}
