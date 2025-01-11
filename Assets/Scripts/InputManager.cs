using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public enum PlatformType
{
    PlayStation,
    Xbox,
    PC,
}

public class InputManager : MonoBehaviour
{
#if UNITY_EDITOR
    [Header("Text Log Position"), Tooltip("Specify the position where the text log will be displayed.")]
    [SerializeField] private Transform textLogPosition;

    [Tooltip("Max distance from the camera to the text log.")]
    [SerializeField] private float maxDistance = 15f;
#endif

    private readonly PlatformType platformType = PlatformType.PC;

    private void Awake()
    {
        InitializePlatform(platformType);
    }

    private void InitializePlatform(PlatformType platformType)
    {
        switch (platformType)
        {
            case PlatformType.PlayStation:
                break;
            case PlatformType.Xbox:
                break;
            case PlatformType.PC:
                break;
            default:
                Application.Quit(40082);
                break;
        }
    }

    private void Start()
    {
        RemoveIncompatibleDevices();
    }

    private void RemoveIncompatibleDevices()
    {
        switch (platformType)
        {
            case PlatformType.PlayStation:
            case PlatformType.Xbox:
                ConsoleDisableDevice();
                break;
            case PlatformType.PC:
                ComputerDisableDevice();
                break;
        }
    }

    private void OnEnable()
    {
        InputSystem.onDeviceChange += OnDeviceChange;
    }

    private void OnDisable()
    {
        InputSystem.onDeviceChange -= OnDeviceChange;
    }

    private void OnDeviceChange(InputDevice device, InputDeviceChange change)
    {
        switch (change)
        {
            case InputDeviceChange.Added:
                Debug.Log($"Device added: {device.displayName}");
                HandleDeviceAdded(device);
                break;
        }
    }

    private void HandleDeviceAdded(InputDevice device)
    {
        switch (platformType)
        {
            case PlatformType.PlayStation:
            case PlatformType.Xbox:
                ConsoleDisableDevice();
                break;
            case PlatformType.PC:
                ComputerDisableDevice();
                break;
        }
    }

    private void ConsoleDisableDevice()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Mouse || device is Keyboard)
            {
                InputSystem.DisableDevice(device);
            }
        }
    }

    private void ComputerDisableDevice()
    {
        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad)
            {
                InputSystem.DisableDevice(device);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (textLogPosition == null) return;

        Vector3 position = textLogPosition.position;

        Camera camera = SceneView.lastActiveSceneView.camera;

        if (camera == null) return;

        float distanceToCamera = Vector3.Distance(camera.transform.position, position);

        if (distanceToCamera > maxDistance) return;

        GUIStyle style = new GUIStyle
        {
            normal = { textColor = Color.white },
            fontSize = 12,
            alignment = TextAnchor.UpperLeft,
            richText = true
        };

        // Device connection log
        string connectedDevices = "<color=#FF6347>Connected Devices:</color>\n" +
            string.Join("\n", InputSystem.devices.Select(device => $"<color=#FFD700>- {device.displayName}</color>"));

        int totalDevices = InputSystem.devices.Count;

        // Input system settings log
        string inputSettings = $"<color=#87CEEB>Input Settings:</color>\n" +
            $"- <color=#98FB98>Active Input Mode:</color> {InputSystem.settings.updateMode}\n" +
            $"- <color=#98FB98>Background Behavior:</color> {(PlayerSettings.runInBackground ? "Enabled" : "Disabled")}\n";

        // Current input device and status log (e.g., keyboard, mouse, gamepad)
        string inputStatus = "<color=#32CD32>Input Device Status:</color>\n";
        inputStatus += $"<color=#FF6347>Keyboard:</color> {GetKeyboardStatus()}\n";
        inputStatus += $"<color=#FFD700>Mouse:</color> {GetMouseStatus()}\n";
        inputStatus += $"<color=#98FB98>Active Gamepad:</color> {GetActiveGamepadStatus()}";

        // Text output
        string text = $"<color=#00FFFF>Platform Type:</color> <color=#32CD32>{platformType}</color>\n\n" +
                      $"{connectedDevices}\n\n" +
                      $"<color=#FFA500>Total Devices:</color> <color=#FFFFFF>{totalDevices}</color>\n\n" +
                      $"{inputSettings}\n\n" +
                      $"{inputStatus}";

        UnityEditor.Handles.Label(position, text, style);
    }

    // Keyboard status log
    private string GetKeyboardStatus()
    {
        if (Keyboard.current != null)
        {
            return $"Keys Pressed: {string.Join(", ", Keyboard.current.allKeys.Where(key => key.isPressed).Select(key => key.displayName))}";
        }
        return "No Keyboard Detected";
    }

    // Mouse status log
    private string GetMouseStatus()
    {
        if (Mouse.current != null)
        {
            return $"Button Pressed: {(Mouse.current.leftButton.isPressed ? "Left" : Mouse.current.rightButton.isPressed ? "Right" : Mouse.current.middleButton.isPressed ? "Middle" : "None")}";
        }
        return "No Mouse Detected";
    }

    // Active gamepad status log
    private string GetActiveGamepadStatus()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            return $"Left Stick: {gamepad.leftStick.ReadValue()}, A Button: {gamepad.buttonSouth.isPressed}";
        }
        return "No Gamepad Detected";
    }
}
