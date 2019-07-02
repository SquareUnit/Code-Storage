/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// To be simplified and allow linked chaining of multiple cam state infos. Eventually change the name for cinematic.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamReveal : IStates
{
    private PlayerCamera user;
    private Vector3 resetSV;
    public Vector3 target;

    public PlayerCamRevealInfo info;

    private float lastPitch;
    private float lastYaw;

    private float tStamp;
    public float t;
    public float t1;
    public float t2;
    public float t3;

    public bool revealStartDone;
    public bool revealPauseDone;

    public PlayerCamReveal(PlayerCamera user)
    {
        this.user = user;
    }

    public void Enter()
    {
        if (user.stateDebugLog) Debug.Log("sCamReveal <color=yellow> Enter </color>");
        InputsManager.instance.gameplayInputsAreDisabled = true;
        user.pitchMinMax = new Vector2(-89.0f, 89.0f);
        // Print initial pitch and yaw
        lastPitch = user.pitch;
        lastYaw = user.yaw;
        // set timers
        tStamp = Time.time;
        t1 = info.lerpTime;
        t2 = info.revealTime;
        t3 = info.lerpTime;
        // Set timer bools
        revealStartDone = false;
        revealPauseDone = false;
        // Set cam target desired pos
        info.SetupCamRevealInfo();
        user.camTarget.revealInitPos = info.camTargetStartPos;
        user.camTarget.revealTargetPos = info.camTargetTargetPos.position;
        user.camTarget.revealLerpTime = info.lerpTime;
        user.camTarget.targetPosSV = new Vector3(0, 0, 0);
        user.camTarget.tStamp = Time.time;
    }

    public void IfStateChange() { }

    public void StateUpdate()
    {
        if (user.stateDebugLog) Debug.Log("sCamReveal <color=blue>Update</color>");

        if (!revealStartDone)
        {
            t = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - tStamp) / info.lerpTime);
            //Debug.Log("Timedottime : " + Time.time.ToString() + " stamp : " + tStamp.ToString() + " result : " + t.ToString());
            if (t < 1)
            {
                if (user.stateDebugLog) Debug.Log("sCamReveal <color=red>StartReveal</color>");
                user.pitch = Mathf.Lerp(lastPitch, info.desiredPitch, t);
                user.yaw = Mathf.Lerp(lastYaw, info.desiredYaw, t);
                user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * user.desiredDollyDst, ref resetSV, 0.025f);
            }
            else revealStartDone = true;
        }

        if (revealStartDone && !revealPauseDone)
        {
            if (user.stateDebugLog) Debug.Log("sCamReveal <color=red>PauseReveal</color>");
            t2 -= 1.0f * Time.deltaTime;
            if (t2 <= 0)
            {
                revealPauseDone = true;
                tStamp = Time.time;
                user.camTarget.tStamp = Time.time;
            }
        }

        if (revealPauseDone)
        {
            t = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - tStamp) / (info.lerpTime / 2));
            if (t < 1)
            {
                if (user.stateDebugLog) Debug.Log("sCamReveal <color=red>EndReveal</color>");
                user.pitch = Mathf.Lerp(info.desiredPitch, lastPitch, t);
                user.yaw = Mathf.Lerp(info.desiredYaw, lastYaw, t);
                user.tr.position = Vector3.SmoothDamp(user.tr.position, user.camTarget.tr.position - user.tr.forward * user.desiredDollyDst, ref resetSV, 0.025f);
            }
            else user.camFSM.ChangeState(user.defaultState);
        }
    }

public void Exit()
{
    if (user.stateDebugLog) Debug.Log("sCamReveal <color=yellow> Exit </color>");
    InputsManager.instance.gameplayInputsAreDisabled = false;
    user.pitchMinMax = new Vector2(-25.0f, 60.0f);
    }
}
