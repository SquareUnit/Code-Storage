/// Designed by Félix Desrosiers-Dorval
/// Last modification date : 2019-07-01
/// Last feature added : Inputs all mapped to hash, memory usage fell to 1/20 after change.
/// https://github.com/SquareUnit/Code-Storage

/// Require using rewired. Get all inputs and pass them from raw to ready to use. Handle deadzones too.
/// Require a bit of refactoring

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Rewired;
using System;

public class InputsManager : MonoBehaviour
{
    public static InputsManager instance;
    private int playerId = 0;
    public static Player player;

    [HideInInspector]
    public bool isInDebug = false; //start the debug mode
    //[HideInInspector]
    public bool isntDebug = false; // end the debug mode

    public enum GameState { mainMenu, pauseMenu, game, debug };
    public GameState currentGameState;

    #region Ready to use inputs  
    // Core Inputs : ALWAYS AVAILABLE, NEVER DISABLE OR I SPANK YOU
    [HideInInspector] public bool pause { get; private set; }

    // Character movements and actions
    [HideInInspector] public Vector3 leftStick { get; private set; }
    [HideInInspector] public Vector3 rightStick { get; private set; }
    [HideInInspector] public bool camButton { get; private set; }
    [HideInInspector] public bool jump { get; private set; }
    [HideInInspector] public bool jumpHigh { get; private set; }
    [HideInInspector] public bool crouch { get; private set; }
    [HideInInspector] public bool interact { get; private set; }
    [HideInInspector] public bool back { get; private set; }
    [HideInInspector] public bool triggerLeft { get; private set; }
    [HideInInspector] public bool triggerLeftDown { get; private set; }
    [HideInInspector] public bool triggerLeftUp { get; private set; }
    [HideInInspector] public bool triggerRight { get; private set; }
    [HideInInspector] public bool triggerRightDown { get; private set; }
    [HideInInspector] public bool triggerRightUp { get; private set; }
    [HideInInspector] public bool bumperRight { get; private set; }
    [HideInInspector] public bool bumperLeft { get; private set; }

    // Keypad for debug and editor testing
    [HideInInspector] public bool start { get; private set; }
    [HideInInspector] public bool select { get; private set; }
    [HideInInspector] public bool crossLeft { get; private set; }
    [HideInInspector] public bool crossRight { get; private set; }
    [HideInInspector] public bool crossUp { get; private set; }
    [HideInInspector] public bool crossDown { get; private set; }
    [HideInInspector] public KeyCode lvlKey { get; private set; }
    [HideInInspector] public bool cheatMenuActive { get; private set; }

    // Instrument toggle. Pour vous les LD! Faite votre propre musique!!!
    public bool[] fKeyArray = new bool[24];

    // Mouse
    [HideInInspector] public bool mouseLeftButton { get; private set; }
    [HideInInspector] public bool mouseRightButton { get; private set; }
    #endregion

    #region Inputs hashs
    public int pauseHash;
    public int horizontalHash;
    public int verticalHach;
    public int camYawHash;
    public int camPitchHash;
    public int interactHash;
    public int jumpHash;
    public int crouchHash;
    
    public int triggerLeftHash;
    public int bumperLeftHash;
    public int triggerRightHash;
    public int bumperRightHash;
    public int camButtonHash;
    public int backHash;

    public int mouseLeftButtonHash;
    public int mouseRightButtonHash;

    public int cheatActiveHash;

    public int startHash;
    public int selectHash;
    public int crossLeftHash;
    public int crossRightHash;
    public int crossUpHash;
    public int crossDownHash;

    #endregion

    #region Pipeline for input management
    // Inputs manager params
    private Vector3 leftStickRaw;
    private Vector3 rightStickRaw;
    [Range(0, 1)] public float genericDeadZone = 0.4f;
    [Range(0, 1)] public float rightStickDeadZone = 0.25f;
    private bool inputXInDeadZone, inputYInDeadZone, inputZInDeadZone;

    //keyboard stuff
    public bool keyboardKeyWasPressed = false;
    public Event keyboardEvent;
    public UnityEvent ToggleRD;

