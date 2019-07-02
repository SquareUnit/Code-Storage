using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCameraTarget : MonoBehaviour
{
    [HideInInspector] public Transform tr;
    [HideInInspector] public PlayerCamera user;
    private float avatarHeight;
    private Vector3 animRootPos;
    [HideInInspector] public float verticalDolly;
    private RaycastHit sphereCastHit;
    private float sphereCastRadius = 0.4f;
    private float sphereCastDistance = 0.65f;

    [Range(-1.5f, 1.5f)] public float heightAdjustment;
    private Vector3 forwardOffset;
    private float forwardOffsetVal = 0.35f;
    private Vector3 yOffset;

    public Vector3 targetPosSV;
    private float SmoothMaxSpeed = 25.0f;
    private Color debugColor;

    public Vector3 revealTargetPos;
    public Vector3 revealInitPos;
    public float revealLerpTime;
    public float t;
    public float tStamp;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void Start()
    {
        tr = transform;
        avatarHeight = GameManager.instance.currentAvatar.GetComponent<CharacterController>().height;
        tr.position = new Vector3(tr.position.x, tr.position.y + avatarHeight, tr.position.z);
    }

    public void LateUpdate()
    {
        if (GameManager.instance.currentAvatar == enabled || GameManager.instance.currentAvatar != null)
        {
            animRootPos = GameManager.instance.currentAvatar.animator.rootPosition;

            SetOffsets();

            if (user.camFSM.currentState != user.revealState)
            {
                SetRotation();
                SetGamePlayPosition();
            }
            else
            {
                SetRevealPosition();
            }
        }
    }

    private void SetOffsets()
    {
        /// Define a forward offset value.
        forwardOffset = tr.forward * forwardOffsetVal;
        /// Define a desired height for camTarget position
        if (GameManager.instance.currentAvatar.stateMachine.currentState == GameManager.instance.currentAvatar.stateMachine.crouch)
        {
            yOffset.y = avatarHeight + heightAdjustment - 0.75f;
        }
        else yOffset.y = avatarHeight + heightAdjustment + verticalDolly;
    }

    /// <summary> Pass the info down to those who needs it </summary>
    private void SetRotation()
    {
        tr.rotation = GameManager.instance.currentAvatar.tr.rotation;
    }

    // Position handling if the camera is not in reveal state
    private void SetGamePlayPosition()
    {
        //If avatar moving, stop forward offset to stop wobble
        if (Physics.SphereCast(animRootPos + yOffset, sphereCastRadius, tr.forward, out sphereCastHit, sphereCastDistance, user.obstaclesMask) && SphereCastHit.collider.tag != "AllowCameraDissolve")
        {
            tr.position = Vector3.SmoothDamp(tr.position, animRootPos + yOffset, ref targetPosSV, 0.013f, SmoothMaxSpeed);
            debugColor = Color.red;
        }
        else
        {
            tr.position = Vector3.SmoothDamp(tr.position, animRootPos + yOffset + forwardOffset, ref targetPosSV, 0.013f, SmoothMaxSpeed);
            debugColor = Color.green;
        }
    }

    // Position handling if the camera is in reveal state
    private void SetRevealPosition()
    {
        if (!user.revealState.revealStartDone)
        {
            t = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - tStamp) / revealLerpTime);
            tr.position = Vector3.Lerp(revealInitPos, revealTargetPos, t);
        }

        if (user.revealState.revealPauseDone)
        {
            t = Mathf.SmoothStep(0.0f, 1.0f, (Time.time - tStamp) / (revealLerpTime / 2));
            tr.position = Vector3.Lerp(revealTargetPos, animRootPos + yOffset + forwardOffset, t);
        }

        debugColor = Color.green;
    }

    private void OnDrawGizmos()
    {
        {
            Debug.DrawRay(tr.position, tr.forward * sphereCastDistance, debugColor);
            Gizmos.color = debugColor;
            Gizmos.DrawWireSphere(sphereCastHit.point, sphereCastRadius);
            Gizmos.DrawSphere(tr.position, 0.05f);
        }
    }

    public RaycastHit SphereCastHit {
        get { return sphereCastHit; }
    }
}
