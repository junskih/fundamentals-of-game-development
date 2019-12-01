using UnityEngine;

// Rotates collectables and level exit
public class Rotator : MonoBehaviour {

    [SerializeField]
    private float _rotationSpeed = 1;
    [SerializeField]
    private Vector3 _rotationAxis;
    private Vector3 rotation;
    [SerializeField]
    private float[] _rotationRandRange = { 0.0f, 100.0f };

    private Vector3 defaultPosition;
    private Vector3 positionAxis;

    [SerializeField]
    private float _positionRange = 0.03f;
    private float positionRandInterval;

    private static float currentTime;

    // Start is called before the first frame update
    void Start() {

        Vector3 randRotation;
        _rotationAxis *= _rotationSpeed;

        defaultPosition = transform.position;
        positionAxis = transform.forward;

        if (gameObject.tag == "Key") {
            positionAxis = transform.right;
        }

        // Randomize initial rotation
        randRotation = _rotationAxis * Random.Range(_rotationRandRange[0], _rotationRandRange[1]);
        transform.Rotate(randRotation.x, randRotation.y, randRotation.z, Space.Self);

        // Randomize position change interval
        positionRandInterval = Random.Range(0.0f, 1.0f);
    }

    // Update is called once per frame
    void Update() {
        
        currentTime = Time.time * 2;

        rotation = _rotationAxis * Time.deltaTime;

        // Rotate object locally at defined speed
        transform.Rotate(rotation.x, rotation.y, rotation.z, Space.Self);

        // Determine position change based on previously randomized interval
        if (positionRandInterval < 0.33f) {
            currentTime = Mathf.Sin(currentTime);

        } else if (positionRandInterval < 0.66f) {
            currentTime = Mathf.Cos(currentTime);

        } else if (positionRandInterval <= 1.0f) {
            currentTime = -Mathf.Cos(currentTime);
        }

        transform.position = defaultPosition + positionAxis * currentTime * _positionRange;
    }
}