    // Disabling inputs
    [HideInInspector] public bool gameplayInputsAreDisabled;
    [HideInInspector] public bool cameraInputsAreDisabled;
    [HideInInspector] public bool gameplayInputsForDialog;

    // Simple inputs interpretations
    [HideInInspector] public bool PlayerIsMovingAvatar;
    #endregion

    private void Awake()
    {
        SingletonSetup();

        player = ReInput.players.GetPlayer(playerId);
        SetInputsHash();
        keyboardEvent = Event.current;
        currentGameState = GameState.mainMenu;
    }

    public void SetInputsHash()
    {
        pauseHash = ReInput.mapping.GetActionId("pause");
        horizontalHash = ReInput.mapping.GetActionId("horizontal");
        verticalHach = ReInput.mapping.GetActionId("vertical");
        camYawHash = ReInput.mapping.GetActionId("camYaw");
        camPitchHash = ReInput.mapping.GetActionId("camPitch");
        interactHash = ReInput.mapping.GetActionId("interact");
        jumpHash = ReInput.mapping.GetActionId("jump");
        crouchHash = ReInput.mapping.GetActionId("crouch");
        triggerLeftHash = ReInput.mapping.GetActionId("triggerLeft");
        bumperLeftHash = ReInput.mapping.GetActionId("bumperLeft");
        triggerRightHash = ReInput.mapping.GetActionId("triggerRight");
        bumperRightHash = ReInput.mapping.GetActionId("bumperRight");
        camButtonHash = ReInput.mapping.GetActionId("camButton");
        backHash = ReInput.mapping.GetActionId("back");
        mouseLeftButtonHash = ReInput.mapping.GetActionId("mouseLeftButton");
        mouseRightButtonHash = ReInput.mapping.GetActionId("mouseRightButton");
        cheatActiveHash = ReInput.mapping.GetActionId("cheatActive");
        startHash = ReInput.mapping.GetActionId("start");
        selectHash = ReInput.mapping.GetActionId("select");
        crossLeftHash = ReInput.mapping.GetActionId("crossLeft");
        crossRightHash = ReInput.mapping.GetActionId("crossRight");
        crossUpHash = ReInput.mapping.GetActionId("crossUp");
        crossDownHash = ReInput.mapping.GetActionId("crossDown");
    }

    void Update()
    {
        GetCoreInputs();
        GetDebugMenuInputs();
        GetGamePlayInputs();
        GetFKeyInputs();

        PlayerIsMovingAvatar = CheckIfPlayerIsMovingAvatar();

        if (cameraInputsAreDisabled) OverwriteAllCameraInputs();
        if (gameplayInputsAreDisabled) OverwriteAllGameplayInputs();
        if (gameplayInputsForDialog) OverwriteAllInputsDialog();
    }
    /// <summary> Gerer le setup de base du UI </summary>
    private void MenuBaseSetup()
    {
        GameManager.instance.uiManager.cheatUIPanel.SetActive(false);
    }

    /// <summary> Inputs that should always be available at runtime</summary>
    private void GetCoreInputs()
    {
        pause = player.GetButtonDown(pauseHash.GetHashCode());
        crouch = player.GetButtonDown(crouchHash.GetHashCode());
        currentGameState = GameState.pauseMenu;
    }

    /// <summary> All the inputs needed by the player during the gameplay, some are sent to check for dead zone </summary>
    private void GetGamePlayInputs()
    {
        leftStick = GetMovementsInputs();
        rightStick = GetCamInputs();
        camButton = player.GetButtonDown(camButtonHash.GetHashCode());

        jump = player.GetButtonDown(jumpHash.GetHashCode());
        jumpHigh = player.GetButton(jumpHash.GetHashCode());
        interact = player.GetButtonDown(interactHash.GetHashCode());
        crouch = player.GetButtonDown(crouchHash.GetHashCode());
        back = player.GetButtonDown(backHash.GetHashCode());
        start = player.GetButtonDown(startHash.GetHashCode());
        
        triggerLeft = player.GetButton(triggerLeft.GetHashCode());
        triggerRight = player.GetButton(triggerRightHash.GetHashCode());
        triggerLeftDown = player.GetButtonDown(triggerLeftHash.GetHashCode());
        triggerRightDown = player.GetButtonDown(triggerRightHash.GetHashCode());
        triggerLeftUp = player.GetButtonUp(triggerLeftHash.GetHashCode());
        triggerRightUp = player.GetButtonUp(triggerRightHash.GetHashCode());

        bumperLeft = player.GetButton(bumperLeftHash.GetHashCode());
        bumperRight = player.GetButton(bumperRightHash.GetHashCode());

        currentGameState = GameState.game;
    }

