using System.Collections;
using UnityEngine;

// Rotates camera around level when game is paused, player is dead or exit is reached
public class CameraRotator : MonoBehaviour {

    [SerializeField]
    private float _distance = 10.0f;
    [SerializeField]
    private float _rotationSpeed = 20.0f;
    private bool rotateOn;
    private Vector3 levelCenterPoint;

    // Start is called before the first frame update
    void Start() {
        // Camera is instantiated at level center point
        levelCenterPoint = transform.position;

        // Move camera away from level center point (if not already away)
        transform.position += Vector3.back * _distance;
        
        // Main menu always has whirling camera
        if (GameManager.instance.GetSceneName() == "Main Menu") {
            rotateOn = true;
            StartCoroutine(RotateCameraRoutine());
        }
    }

    /********************************************************/
    public void SetRotateOn(bool rotate) {

        if (!rotateOn) {
            rotateOn = rotate;
            StartCoroutine(RotateCameraRoutine());
        }
    }

    /********************************************************/
    IEnumerator RotateCameraRoutine() {

        float currentTime, dTime, up, right, forward;

        // Fluctuate rotation speeds around each axis at different intervals
        while (rotateOn) {
            currentTime = Time.time;
            dTime = Time.deltaTime;

            up = Mathf.Sin(currentTime);
            right = Mathf.Cos(currentTime);
            forward = -Mathf.Cos(currentTime);

            transform.RotateAround(levelCenterPoint, Vector3.up, -_rotationSpeed * up * dTime);
            transform.RotateAround(levelCenterPoint, Vector3.right, _rotationSpeed * right * dTime);
            transform.RotateAround(levelCenterPoint, Vector3.forward, _rotationSpeed * forward * dTime);

            yield return null;
        }
    }
}
