using System.Collections;
using UnityEngine;
using System.Linq;

// Controls player based on input received from GameManager and player state
public class PlayerController : MonoBehaviour {

    // Movement
    [SerializeField]
    private float _movementDuration = 0.5f;
    [SerializeField]
    private float _movementRotationSpeedMultiplier = 1.5f;
    [SerializeField]
    private float _cameraTurnDuration = 0.2f;
    [SerializeField]
    private float _jumpHeight = 3.0f;
    [SerializeField]
    private float _jumpDistance = 2.0f;
    [SerializeField]
    private float _jumpDuration = 0.5f;
    [SerializeField]
    private float _jumpRotationSpeedMultiplier = 2.0f;
    [SerializeField]
    private float _fallSpeed = 3.0f;
    [SerializeField]
    private float _maxFallDuration = 3.0f;
    [SerializeField]
    private float _popDuration = 0.2f;
    [SerializeField]
    private float _bounceAnimationDuration = 0.2f;

    // Player orientation (unit vectors forward, down and right)
    private Vector3[] playerAxes;

    // Camera
    private Quaternion cameraDefaultRotation;
    private Quaternion cameraCurrentRotation;
    private Vector3 cameraDefaultPosition;
    private Vector3 cameraCurrentPosition;
    private Vector3 cameraDefaultForward;
    private Vector3 cameraCurrentForward;
    private Vector3 cameraTurnAngle = new Vector3(90.0f, 90.0f, 90.0f);
    [SerializeField]
    private Vector3 _cameraOffset = new Vector3(0.0f, 0.8f, -1.0f);
    [SerializeField]
    private Vector3 _cameraIntroOffset = new Vector3(0.0f, 4.0f, -15.0f);
    [SerializeField]
    private Vector3 _cameraIntroRotation = new Vector3(35.0f, 0.0f, 180.0f);
    [SerializeField]
    private Vector3 _cameraTiltUpAngle = new Vector3(70.0f, 70.0f, 70.0f);
    [SerializeField]
    private Vector3 _cameraTiltDownAngle = new Vector3(30.0f, 30.0f, 30.0f);
    [SerializeField]
    private float _cameraTiltSpeed = 90.0f;
    [SerializeField]
    private float _cameraIntroDuration = 1.0f;
    private float tempAngleDiff;

    // Player states
    [SerializeField]
    private bool _isAlive = true;
    [SerializeField]
    private bool _isMoving = false;
    [SerializeField]
    private bool _isTryingToMove = false;
    [SerializeField]
    private bool _isTurning = false;
    [SerializeField]
    private bool _isJumping = false;
    [SerializeField]
    private bool _isFalling = false;
    [SerializeField]
    private bool _isTilted = false;
    [SerializeField]
    private bool _isInIntro = false;

    // Child gameobjects
    private GameObject ball;
    private GameObject scaler;

    // Coroutines
    private Coroutine movementCoroutine;
    private Coroutine cameraRoutine;
    private Coroutine tiltRoutine;

    // Constants (not )
    private const float playerSize = 0.4f;
    private const float cubeSize = 1.0f;

    // Audio
    private AudioSource playerAudioSource;
    [SerializeField]
    private AudioClip _playerLandAudioClip;
    [SerializeField]
    private AudioClip _playerPopAudioClip;
    [SerializeField]
    private float[] _playerAudioClipVolumes = { 0.5f, 0.5f , 0.5f};

    // Start is called before the first frame update
    void Start() {

        // Find child gameobjects
        ball = GameObject.Find("Ball");
        scaler = GameObject.Find("Scaler");

        playerAudioSource = GetComponent<AudioSource>();

        cameraDefaultRotation = Camera.main.transform.rotation;
        cameraDefaultPosition = Camera.main.transform.position;
        cameraDefaultForward = Camera.main.transform.forward;

        // Get direction player is facing
        playerAxes = FindPlayerAxes();

        // Do intro camera turn
        _isAlive = false;
        StartCoroutine(CameraIntroRoutine());
    }

