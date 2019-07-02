/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// States trigger when detecting that character controler is falling too.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamFall : IStates
{
    private PlayerCamera user;
    private RaycastHit hit;
    private Vector3 dollySV;

    public PlayerCamFall(PlayerCamera user)
    {
        this.user = user;
    }

    public void Enter()
    {
        if (user.stateDebugLog) Debug.Log("sCamFall <color=yellow>Enter</color>");
        dollySV = Vector3.zero;
    }

    public void IfStateChange()
    {
        if (user.isColliding)
        {
            if (user.stateDebugLog) Debug.Log("From sFall to sColl <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.collisionState);
        }
        else if (user.avatarFSM.currentState != user.avatarFSM.fall)
        {
            if (user.stateDebugLog) Debug.Log("From sFall to sDefault <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.defaultState);
        }
    }

    public void StateUpdate()
    {
        if (user.stateDebugLog) Debug.Log("sCamFall <color=blue>Update</color>");

        RaisePitchIfObstacle();

        user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * user.desiredDollyDst, ref dollySV, 0.025f);
    }

    public void Exit()
    {
        if (user.stateDebugLog) Debug.Log("sCamFall <color=yellow>Exit</color>");
    }

    /// <summary> While falling, check if something directly below might obstrude the camera. If so, attempt to move the cam pitch up preventively. 
    /// This is all made to try to avoid a transition to the collision state while falling, if & when possible</summary>
    private void RaisePitchIfObstacle()
    {
        Vector3 camDownStart = user.tr.position - (1.6f * user.tr.up);
        Vector3 camDownEnd = user.camTarget.tr.position - user.tr.position;
        
        if (Physics.Raycast(camDownStart, camDownEnd, out hit, user.desiredDollyDst, user.obstaclesMask) && hit.collider.tag != "AllowCameraDissolve")
        {
            Debug.DrawRay(camDownStart, camDownEnd, Color.red);
            user.pitch += 200f * Time.deltaTime;
        } 
        else Debug.DrawRay(camDownStart, camDownEnd, Color.green);
    }
}



