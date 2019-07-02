///Creation date : 03-04-19
///Par: Felix Desrosiers-Dorval

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PlayerCamera : MonoBehaviour
{
    [HideInInspector] public Transform tr;
    [HideInInspector] public new Camera camera;
    [HideInInspector] public PlayerCameraTarget playerCamTarget;
    [HideInInspector] public PlayerCameraTarget camTarget;
    [HideInInspector] public LayerMask obstaclesMask;
    private bool loadingDone = false;

    [HideInInspector] public StateMachine camFSM;
    [HideInInspector] public PlayerCamOnLoad onLoadState;
    [HideInInspector] public PlayerCamDefault defaultState;
    [HideInInspector] public PlayerCamCollision collisionState;
    [HideInInspector] public PlayerCamFall fallingState;
    [HideInInspector] public PlayerCamReset resetState;
    [HideInInspector] public PlayerCamReveal revealState;
    [HideInInspector] public PlayerCamValve valveState;

    [HideInInspector] public CharacterState avatarFSM;

    [Range(40.0f, 115.0f)] public float camFOV = 60.0f;
    public float yaw;
    public float pitch;
    public float camPosSpeed = 0.009f;
    private float camYawSensitivity = 10.0f, camPitchSensitivity = 10.0f;
    [HideInInspector] public Vector2 pitchMinMax = new Vector2(-25, 60);

    private Vector3 currRotation, desiredRotation;
    private Vector3 rotationSV;
    private float a1, b1, a2, b2;

    // Audio
    public AudioLowPassFilter lowPassFilter;

    // Collision params
    private Vector3 dirToCamera;
    private Collider lastCollFound;
    [HideInInspector] public RaycastHit hit;
    [HideInInspector] public float desiredCollOffset = 0.5f;
    [HideInInspector] public bool isColliding;

    // Sideray's params
    private RaycastHit hitRight;
    private RaycastHit hitLeft;

    // Shader dissolve params
    private float a3 = 2.1f, b3 = 4.0f;
    private MeshRenderer collMeshRend;
    private List<Material> dissolvedMats = new List<Material>();
    private float lastHitDist;
    private float dissolveIncr, targetDissolveRad, dissolveVal;
    private bool canDecrementDissolveRad;
    private float delayToFullDissolve = 0.5f;
    private bool canIncrDissolveTStamp = true, canDecrDissolveTStamp = true;
    private float incrDissolveTStamp, decrDissolveTStamp;
    private float currShaderDissRadius = 0.0f;

    // Camera dolly params
    [HideInInspector] public float desiredDollyDst;
    [HideInInspector] public float camDollyMinDist = 2.4f;
    [HideInInspector] public float camDollyMaxDist = 5.0f;

    // Camera yOffset params
    private float desiredYOffset;
    private float targetMinYOffset = 0.0f;
    private float targetMaxYOffset = 0.15f;

    [HideInInspector] public bool camAxisInUse;

    public bool stateDebugLog;
    public bool raycastsDebug = true;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        tr = transform;
        camera = GetComponent<Camera>();
        camera.fieldOfView = camFOV;
        UIManager.instance.refCamera = camera;
        lowPassFilter = GetComponentInChildren<AudioLowPassFilter>();
        avatarFSM = GameManager.instance.currentAvatar.GetComponent<CharacterState>();
        obstaclesMask = LayerMask.GetMask("Obstacles");
        SetCamDollyParams();

        camFSM = GetComponent<StateMachine>();
        onLoadState = new PlayerCamOnLoad(this);
        defaultState = new PlayerCamDefault(this);
        collisionState = new PlayerCamCollision(this);
        fallingState = new PlayerCamFall(this);
        resetState = new PlayerCamReset(this);
        revealState = new PlayerCamReveal(this);
        valveState = new PlayerCamValve(this);

        StartCoroutine(WaitForLoading(3.0f));
    }

    /// <summary> Wait until loading is done, then fetch player, create camTarget and start machine</summary>
    private IEnumerator WaitForLoading(float delay)
    {
        tr.eulerAngles = GameManager.instance.currentAvatar.transform.rotation.eulerAngles;
        camTarget = Instantiate(playerCamTarget, GameManager.instance.spcLvlMan.playerStart, GameManager.instance.spcLvlMan.playerStartRotation);
        camTarget.user = this;
        camFSM.ChangeState(defaultState);
        loadingDone = true;
        Debug.Log("loadingDone");
        yield return new WaitForSeconds(delay);
    }

    private void LateUpdate()
    {
        if (loadingDone)
        {
            UpdateCamera();
            camFSM.CheckIfStateChange();
            camFSM.CurrentStateUpdate();
        }
    }

    private void UpdateCamera()
    {
        CameraGetInputs();
        CameraVerticalDolly();
        CameraOrientation();
        CameraHorizontalDolly();
        IsCameraBeingUsed();
        IsCamViewBlocked();
        if (!isColliding) CameraSideRays();
    }

    private void CameraGetInputs()
    {
        /// Setup yaw. Will use movement axis if camera not being used, else only use camera axis.
        if (InputsManager.instance.rightStick.x > 0)
        {
            yaw += Mathf.Pow(InputsManager.instance.rightStick.x * camYawSensitivity, 2.1f) * Time.deltaTime;
        }
        if (InputsManager.instance.rightStick.x < 0)
        {
            yaw += -(Mathf.Pow(Mathf.Abs(InputsManager.instance.rightStick.x) * camYawSensitivity, 2.1f)) * Time.deltaTime;
        }

        if (InputsManager.instance.rightStick.x == 0
            && InputsManager.instance.leftStick.x >= 0.25
            && InputsManager.instance.cameraInputsAreDisabled == false)
        {
            yaw += Mathf.Pow(InputsManager.instance.leftStick.x * 2.5f, 4.6f) * Time.deltaTime;
        }
        if (InputsManager.instance.rightStick.x == 0
            && InputsManager.instance.leftStick.x <= -0.25
            && InputsManager.instance.cameraInputsAreDisabled == false)
        {
            yaw += -(Mathf.Pow(Mathf.Abs(InputsManager.instance.leftStick.x) * 2.5f, 4.6f)) * Time.deltaTime;
        }
        /// Clamp the yaw to [0, 360]. Anti-gimbal-lock. Works pretty well!!!
        if (yaw > 360.0f)
        {
            //yaw = yaw % 360 * Mathf.Sign(yaw); TODO : Replace current matsh for that single line of code
            yaw -= 360.0f;
            currRotation = new Vector3(currRotation.x, currRotation.y - 360, currRotation.z);
        }
        if (yaw < 0.0f)
        {
            yaw += 360.0f;
            currRotation = new Vector3(currRotation.x, currRotation.y + 360, currRotation.z);
        }
        /// Setup pitch and clamp it
        pitch -= InputsManager.instance.rightStick.y * Mathf.Pow(camPitchSensitivity, 2.0f) * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, pitchMinMax.x, pitchMinMax.y);
    }

    /// <summary> Dolly the camera upward or downward depending on the pitch. Linear function start dollying below 0 </summary>
    private void CameraVerticalDolly()
    {
        desiredYOffset = a2 * pitch + b2;
        camTarget.verticalDolly = Mathf.Clamp(desiredYOffset, targetMinYOffset, targetMaxYOffset);
    }

    ///<summary> Set up the camera orientation </summary>
    private void CameraOrientation()
    {
        desiredRotation = new Vector3(pitch, yaw, 0.0f);
        currRotation = Vector3.SmoothDamp(currRotation, desiredRotation, ref rotationSV, 0.09f);
        tr.eulerAngles = currRotation;
    }

    /// <summary> Dolly the camera forward or backward depending on the pitch. Linear function start dollying below 0 </summary>
    private void CameraHorizontalDolly()
    {
        desiredDollyDst = a1 * pitch + b1;
        desiredDollyDst = Mathf.Clamp(desiredDollyDst, camDollyMinDist, camDollyMaxDist);
    }

    /// <summary> Verify if the player is actively manipulating the camera with the right stick axis </summary>
    private void IsCameraBeingUsed()
    {
        if (InputsManager.instance.rightStick.x == 0 && InputsManager.instance.rightStick.y == 0) camAxisInUse = false;
        else camAxisInUse = true;
    }

    /// <summary> Check if there is a valid collision </summary>
    private void IsCamViewBlocked()
    {
        dirToCamera = tr.position - camTarget.tr.position;
        if (raycastsDebug) Debug.DrawRay(camTarget.transform.position, dirToCamera, Color.white);
        if (Physics.Raycast(camTarget.tr.position, dirToCamera, out hit, camDollyMaxDist, obstaclesMask))
        {
            if (hit.collider.tag != "AllowCameraDissolve")
            {
                canIncrDissolveTStamp = true;
                canDecrDissolveTStamp = true;
                float product = Vector3.Dot(hit.normal, Vector3.up);
                if (product <= 0.3 && product >= -0.3) //TODO : Handle ceilings(|| hit.normal.y < 0))
                {
                    IsCollisionValid();
                }
                else isColliding = false;
            }
            else if (hit.collider.tag == "AllowCameraDissolve")
            {
                isColliding = false;
                canDecrDissolveTStamp = true;
                if (canIncrDissolveTStamp)
                {
                    canIncrDissolveTStamp = false;
                    incrDissolveTStamp = Time.time;
                }

                lastHitDist = Vector3.Distance(camTarget.tr.position, hit.point);
                if (lastHitDist < 0.3f) lastHitDist = 0.3f;

                targetDissolveRad = (a3 * lastHitDist) + b3;
                float t = (Time.time - incrDissolveTStamp) / delayToFullDissolve;
                dissolveVal = Mathf.SmoothStep(currShaderDissRadius, targetDissolveRad, t);

                collMeshRend = hit.transform.parent.GetComponentInChildren<MeshRenderer>();
                foreach (Material i in collMeshRend.materials)
                {
                    if (!dissolvedMats.Contains(i))
                    {
                        dissolvedMats.Add(i);
                        i.SetVector("_avatarPos", GameManager.instance.currentAvatar.tr.position);
                        i.SetFloat("_radius", 1.0f);
                        currShaderDissRadius = i.GetFloat("_radius");
                    }
                    else
                    {
                        i.SetFloat("_radius", dissolveVal);
                        i.SetVector("_avatarPos", GameManager.instance.currentAvatar.tr.position);
                        currShaderDissRadius = i.GetFloat("_radius");
                    }
                }
            }
        }
        else // If raycast not picking up anything
        {
            isColliding = false;
            canIncrDissolveTStamp = true;
            if (canDecrDissolveTStamp)
            {
                canDecrDissolveTStamp = false;
                decrDissolveTStamp = Time.time;
                targetDissolveRad = currShaderDissRadius;
            }
            float t = (Time.time - decrDissolveTStamp) / delayToFullDissolve;
            if (dissolvedMats.Count != 0)
            {
                dissolveVal = Mathf.SmoothStep(targetDissolveRad, 0.0f, t);
                foreach (Material i in dissolvedMats)
                {
                    i.SetFloat("_radius", dissolveVal);
                    i.SetVector("_avatarPos", GameManager.instance.currentAvatar.tr.position);
                    currShaderDissRadius = i.GetFloat("_radius");
                }
                if (dissolveVal == 0.0f) dissolvedMats.Clear();
            }
        }
    }

    /// <summary> Check if the collsion distance small enough for it to be considered valid for camera wall hoovering behaviour?</summary>
    public void IsCollisionValid()
    {
        Vector3 hitToCamDir = camTarget.tr.position - hit.point;
        hitToCamDir.y = 0.0f;
        float hitAngle = Vector3.Angle(hit.normal, hitToCamDir);
        float hitToCamDist = Vector3.Distance(hit.point, camTarget.tr.position) - desiredDollyDst;
        float wallToCamDist = Mathf.Sin(Mathf.Deg2Rad * hitAngle) * hitToCamDist;

        if (wallToCamDist < desiredCollOffset) isColliding = true;
        else isColliding = false;
    }

    /// <summary> Cast two rays parallel to the camera LOS. Nudge the camera away from wall collisions </summary>
    private void CameraSideRays()
    {
        Vector3 rayOrigin;
        Vector3 rayDir;
        float camCorrectionSpd = 60 * (1 + Mathf.Abs(InputsManager.instance.leftStick.x));
        float sideRaysdist = 1.4f * Mathf.Abs(InputsManager.instance.leftStick.x);
        // if (GameManager.instance.debugTimer % 10 == 0) Debug.Log("Speed : " + camCorrectionSpd + "Distance : " + sideRaysdist);

        if (InputsManager.instance.PlayerIsMovingAvatar)
        {
            // Parallel ray right of LOS
            rayOrigin = tr.position + (sideRaysdist * tr.right);
            rayDir = camTarget.tr.position - tr.position;
            if (Physics.Raycast(rayOrigin, rayDir, out hitRight, desiredDollyDst, obstaclesMask) && hitRight.collider.tag != "AllowCameraDissolve")
            {
                if (raycastsDebug) Debug.DrawRay(rayOrigin, rayDir, Color.red);
                yaw += camCorrectionSpd * Time.deltaTime;
            }
            else if (raycastsDebug) Debug.DrawRay(rayOrigin, rayDir, Color.green);

            // Parallel ray left of LOS
            rayOrigin = tr.position - (sideRaysdist * tr.right);
            rayDir = camTarget.tr.position - tr.position;
            if (Physics.Raycast(rayOrigin, rayDir, out hitLeft, desiredDollyDst, obstaclesMask) && hitLeft.collider.tag != "AllowCameraDissolve")
            {
                if (raycastsDebug) Debug.DrawRay(rayOrigin, rayDir, Color.red);
                yaw -= camCorrectionSpd * Time.deltaTime;
            }
            else if (raycastsDebug) Debug.DrawRay(rayOrigin, rayDir, Color.green);
        }
    }

    /// <summary> Determin how the forward/backward dolly and vertical offset will move the camera, depending on the parameters </summary>
    private void SetCamDollyParams()
    {
        // Forward / backward dolly setup
        a1 = (camDollyMinDist - camDollyMaxDist) / pitchMinMax.x;
        b1 = camDollyMaxDist;
        // Vertical offset setup
        a2 = (targetMaxYOffset - targetMinYOffset) / pitchMinMax.x;
        b2 = targetMinYOffset;
    }

    /// <summary> </summary>
    /// <param name="duration"> How long in game time seconds should the low pass effect last</param>
    /// <param name="cutoffFreq"> The highest frequency that can filter through and to the listener</param>
    public void ApplyLowPassFilter(float duration, int cutoffFreq)
    {
        lowPassFilter.enabled = true;
        lowPassFilter.cutoffFrequency = cutoffFreq;
        Invoke("ResetyLowPassFilter", duration);
    }

    public void ResetyLowPassFilter()
    {
        lowPassFilter.enabled = false;
    }
}

// Tool used to detect the nature of what we are colliding with. Help us debug and find the culprit
// when the camera act unpredictably.
#if UNITY_EDITOR
[CustomEditor(typeof(PlayerCamera))]
public class PlayerCameraDebugTool : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        PlayerCamera user = (PlayerCamera)target;

        if (GUILayout.Button("Get collision name"))
        {
            Debug.Log(user.camTarget.SphereCastHit.collider.transform.root.name + " : is root");
            Debug.Log(user.camTarget.SphereCastHit.collider.transform.parent.name + " : is direct parent");
            Debug.Log(user.camTarget.SphereCastHit.collider.gameObject.layer.ToString() + " : is layer hit");
        }
    }
}
#endif