    // Update is called once per frame
    void Update() {

        // This is to enable player to jump off edge (ugly but it works)
        if (_isTryingToMove) {
            _isTryingToMove = false;
        }
    }

    /********************************************************/
    public bool PlayerIsAlive() {
        return _isAlive;
    }

    /********************************************************/
    public bool PlayerIsJumping() {
        return _isJumping;
    }

    /********************************************************/
    public bool PlayerIsFalling() {
        return _isFalling;
    }

    /********************************************************/
    public bool IsInIntro() {
        return _isInIntro;
    }

    /********************************************************/
    public void HandleForwardMovement() {
        
        // Player can only move when otherwise stationary
        if (_isAlive && !_isMoving && !_isTurning && !_isJumping && !_isFalling) {

            // Determine whether player should move forward, over edge, up the wall or stay put and execute that action
            ExecuteMovement();
        }
    }

    /********************************************************/
    public void HandleTurn(bool direction) {

        // Player can only turn when otherwise stationary
        if (_isAlive && !_isTurning && !_isMoving && !_isJumping && !_isFalling) {

            // Turn, i.e. rotate camera around player
            StartCoroutine(RotateCameraRoutine(direction, -playerAxes[1], false));
        }
    }

    /********************************************************/
    public void HandleJump() {

        // Player can jump while moving or turning
        if (_isAlive && !_isJumping && !_isFalling) {

            if (_isMoving && !_isTurning) {

                // Stop moving, turning and bounce animation
                StopAllCoroutines();

                // Just to be sure, reset scale
                scaler.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            } else if (_isMoving && _isTurning) {

                // Stop moving
                StopCoroutine(movementCoroutine);
            }

            // Jump upwards (stationary or moving)
            StartCoroutine(JumpRoutine(-playerAxes[1], playerAxes[0]));
        }
    }

    /********************************************************/
    public void HandleCameraTilt(float direction) {

        // Camera can be tilted at any time (EXCEPT MAYBE WHEN TURNING?)
        if (_isAlive && !_isTurning) {
            TiltCamera(direction);
        }
    }

    /********************************************************/
    private void SetCameraFollow() {

        Vector3 camPosition = Camera.main.transform.position;
        Quaternion camRotation = Camera.main.transform.rotation;
        float angle;

        Vector3 forwardAxis = playerAxes[0];
        Vector3 downAxis = playerAxes[1];
        Vector3 rightAxis = playerAxes[2];

        // Set default offset and rotation first and get new default camera forward axis
        camPosition = transform.position + forwardAxis * _cameraOffset.z - downAxis * _cameraOffset.y + rightAxis * _cameraOffset.x;
        cameraCurrentForward = Camera.main.transform.forward;
        Camera.main.transform.rotation = cameraDefaultRotation;
        cameraDefaultForward = Camera.main.transform.forward;

        // Add tilt angle
        angle = Quaternion.Angle(camRotation, cameraDefaultRotation);

        // Determine which direction to rotate to
        if (Vector3.SignedAngle(cameraCurrentForward, cameraDefaultForward, -playerAxes[2]) < 0.0f) {
            angle = -angle;
        }

        Camera.main.transform.position = camPosition;
        Camera.main.transform.RotateAround(transform.position, playerAxes[2], angle);
    }

    /********************************************************/
    private Vector3[] FindPlayerAxes() {
        Vector3 forwardAxis = new Vector3(0, 0, 0);
        Vector3 downAxis = new Vector3(0, 0, 0);
        Vector3 rightAxis;

        // Player axes are initially (at start of level) dictated by camera direction
        // Largest absolute value is forward, second largest is downwards
        Vector3 camAxes = Camera.main.transform.forward;

        // Values and absolute values
        float[] dirArray = { camAxes.x, camAxes.y, camAxes.z };
        float[] absArray = { Mathf.Abs(camAxes.x), Mathf.Abs(camAxes.y), Mathf.Abs(camAxes.z) };

        // Index of largest absolute value
        int maxAbsIndex = absArray.ToList().IndexOf(absArray.Max());

        // Normalized forward direction vector
        forwardAxis[maxAbsIndex] = dirArray[maxAbsIndex];
        forwardAxis = forwardAxis.normalized;

        // Index of second largest absolute value
        absArray[maxAbsIndex] = 0;
        int secondMaxAbsIndex = absArray.ToList().IndexOf(absArray.Max());

        // Normalized down direction vector
        downAxis[secondMaxAbsIndex] = dirArray[secondMaxAbsIndex];
        downAxis = downAxis.normalized;

        // Normalized right direction vector
        rightAxis = Vector3.Cross(forwardAxis, downAxis).normalized;

        return new Vector3[] { forwardAxis, downAxis, rightAxis };
    }