    private void GetFKeyInputs()
    {
        for (int i = 0; i < 23; i++)
        {
            string temp = "f" + (i + 1).ToString();
            if(player.GetButtonDown(temp))
            {
                fKeyArray[i] = true;
            }
            else
            {
                fKeyArray[i] = false;
            }
        }
    }

    /// <summary> Get left stick and send it for dead zone check, return clean inputs</summary>
    private Vector3 GetMovementsInputs()
    {
        leftStickRaw = new Vector3(player.GetAxisRaw(horizontalHash.GetHashCode()), 0.0f, player.GetAxisRaw(verticalHach.GetHashCode()));
        return MovementDeadZone(leftStickRaw);
    }

    /// <summary> Get right stick and send it for dead zone check, return clean inputs</summary>
    private Vector3 GetCamInputs()
    {
        rightStickRaw = new Vector3(player.GetAxisRaw(camYawHash.GetHashCode()), player.GetAxisRaw(camPitchHash.GetHashCode()), 0.0f);
        return CameraDeadZone(rightStickRaw);
    }

    /// <summary> Inputs for navigating in the debug menu</summary>
    private void GetDebugMenuInputs()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ToggleRD.Invoke();
        }

        //cheatMenuActive = player.GetButtonDown(CHEATMENUACTIVE);

        crossDown = player.GetButtonDown(crossDownHash.GetHashCode());
        if (crossDown)
        {
            if (!cheatMenuActive)
                cheatMenuActive = true;
            else
            {
                cheatMenuActive = false;
                GameManager.instance.cheatMenu.EndDebug();
            }
        }


        //isntDebug = player.GetButtonDown(SELECT);
        select = player.GetButtonDown(selectHash.GetHashCode());
        crossLeft = player.GetButtonDown(crossLeftHash.GetHashCode());
        crossRight = player.GetButtonDown(crossRightHash.GetHashCode());
        crossUp = player.GetButtonDown(crossUpHash.GetHashCode());


        back = player.GetButtonDown(backHash.GetHashCode());

        /*
        if (back && cheatMenuActive)
        {
            //Debug.Log("^");            
            cheatMenuActive = false;
            isntDebug = true;
        }
        */
    }

    private void OnGUI()
    {
        //keyboard inputs
        // Modifier de key : & = Alternate, ^ = Control, % = Command/Windows key, # = Shift, 
        // Examples: &f12 = Alternate + F12, "^[0]" = Control + keypad0.
        if (cheatMenuActive)
        {
            keyboardEvent = Event.current;
            if (keyboardEvent != null)
            {
                if (keyboardEvent.isKey)
                {
                    lvlKey = keyboardEvent.keyCode;
                    keyboardKeyWasPressed = true;
                }
                else
                {
                    keyboardKeyWasPressed = false;
                }
            }
        }

    }

    private bool CheckIfPlayerIsMovingAvatar()
    {
        if (leftStick.x != 0 || leftStick.z != 0) return true;
        else return false;
    }

    private Vector3 MovementDeadZone(Vector3 leftStickRaw)
    {
        inputXInDeadZone = leftStickRaw.x < genericDeadZone && leftStickRaw.x > -genericDeadZone;
        inputZInDeadZone = leftStickRaw.z < genericDeadZone && leftStickRaw.z > -genericDeadZone;
        if (inputXInDeadZone)
            leftStickRaw.x = 0.0f;
        if (inputZInDeadZone)
            leftStickRaw.z = 0.0f;
        return leftStickRaw;
    }

    /// <summary> Check if input is in dead zone, then remap the value range from 0.75 to a full range of 1</summary>
    /// <param name="rightStickRaw"></param> <returns> It's parameter</returns>
    private Vector3 CameraDeadZone(Vector3 rightStickRaw)
    {
        // If the x value of the right stick is bigger than 0, but smaller than 0.25, then set it to 0.
        // Else remap the possible values from [0.25, 1] to [0, 1].
        if (rightStickRaw.x >= 0)
        {
            inputXInDeadZone = rightStickRaw.x < rightStickDeadZone;
            if (inputXInDeadZone) rightStickRaw.x = 0.0f;
            else
            {
                float remapedVal = ((rightStickRaw.x - 0.25f) / 0.75f);
                rightStickRaw.x = remapedVal;
            }
        }
        // If the x value of the right stick is smaller than 0, but bigger than -0.25, then set it to 0.
        // Else remap the possible values from [-1, -0.25] to [-1, 0].
        if (rightStickRaw.x < 0)
        {
            inputXInDeadZone = rightStickRaw.x > -rightStickDeadZone;
            if (inputXInDeadZone) rightStickRaw.x = 0.0f;
            else
            {
                float remapedVal = ((rightStickRaw.x + 0.25f) / 0.75f);
                rightStickRaw.x = remapedVal;
            }
        }

        // If the y value of the right stick is bigger than 0, but smaller than 0.25, then set it to 0.
        // Else remap the possible values from [0.25, 1] to [0, 1].
        if (rightStickRaw.y >= 0)
        {
            inputYInDeadZone = rightStickRaw.y < rightStickDeadZone;
            if (inputYInDeadZone) rightStickRaw.y = 0.0f;
            else
            {
                float remapedVal = ((rightStickRaw.y - 0.25f) / 0.75f);
                rightStickRaw.y = remapedVal;
            }
        }

        // If the y value of the right stick is smaller than 0, but bigger than -0.25, then set it to 0.
        // Else remap the possible values from [-1, -0.25] to [-1, 0].
        if (rightStickRaw.y < 0)
        {
            inputYInDeadZone = rightStickRaw.y > -rightStickDeadZone;
            if (inputYInDeadZone) rightStickRaw.y = 0.0f;
            else
            {
                float remapedVal = ((rightStickRaw.y + 0.25f) / 0.75f);
                rightStickRaw.y = remapedVal;
            }
        }
        return rightStickRaw;
    }

    private void SingletonSetup()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);
    }

    private void OverwriteAllGameplayInputs()
    {
        leftStick = new Vector3(0, 0, 0);
        rightStick = new Vector3(0, 0, 0);
        camButton = false;

        jump = false;
        jumpHigh = false;
        interact = false;
        crouch = false;

        triggerLeft = false;
        triggerRight = false;
        triggerLeftDown = false;
        triggerRightDown = false;
        triggerLeftUp = false;
        triggerRightUp = false;

        bumperLeft = false;
        bumperRight = false;
    }

    private void OverwriteAllCameraInputs()
    {
        rightStick = new Vector3(0, 0, 0);
        camButton = false;
    }

    private void OverwriteAllInputsDialog()
    {
        leftStick = new Vector3(0, 0, 0);
        rightStick = new Vector3(0, 0, 0);
        camButton = false;

        interact = false;
        crouch = false;

        triggerLeft = false;
        triggerRight = false;
        triggerLeftDown = false;
        triggerRightDown = false;
        triggerLeftUp = false;
        triggerRightUp = false;

        bumperLeft = false;
        bumperRight = false;
    }
    /// <summary>
    /// Active la vibration du controller
    /// </summary>
    /// <param name="motorLevel"> Puissance du motor (0 a 1f) </param>
    /// <param name="time"> Duree en secondes </param>
    /// <param name="stopOtherMotors"> Stop les autres vibrations </param>
    public void SetVibration(float motorLevel, float time, bool stopOtherMotors)
    {
        player.SetVibration(0, motorLevel, time, stopOtherMotors);
    }
    public void SetVibration(float motorLevel)
    {
        player.SetVibration(0, motorLevel);
    }
    public void StopVibration()
    {
        player.StopVibration();
    }
}

