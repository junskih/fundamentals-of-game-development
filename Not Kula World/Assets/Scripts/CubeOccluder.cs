using UnityEngine;

// Hides cubes so they don't block camera
public class CubeOccluder : MonoBehaviour {
    
    private MeshRenderer rend;

    /********************************************************/
    private void OnTriggerEnter(Collider other) {
        // Hide cube
        if (other.tag == "MainCamera") {
            rend = gameObject.GetComponent<MeshRenderer>();
            rend.enabled = false;
        }
    }

    /********************************************************/
    private void OnTriggerExit(Collider other) {
        // Show cube
        if (other.tag == "MainCamera") {
            rend = gameObject.GetComponent<MeshRenderer>();
            rend.enabled = true;
        }
    }
}
