/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : 
/// https://github.com/SquareUnit/Code-Storage

/// Simplistic tool made for the level designer so they could store data, telling what the camera reveal would do
/// during it's state time. Very simple. Allow rotations and panning, cannot be done in sequences. Thats to be changed.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamRevealInfo : MonoBehaviour
{
    
    [HideInInspector] public Vector3 camTargetStartPos;
    [Header("Hover on var names to display tooltips")]
    [Tooltip("Leave empty if you just want the camera rotation to move")]
    public Transform camTargetTargetPos;
    public float lerpTime;
    public float revealTime;
    public float desiredYaw;
    public float desiredPitch;

    public void SetupCamRevealInfo()
    {
        camTargetStartPos = GameManager.instance.cameraManager.thirdPersonCam.camTarget.tr.position;
        if(camTargetTargetPos == null)
        {
            camTargetTargetPos = GameManager.instance.cameraManager.thirdPersonCam.camTarget.tr;
        }
    }
}
