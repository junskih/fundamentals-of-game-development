using UnityEngine;

// Handles the user input
public class InputManager : MonoBehaviour {
    
    private bool cancelInUse = false;
    
    /********************************************************/
    public void ReceiveInput() {

        // Moving forward
        if (Input.GetAxisRaw("Vertical") > 0.0f) {
            GameManager.instance.SendInput("Forward");
        }

        // Jumping
        if (Input.GetAxisRaw("Jump") > 0.0f) {
            GameManager.instance.SendInput("Jump");
        }

        // Turning
        if (Input.GetAxisRaw("Horizontal") > 0.0f) {
            GameManager.instance.SendInput("TurnRight");

        } else if (Input.GetAxisRaw("Horizontal") < 0.0f) {
            GameManager.instance.SendInput("TurnLeft");
        }

        // Camera tilt
        if (Input.GetAxisRaw("CameraTilt") > 0.0f) {
            GameManager.instance.SendInput("TiltCameraUp");

        } else if (Input.GetAxisRaw("CameraTilt") < 0.0f) {
            GameManager.instance.SendInput("TiltCameraDown");

        } else if (Input.GetAxisRaw("CameraTilt") == 0.0f) {
            GameManager.instance.SendInput("TiltReset");
        }

        // Pausing
        if (Input.GetAxisRaw("Cancel") > 0.0f) {

            // This should in effect be same as Input.KeyDown
            if (!cancelInUse) {
                cancelInUse = true;
                GameManager.instance.SendInput("Cancel");
            }
            
        } else if (Input.GetAxisRaw("Cancel") == 0.0f) {

            if (cancelInUse) {
                cancelInUse = false;
            }
        }
    }
}
