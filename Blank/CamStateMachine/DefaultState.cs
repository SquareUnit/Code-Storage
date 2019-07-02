/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// Will need to be splitted into two states, one for movements and one for immobility

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamDefault : IStates
{
    private PlayerCamera user;
    private Vector3 dollySV;
    private float camPosSpeed;
    private float raisePitchValue;
    private float lowerPitchValue;

    public PlayerCamDefault(PlayerCamera user)
    {
        this.user = user;
    }

    public void Enter()
    {
        if (user.stateDebugLog) Debug.Log("sCamDefault <color=yellow>Enter</color>");
        lowerPitchValue = 13f;
        dollySV = Vector3.zero;
    }

    public void IfStateChange()
    {
        if (InputsManager.instance.camButton)
        {
            if (user.stateDebugLog) Debug.Log("From sDefault to sReset <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.resetState);
        }
        else if (user.isColliding)
        {
            if (user.stateDebugLog) Debug.Log("From sDefault to sColl <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.collisionState);
        }
        else if (GameManager.instance.currentAvatar.velocityY < 0)
        {
            if (user.stateDebugLog) Debug.Log("From sDefault to sFall <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.fallingState);
        }
    }

    public void StateUpdate()
    {
        if (user.stateDebugLog) Debug.Log("sCamDefault <color=blue>Update</color>");

        LowerPitchAfterFalling();
        RaisePitchWhileMoving();

        camPosSpeed = user.camPosSpeed;
        user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * user.desiredDollyDst, ref dollySV, camPosSpeed);
    }

    public void Exit()
    {
        if (user.stateDebugLog) Debug.Log("sCamDefault <color=yellow>Exit</color>");
    }

    /// <summary> Slowly lower the camera pitch after falling downward and while in movement
    /// Counter balance a function in the fall state that raise the pitch upward</summary>
    private void LowerPitchAfterFalling()
    {
        if (InputsManager.instance.leftStick != new Vector3(0, 0, 0) && InputsManager.instance.rightStick == new Vector3(0, 0, 0))
        {
            if (user.camFSM.previousState == user.fallingState && user.pitch > 25.0f && lowerPitchValue > 0)
            {
                lowerPitchValue -= 0.06f;
                user.pitch -= lowerPitchValue * Time.deltaTime;
            }
        }
    }

    /// <summary> Raise the camera pitch when moving while the camera close to the avatar and looking upward.
    /// This happend when the player lower the camera to look up/around and start moving again</summary>
    private void RaisePitchWhileMoving()
    {
        if (user.pitch < 15 && InputsManager.instance.rightStick.y > 0)
        {
            raisePitchValue += 90.0f * Time.deltaTime;
            if (raisePitchValue >= 800) raisePitchValue = 800;
        }

        if (InputsManager.instance.leftStick != new Vector3(0, 0, 0) && InputsManager.instance.rightStick == new Vector3(0, 0, 0))
        {
            if (user.pitch < 15)
            {
                raisePitchValue *= 0.962f;
                user.pitch += raisePitchValue * Time.deltaTime;
            }
        }
    }
}