    /********************************************************/
    private void ChangeAxes(Vector3 forwardAxis, Vector3 downAxis, Vector3 rotationAxis, bool direction) {

        Quaternion cameraNewDefaultRotation;
        Vector3 turnAngle;

        // Calculate new right axis
        Vector3 rightAxis = Vector3.Cross(forwardAxis, downAxis).normalized;

        playerAxes[0] = forwardAxis;
        playerAxes[1] = downAxis;
        playerAxes[2] = rightAxis;

        // Calculate new camera default rotation
        turnAngle = Vector3.Scale(cameraTurnAngle, rotationAxis);

        if (!direction) { turnAngle = -turnAngle; }

        cameraNewDefaultRotation = Quaternion.Euler(turnAngle) * cameraDefaultRotation;
        cameraDefaultRotation = cameraNewDefaultRotation;
    }

    /********************************************************/
    private bool CubeFound(Vector3 origin, Vector3 direction, out RaycastHit hit) {

        const float maxDistance = 1.0f;

        if (Physics.Raycast(origin, direction, out hit, maxDistance)) { return true; }

        return false;
    }

    /********************************************************/
    private void ExecuteMovement() {
        
        // Raycast position and max raycast distance
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 currentCube;

        // Directions and target
        Vector3 forwardAxis = playerAxes[0];
        Vector3 downAxis = playerAxes[1];
        Vector3 rightAxis = playerAxes[2];
        Vector3 target;

        // Find cube below player to use for other raycasts
        if (CubeFound(origin, downAxis, out hit)) {
            currentCube = hit.transform.position;
            target = CenterTargetPosition(currentCube, downAxis);

        } else {
            Debug.Log("No cube found!");
            return;
        }

        // Cube in front of player (wall)
        if (CubeFound(origin, forwardAxis, out hit)) {

            // Find and move to against wall
            target = WallTargetPosition(currentCube, downAxis, forwardAxis);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, true));

            // Rotate camera simultaneously BUT ONLY DURING SECOND MOVETOTARGETROUTINE
            cameraRoutine = StartCoroutine(RotateCameraRoutine(true, -playerAxes[2], true));

