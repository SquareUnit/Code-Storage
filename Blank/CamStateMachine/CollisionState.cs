/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : LowPassFilter slider now updating when valus is changed
/// https://github.com/SquareUnit/Code-Storage

/// Maths need a serious rework. They function properly but are fragile.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamCollision : IStates
{
    private PlayerCamera user;
    private float hitPointDist;
    private Vector3 collSV;
    private float SmoothMaxSpeed = 25.0f;

    public PlayerCamCollision(PlayerCamera user)
    {
        this.user = user;
    }

    public void Enter()
    {
        if (user.stateDebugLog) Debug.Log("sCamWallColl <color=yellow>Enter</color>");
        collSV = Vector3.zero;
    }

    public void IfStateChange()
    {
        if (!user.isColliding)
        {
            if (user.stateDebugLog) Debug.Log("From sColl to sDefault <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.defaultState);
        }
        else if (InputsManager.instance.camButton)
        {
            if (user.stateDebugLog) Debug.Log("From sColl to sReset <color=purple>StateChange</color>");
            user.camFSM.ChangeState(user.resetState);
        }
    }

    public void StateUpdate()
    {
        if (user.stateDebugLog) Debug.Log("sCamWallColl <color=blue>Update</color>");

        Vector3 dirToTarget = user.camTarget.tr.position - user.hit.point;
        dirToTarget.y = 0.0f;
        float hitAngle = 90 - Vector3.Angle(user.hit.normal, dirToTarget);
        float hyp = (user.desiredCollOffset - 0.01f) / Mathf.Sin(Mathf.Deg2Rad * hitAngle); // Must be smaller than the camera desired collision offset because of float imprecision.
        hitPointDist = Vector3.Distance(user.hit.point, user.camTarget.tr.position);

        user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * (hitPointDist - hyp), ref collSV, 0.032f, SmoothMaxSpeed);
    }

    public void Exit()
    {
        if (user.stateDebugLog) Debug.Log("sCamWallColl <color=yellow>Exit</color>");
    }
}
