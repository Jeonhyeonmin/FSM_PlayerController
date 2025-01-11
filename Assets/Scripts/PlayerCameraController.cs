using jeonhyeonmin;
using UnityEngine;

public class PlayerCameraController : MonoBehaviour
{
    [Header("Scripts / Objects References")]
    [SerializeField] private Transform dummyCharacter;
    [SerializeField] private Camera playerCamera;

    [SerializeField] private InputReader inputReader;

    [Space(20), Header("Camera Settings")]
    [SerializeField] private float cameraSensitivity = 5f;
    [SerializeField] private float cameraSmoothing = 2f;

    [SerializeField] private float cameraDistance = 3.5f;
    [SerializeField] private float cameraHeightRelativeToPivot = 0.05f;

    [Space(20), Header("Camera Limits")]
    [SerializeField] private float minCameraAngleZ = -3.5f;
    [SerializeField] private float maxCameraAngleZ = 3.5f;

    [SerializeField] private float minPivotAngleX = -40;
    [SerializeField] private float maxPivotAngleX = 50;

    [Space(20), Header("Camera Pivot")]
    [SerializeField] private Transform cameraPivot;
    private Vector3 cameraPivotVector;

    private void Awake()
    {
        cameraPivotVector = cameraPivot.position;
        Vector3 cameraDistance = new Vector3(cameraPivot.position.x, cameraPivot.position.y + cameraHeightRelativeToPivot, cameraPivot.position.z - this.cameraDistance);
        playerCamera.transform.position = cameraDistance;

        cameraPivot.rotation = Quaternion.Euler(5, 0, 0);
    }
    
    private void LateUpdate()
    {
        cameraPivot.position = dummyCharacter.position + Vector3.up * cameraPivotVector.y;
    }

    private void Update()
    {
        HandleCamera();
    }

    private void HandleCamera()
    {
        Vector3 lookDirection = new Vector3(-inputReader.MouseComposite.y, inputReader.MouseComposite.x, 0);

        float cameraAngleX = cameraPivot.eulerAngles.x + lookDirection.x * cameraSensitivity;
        float cameraAngleY = cameraPivot.eulerAngles.y + lookDirection.y * cameraSensitivity;

        cameraAngleX = cameraAngleX > 180 ? cameraAngleX - 360 : cameraAngleX;
        cameraAngleX = Mathf.Clamp(cameraAngleX, minPivotAngleX, maxPivotAngleX);

        cameraPivot.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(cameraAngleX, cameraAngleY, 0), 0.13f);
    }
}