            // Find and move to center of next cube (previous forward direction is new down direction)
            target = CenterTargetPosition(hit.transform.position, forwardAxis);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, true));
            return;
        }

        // Cube in front and below of player (floor)
        if (CubeFound(currentCube, forwardAxis, out hit)) {

            // Center of current cube is in front of player
            if (Vector3.Dot(forwardAxis, target - transform.position) > 0) {
                movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, false));
                return;
            }

            // Find and move to center of next cube
            target = CenterTargetPosition(hit.transform.position, downAxis);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, false));
            return;

        // Cube to right and below, or to left and below of player
        } else if (CubeFound(currentCube, rightAxis, out hit) || CubeFound(currentCube, -rightAxis, out hit)) {

            // Center of current cube is in front of player
            if (Vector3.Dot(forwardAxis, target - transform.position) > 0) {
                movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, false));

            } else {
                // Player may want to jump forward off edge
                _isTryingToMove = true;
            }
            return;

        } else {

            // Find and move to over edge of current cube
            target = EdgeTargetPosition(currentCube, downAxis, forwardAxis);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, true));

            // Rotate camera simultaneously BUT ONLY DURING SECOND MOVETOTARGETROUTINE
            cameraRoutine = StartCoroutine(RotateCameraRoutine(false, -playerAxes[2], true));

            // Find and move to center of current cube (on other side, previous back direction is new down direction)
            target = CenterTargetPosition(currentCube, -forwardAxis);
            movementCoroutine = StartCoroutine(MoveToTargetRoutine(target, true));
            return;
        }
    }

    /********************************************************/
    private Vector3 CenterTargetPosition(Vector3 target, Vector3 downAxis) {

        // Returns center point above surface of target cube
        target += (playerSize / 2 + cubeSize / 2) * (-downAxis);

        return target;
    }

    /********************************************************/
    private Vector3 WallTargetPosition(Vector3 target, Vector3 downAxis, Vector3 forwardAxis) {

        // Returns point where player is against wall
        target += (playerSize / 2 + cubeSize / 2) * (-downAxis);
        target += (cubeSize / 2 - playerSize / 2) * forwardAxis;

        return target;
    }

    /********************************************************/
    private Vector3 EdgeTargetPosition(Vector3 target, Vector3 downAxis, Vector3 forwardAxis) {

        // Returns point where player is just over edge of current cube
        target += (playerSize / 2 + cubeSize / 2) * (-downAxis);
        target += (cubeSize / 2 + playerSize / 2) * (forwardAxis);

        return target;
    }

    /********************************************************/
    private Vector3 FallTargetPosition(Vector3 target, Vector3 downAxis, Vector3 currPosition) {

        Vector3 absDirection = new Vector3(Mathf.Abs(downAxis.x), Mathf.Abs(downAxis.y), Mathf.Abs(downAxis.z));

        // Returns point above surface of target cube straight below player
        target += (playerSize / 2 + cubeSize / 2) * (-downAxis);
        target = Vector3.Scale(target, absDirection);
        currPosition -= Vector3.Scale(currPosition, absDirection);
        target = currPosition + target;

        return target;
    }

    /********************************************************/
    private void RotatePlayer() {
        Vector3 rightAxis = playerAxes[2];
        float rotationSpeed = _movementRotationSpeedMultiplier;
        
        if (_isJumping) {
            rotationSpeed = _jumpRotationSpeedMultiplier;
        }

        rightAxis *= Time.deltaTime * rotationSpeed * 360.0f;
        ball.transform.Rotate(rightAxis.x, rightAxis.y, rightAxis.z, Space.World);
    }

    /********************************************************/
    private void TiltCamera(float direction) {

        Vector3 turnAngle;
        Quaternion targetRotation;
        float angleDiff;

        // Rotate camera around player and right axis
        cameraCurrentRotation = Camera.main.transform.rotation;
        cameraCurrentPosition = Camera.main.transform.position;
        cameraCurrentForward = Camera.main.transform.forward;

        if (cameraCurrentRotation != cameraDefaultRotation) {
            _isTilted = true;

        } else if (direction == 0.0f) {
            // No input and camera not tilted, do nothing
            return;
        }

        // Return to default rotation
        if (direction == 0.0f && _isTilted) {

            angleDiff = Quaternion.Angle(cameraCurrentRotation, cameraDefaultRotation);

            if (angleDiff == 0.0f) { return; }

            // Determine which direction to rotate to
            if (Vector3.SignedAngle(cameraCurrentForward, cameraDefaultForward, -playerAxes[2]) < 0.0f) {
                angleDiff = -angleDiff;
            }

            // If close to default rotation, snap to it
            if (Mathf.Abs(angleDiff) < 1.5f) {
                Camera.main.transform.RotateAround(transform.position, -playerAxes[2], angleDiff);
                angleDiff = 0.0f;
                _isTilted = false;
                SetCameraFollow();

            } else {
                // Otherwise keep rotating
                Camera.main.transform.RotateAround(transform.position, -playerAxes[2], Mathf.Abs(angleDiff) / angleDiff * _cameraTiltSpeed * Time.deltaTime);
            }

        } else if (direction == 1.0f || direction == -1.0f) {

            // Calculate target rotation and difference to current rotation
            if (direction == -1.0f) {
                turnAngle = _cameraTiltUpAngle;

            } else {
                turnAngle = _cameraTiltDownAngle;
            }

            turnAngle = Vector3.Scale(turnAngle, -playerAxes[2]);
            turnAngle *= direction;
            targetRotation = Quaternion.Euler(turnAngle) * cameraDefaultRotation;

            angleDiff = Quaternion.Angle(cameraCurrentRotation, targetRotation);

            if (angleDiff == 0.0f) { return; }
            
            // If close to target rotation, snap to it
            if (Mathf.Abs(angleDiff) < 1.5f) {
                Camera.main.transform.RotateAround(transform.position, -playerAxes[2], angleDiff * direction);
                angleDiff = 0.0f;

            } else {
                // Otherwise keep rotating
                Camera.main.transform.RotateAround(transform.position, -playerAxes[2], direction * _cameraTiltSpeed * Time.deltaTime);
            }
        }
    }

    /********************************************************/
    private IEnumerator MoveToTargetRoutine(Vector3 target, bool corner) {
        
        Vector3 startPos;
        float startTime;
        float movementProgress;
        float duration;
        float distance;

        // Wait for previous movement to end
        while (true) {

            if (_isMoving) {
                yield return null;

            } else {
                break;
            }
        }

        _isMoving = true;
        startPos = transform.position;
        startTime = Time.time;
        duration = _movementDuration;
        distance = Vector3.Distance(transform.position, target);

        // Shorten movement time in corners and in distances shorter than cube's length
        if (corner) {
            duration = _movementDuration / 2;

        } else if (distance < cubeSize) {
            duration = _movementDuration * (distance / cubeSize);
        }

        // Move to target
        while (_isMoving) {

            // Determine position based on elapsed time
            movementProgress = (Time.time - startTime) / duration;

            if (movementProgress >= 1) {
                transform.position = target;
                _isMoving = false;
                break;
            }

            transform.position = Vector3.Lerp(startPos, target, movementProgress);

            // Player always rotates in forward direction (ie. around right axis)
            RotatePlayer();
            yield return null;
        }
    }

    /********************************************************/
    private IEnumerator RotateCameraRoutine(bool direction, Vector3 axis, bool corner) {
        
        Quaternion initialRotation = Camera.main.transform.rotation;
        Quaternion targetRotation;
        Vector3 turnAngle;

        float startTime;
        float turnProgress;
        float tempProgress = 0.0f;
        float turnDuration = _cameraTurnDuration;
        float angle;

        // Wait for movement to end
        while (_isMoving) { yield return null; }

        _isTurning = true;

        // In case of corners, make rotation quicker
        if (corner) { turnDuration = _cameraTurnDuration / 2; }

        // Determine target camera rotation and set rotation direction to right...
        turnAngle = cameraTurnAngle;
        turnAngle = Vector3.Scale(turnAngle, axis);

        // ... or left
        if (!direction) { turnAngle = -turnAngle; }

        // Add angle to initial rotation to get target rotation
        targetRotation = Quaternion.Euler(turnAngle) * initialRotation;

        // Start timer
        startTime = Time.time;

        // Change player axes
        if (direction && !corner) {
            // Turn right (new forward is previous right, down remains)
            ChangeAxes(playerAxes[2], playerAxes[1], axis, direction);

        } else if (!direction && !corner) {
            // Turn left (new forward is previous negative right, down remains)
            ChangeAxes(-playerAxes[2], playerAxes[1], axis, direction);

        } else if (direction && corner) {
            // Go up wall (new forward is previous negative down, new down is previous forward)
            ChangeAxes(-playerAxes[1], playerAxes[0], axis, direction);

        } else if (!direction && corner) {
            // Go over edge (new forward is previous down, new down is previous negative forward)
            ChangeAxes(playerAxes[1], -playerAxes[0], axis, direction);
        }

        while (_isTurning) {
            
            // Determine rotation based on elapsed time
            turnProgress = (Time.time - startTime) / turnDuration;

            if (turnProgress >= 1) {
                Camera.main.transform.rotation = targetRotation;
                SetCameraFollow();
                _isTurning = false;
                break;
            }

            angle = (turnProgress - tempProgress) * 90.0f;

            if (!direction) { angle = -angle; }
            
            Camera.main.transform.RotateAround(transform.position, axis, angle);
            tempProgress = turnProgress;
            yield return null;
        }
    }

    /********************************************************/
    private IEnumerator JumpRoutine(Vector3 upDirection, Vector3 forwardAxis) {

        Vector3 startPos = transform.position;
        Vector3 nextPos;
        Vector3 groundTarget = startPos;

        float startTime = Time.time;
        float jumpProgress;
        float height;

        // For rotation
        Vector3 rightAxis = playerAxes[2];

        _isJumping = true;

        if (_isMoving || _isTryingToMove) {
            // Set new target
            groundTarget += forwardAxis * _jumpDistance;
            _isMoving = true;
        }

        while (_isJumping) {

            // Determine position based on elapsed time
            jumpProgress = (Time.time - startTime) / _jumpDuration;

            if (jumpProgress >= 1) {
                transform.position = groundTarget;

                _isMoving = false;
                _isJumping = false;

                if (!CubeFound(transform.position, playerAxes[1], out RaycastHit hit)) {
                    // Fall straight down
                    StartCoroutine(FallRoutine(false));

                } else {
                    // Play landing sound and bounce animation
                    playerAudioSource.PlayOneShot(_playerLandAudioClip, _playerAudioClipVolumes[0]);
                    StartCoroutine(BounceAnimationRoutine());
                }
                break;
            }

            // Position along forward axis
            nextPos = Vector3.Lerp(startPos, groundTarget, jumpProgress);

            // Position along up axis
            height = Mathf.Sin(jumpProgress * Mathf.PI) * _jumpHeight;
            nextPos = nextPos + upDirection * height;

            transform.position = nextPos;

            // Player rotates freely (at constant speed) and slightly faster while jumping
            if (_isMoving) { RotatePlayer(); }
            
            yield return null;
        }
    }

    /********************************************************/
    private IEnumerator FallRoutine(bool targetCenter) {

        // FallRoutine is always activated at end of jump or when colliding with wall during jump
        _isFalling = true;
        _isMoving = false;
        _isJumping = false;

        Vector3 downAxis = playerAxes[1];
        Vector3 target = transform.position + downAxis;
        float step;

        float startTime = Time.time;
        
        // Fall until cube is found below
        while (_isFalling) {

            if (CubeFound(transform.position, downAxis, out RaycastHit hit)) {

                if (targetCenter) {
                    target = CenterTargetPosition(hit.transform.position, downAxis);

                } else {
                    target = FallTargetPosition(hit.transform.position, downAxis, transform.position);
                }
                break;
            }
            
            // Check whether player has fallen for too long
            if (Time.time - startTime > _maxFallDuration) {
                _isAlive = false;
                _isFalling = false;
                GameManager.instance.FallOff();
                StopAllCoroutines();
            }

            // Keep falling
            target = transform.position + downAxis;
            step = _fallSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            RotatePlayer();
            yield return null;
        }

        // Fall onto cube surface
        while (_isFalling) {

            if (Vector3.Distance(transform.position, target) < 0.1f) {
                transform.position = target;
                _isFalling = false;

                // Play landing sound and bounce animation
                playerAudioSource.PlayOneShot(_playerLandAudioClip, _playerAudioClipVolumes[0]);
                StartCoroutine(BounceAnimationRoutine());
                break;
            }

            step = _fallSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            yield return null;
        }
    }

    /********************************************************/
    private IEnumerator PoppingRoutine() {

        // "Pop" by enabling/disabling meshrenderer a couple of times
        _isAlive = false;
        float startTime = Time.time;
        Renderer playerRend = ball.GetComponent<MeshRenderer>();

        // Play popping sound
        playerAudioSource.PlayOneShot(_playerPopAudioClip, _playerAudioClipVolumes[1]);

        // Flash player
        while (Time.time - startTime <= _popDuration) {
            playerRend.enabled = !playerRend.enabled;
            yield return new WaitForSeconds(_popDuration / 5);
        }

        playerRend.enabled = true;
        GameManager.instance.HitEnemy();
    }

    /********************************************************/
    private IEnumerator BounceAnimationRoutine() {

        // "Bounce" by scaling along positive up axis
        float startTime = Time.time;
        float bounceProgress;

        Vector3 playerScale = scaler.transform.localScale;
        Vector3 axis = new Vector3(Mathf.Abs(-playerAxes[1].x), Mathf.Abs(-playerAxes[1].y), Mathf.Abs(-playerAxes[1].z));
        Vector3 targetScale = playerScale - Vector3.Scale(playerScale, axis) * 0.25f;

        // Scale down
        while (Time.time - startTime <= _bounceAnimationDuration / 2) {
            bounceProgress = (Time.time - startTime) / (_bounceAnimationDuration / 2);
            scaler.transform.localScale = Vector3.Lerp(playerScale, targetScale, bounceProgress);
            yield return null;
        }
        scaler.transform.localScale = Vector3.Lerp(playerScale, targetScale, 100.0f);

        startTime = Time.time;
        targetScale = playerScale;
        playerScale = scaler.transform.localScale;

        // Scale back up
        while (Time.time - startTime <= _bounceAnimationDuration / 2) {
            bounceProgress = (Time.time - startTime) / (_bounceAnimationDuration / 2);
            scaler.transform.localScale = Vector3.Lerp(playerScale, targetScale, bounceProgress);
            yield return null;
        }
        scaler.transform.localScale = Vector3.Lerp(playerScale, targetScale, 100.0f);
    }

    /********************************************************/
    private IEnumerator CameraIntroRoutine() {

        // Do a wicked intro camera movement
        _isInIntro = true;

        Vector3 forwardAxis = playerAxes[0];
        Vector3 downAxis = playerAxes[1];
        Vector3 rightAxis = playerAxes[2];

        float startTime;
        float introProgress;

        // Start camera behind player and rotated
        Vector3 camStartPosition = transform.position + forwardAxis * _cameraIntroOffset.z - downAxis * _cameraIntroOffset.y + rightAxis * _cameraIntroOffset.x;
        Vector3 camTargetPosition = transform.position + forwardAxis * _cameraOffset.z - downAxis * _cameraOffset.y + rightAxis * _cameraOffset.x;
        Quaternion camStartRotation = Quaternion.identity;
        camStartRotation.eulerAngles = _cameraIntroRotation;

        Camera.main.transform.position = camStartPosition;
        Camera.main.transform.rotation = camStartRotation;

        // Zoom in to default third person view
        startTime = Time.time;

        // Play intro sound
        playerAudioSource.Play();

        // Lerp from starting transform to target (player's third person)
        while ((Time.time - startTime) <= _cameraIntroDuration) {
            introProgress = (Time.time - startTime) / _cameraIntroDuration;
            Camera.main.transform.position = Vector3.Lerp(camStartPosition, cameraDefaultPosition, introProgress);
            Camera.main.transform.rotation = Quaternion.Lerp(camStartRotation, cameraDefaultRotation, introProgress);
            yield return null;
        }

        // Set exact transform
        Camera.main.transform.position = cameraDefaultPosition;
        Camera.main.transform.rotation = cameraDefaultRotation;

        _isAlive = true;
        _isInIntro = false;
    }

    /********************************************************/
    private void OnTriggerEnter(Collider other) {
        
        switch (other.gameObject.tag) {

            case "Cube":
                if (_isJumping) {
                    // Stop and fall onto center of cube below
                    StopAllCoroutines();
                    StartCoroutine(FallRoutine(true));
                }
                break;

            case "Thorns":
                // Player pops
                StopAllCoroutines();
                StartCoroutine(PoppingRoutine());
                break;

            case "Exit":
                // Tell GameManager player has reached level exit
                if (GameManager.instance.ExitIsOpen()) {
                    StopAllCoroutines();
                    GameManager.instance.ReachExit();
                }
                break;

            default:
                break;
        }
    }
}
