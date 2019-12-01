using System.Collections;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

// Initializes game and handles game logic, contains all other managers and miscellaneous functions
public class GameManager : MonoBehaviour {
    
    // Managers
    public static GameManager instance = null;
    private static UIManager uiManager = null;
    private static InputManager inputManager = null;
    private static PlayerController playerController = null;

    // Level load
    AsyncOperation asyncLoadOperation;

    // Game states
    private bool inGame = false;
    private bool isPlayerAlive = false;
    private bool isPaused = false;
    private bool exitOpen = false;
    private bool exitReached = false;
    private bool gameOver = false;

    // Game stats
    private int totalScore = 0;
    private int levelScore = 0;
    private int currentLevel;
    private int levelKeysLeft;

    [SerializeField]
    private int _fallPenalty = -400;
    [SerializeField]
    private int _popPenalty = -400;

    // Constants
    private const int maxLevel = 15;

    // Prefabs
    public GameObject playerControllerPrefab;
    public GameObject levelCanvasPrefab;
    public GameObject whirlingCameraPrefab;

    // Objects
    private GameObject player;
    private GameObject mainCamera;
    private GameObject whirlCamera;

    // Level exit
    [SerializeField]
    private Material _exitClosedMat;
    [SerializeField]
    private Material _exitOpenMat;
    private MeshRenderer exitRend;

    // Level spawn and center points
    private GameObject levelSpawnPoint;
    private GameObject levelCenterPoint;

    // Audio
    private AudioSource gameManagerAudioSource;
    [SerializeField]
    private AudioClip _buttonAudioClip;
    [SerializeField]
    private AudioClip _pauseAudioClip;
    [SerializeField]
    private AudioClip _unPauseAudioClip;
    [SerializeField]
    private float _buttonAudioClipVolume = 1.25f;

    // Save file
    private const string filename = "/notkulaworldsave.dat";

    // Awake is called before Start
    void Awake() {

        // Set GameManager as singleton
        if (instance == null) {
            instance = this;

        } else if (instance != this) {
            Destroy(this.gameObject);
            return;
        }

        DontDestroyOnLoad(this.gameObject);

        // Get other managers
        uiManager = GetComponent<UIManager>();
        inputManager = GetComponent<InputManager>();

        DontDestroyOnLoad(uiManager);
        DontDestroyOnLoad(inputManager);
    }

    // Start is called before the first frame update
    void Start() {
        string scene = GetSceneName();

        if (scene == "Main Menu") {
            uiManager.EnableMenuButtons();

        } else {
            currentLevel = SceneManager.GetActiveScene().buildIndex - 1;
            LoadNextLevel();
        }

        gameManagerAudioSource = gameObject.GetComponent<AudioSource>();

        // Start playing background music
        gameManagerAudioSource.Play();
    }

    // Update is called once per frame
    void Update() {

        if (playerController != null) {
            isPlayerAlive = playerController.PlayerIsAlive();

            // MAIN PLAYER INPUT LOOP
            if (inGame) {
                // InputManager receives input and sends it to GameManager, GameManager sends it to PlayerController
                inputManager.ReceiveInput();
            }

        } else if (inGame && !isPaused) {
            Debug.Log("PlayerController is null!");
        }
    }

    /********************************************************/
    public void NewGame() {
        // Called at click of New Game button
        totalScore = 0;
        levelScore = 0;
        currentLevel = 0;

        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);

        // First level
        LoadNextLevel();
    }

    /********************************************************/
    public void LoadGame() {
        // Called at click of Load Game button
        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);

        // Fetch save data from file
        SearchSave();
    }

    /********************************************************/
    public void Save() {
        // Called at end of each level
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) {
            // Open save file
            file = File.OpenWrite(destination);

        } else {
            // or create save file
            file = File.Create(destination);
        }

        // New save data object
        Save save = new Save(currentLevel, totalScore);

        // Serialize and write save data to file
        BinaryFormatter binFor = new BinaryFormatter();
        binFor.Serialize(file, save);
        file.Close();
    }

    /********************************************************/
    public void SearchSave() {
        // Looks for save file and loads its data if it's present
        string destination = Application.persistentDataPath + filename;
        FileStream file;

        if (File.Exists(destination)) {
            // Open save file
            file = File.OpenRead(destination);

        } else {
            Debug.Log("File not found");
            return;
        }

        // Deserialize and read save data
        BinaryFormatter binFor = new BinaryFormatter();
        Save save = (Save) binFor.Deserialize(file);
        file.Close();
        currentLevel = save.level;
        totalScore = save.totalScore;

        // Populate load save UI with save data
        uiManager.ToggleSaveDataMenu(true, currentLevel, totalScore);
    }

    /********************************************************/
    public void LoadSave() {
        // Called at click of save data button (Load Game button with different text)
        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);

        // Load level
        StartCoroutine(LoadLevel(currentLevel));
    }

    /********************************************************/
    public void Back() {
        // Called at click of back button (Quit Game button with different text)
        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);

        // Show main menu
        uiManager.ToggleSaveDataMenu(false, 0, 0);
    }

    /********************************************************/
    public void QuitGame() {
        // Called at click of Quit Game button
        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);
        Application.Quit();
    }

    /********************************************************/
    public void ReturnToMainMenu() {
        // Called at click of Main Menu button in-game
        // Play button sound
        gameManagerAudioSource.PlayOneShot(_buttonAudioClip, _buttonAudioClipVolume);
        StartCoroutine(LoadMainMenu());
    }

    /********************************************************/
    IEnumerator LoadLevel(int level) {

        gameOver = false;

        asyncLoadOperation = SceneManager.LoadSceneAsync("Level" + level);

        // Wait until scene is loaded
        while (!asyncLoadOperation.isDone) {
            yield return null;
        }

        currentLevel = level;

        // Level points
        levelSpawnPoint = GameObject.FindGameObjectWithTag("Spawn");
        levelCenterPoint = GameObject.FindGameObjectWithTag("Center");

        // Instantiate player at level spawn point
        player = Instantiate(playerControllerPrefab, levelSpawnPoint.transform.position, Quaternion.identity);
        playerController = player.GetComponent<PlayerController>();

        // Instantiate Level Canvas
        Instantiate(levelCanvasPrefab);

        // Reset level score
        UpdateLevelScore(0);

        // Refresh UI connections
        uiManager.Refresh();
        StartCoroutine(uiManager.UpdateTotalScoreText(totalScore));
        StartCoroutine(uiManager.UpdateLevelScoreText(levelScore));
        uiManager.UpdateLevelNumberText(currentLevel);
        uiManager.ShowMainMenuButton(false);

        // Check amount of keys and display key UI
        levelKeysLeft = GameObject.FindGameObjectsWithTag("Key").Length;
        uiManager.SetInitialKeyImages(levelKeysLeft);

        // Instantiate Whirling Camera at level center point
        whirlCamera = Instantiate(whirlingCameraPrefab, levelCenterPoint.transform.position, Quaternion.identity);
        whirlCamera.GetComponent<Camera>().enabled = false;

        // Get hold of main camera
        mainCamera = Camera.main.gameObject;
        mainCamera.GetComponent<Camera>().enabled = true;

        // Allow player input
        inGame = true;
        isPlayerAlive = true;
        isPaused = false;
        exitOpen = false;
        exitReached = false;

        // Save game data
        Save();
    }

    /********************************************************/
    IEnumerator LoadMainMenu() {

        // Load scene
        asyncLoadOperation = SceneManager.LoadSceneAsync("Main Menu");

        // Wait until scene is loaded
        while (!asyncLoadOperation.isDone) {
            yield return null;
        }

        inGame = false;
        uiManager.EnableMenuButtons();
    }

    /********************************************************/
    public void LoadNextLevel() {

        if (currentLevel < maxLevel) {
            StartCoroutine(LoadLevel(currentLevel + 1));

        } else {
            // If final level reached, go back to menu
            StartCoroutine(LoadMainMenu());
        }
    }

    /********************************************************/
    public void UpdateLevelScore(int levelScoreAdd) {
        // Done at start of level and each time player collects collectable
        if (levelScoreAdd == 0) {
            levelScore = 0;

        } else {
            levelScore += levelScoreAdd;
            StartCoroutine(uiManager.UpdateLevelScoreText(levelScore));
        }
    }

    /********************************************************/
    public void UpdateTotalScore(int totalScoreAdd) {
        // Done only at end of level
        totalScore += totalScoreAdd;
    }

    /********************************************************/
    public void UpdateKeys() {
        // One key collected, check whether exit is open
        levelKeysLeft -= 1;
        uiManager.UpdateKeyImages(levelKeysLeft);

        if (levelKeysLeft == 0) {
            // Enable level exit and change its appearance
            exitOpen = true;
            exitRend = GameObject.Find("Level Exit").GetComponent<MeshRenderer>();
            exitRend.material = _exitOpenMat;
        }
    }

    /********************************************************/
    public void ReachExit() {

        if (inGame && exitOpen) {
            exitReached = true;

            // Display end of level UI
            if (currentLevel != 15) {
                uiManager.ShowLevelInfoText(true, "Level complete!");
                uiManager.ShowPromptText(true, "Press Jump to continue");

            } else {
                uiManager.ShowLevelInfoText(true, "Game finished!");
                uiManager.ShowPromptText(true, "Press Jump to return to menu");
            }
            

            // Switch active camera to Whirling Camera
            SwitchCamera(true);

            // Add previous level score to total score
            UpdateTotalScore(levelScore);
            UpdateLevelScore(0);
            StartCoroutine(uiManager.UpdateLevelScoreText(levelScore));
            StartCoroutine(uiManager.UpdateTotalScoreText(totalScore));
        }
    }

    /********************************************************/
    public void FallOff() {

        if (inGame) {
            isPlayerAlive = false;

            // Hide player
            playerController.gameObject.transform.Find("Scaler").Find("Ball").GetComponent<MeshRenderer>().enabled = false;

            // Display fallen off UI
            uiManager.ShowLevelInfoText(true, "Fell off!");
            uiManager.ShowPromptText(true, "Press Jump to restart level");

            // Penalty
            UpdateLevelScore(0);
            UpdateTotalScore(_fallPenalty);
            StartCoroutine(uiManager.UpdateLevelScoreText(levelScore));
            StartCoroutine(uiManager.UpdateTotalScoreText(totalScore));
            CheckTotalScore();

            // Switch active camera to Whirling Camera
            SwitchCamera(true);
        }
    }

    /********************************************************/
    public void HitEnemy() {

        if (inGame) {
            isPlayerAlive = false;

            // Hide player
            playerController.gameObject.transform.Find("Scaler").Find("Ball").GetComponent<MeshRenderer>().enabled = false;

            // Display popped UI
            uiManager.ShowLevelInfoText(true, "Popped!");
            uiManager.ShowPromptText(true, "Press Jump to restart level");

            // Penalty
            UpdateLevelScore(0);
            UpdateTotalScore(_popPenalty);
            StartCoroutine(uiManager.UpdateLevelScoreText(levelScore));
            StartCoroutine(uiManager.UpdateTotalScoreText(totalScore));
            CheckTotalScore();

            // Switch active camera to Whirling Camera
            SwitchCamera(true);
        }
    }

    /********************************************************/
    public void CheckTotalScore() {
        if (totalScore < 0) {
            uiManager.ShowPromptText(true, "Game over! Press Jump to return to main menu");
            gameOver = true;
        }
    }

    /********************************************************/
    private void SwitchCamera(bool switchToWhirl) {

        if (switchToWhirl) {
            mainCamera.GetComponent<Camera>().enabled = false;
            whirlCamera.GetComponent<Camera>().enabled = true;

            // Start rotating Whirling Camera
            whirlCamera.GetComponent<CameraRotator>().SetRotateOn(true);

        } else {
            whirlCamera.GetComponent<Camera>().enabled = false;
            mainCamera.GetComponent<Camera>().enabled = true;

            // Stop rotating Whirling Camera
            whirlCamera.GetComponent<CameraRotator>().SetRotateOn(false);
        }
    }

    /********************************************************/
    private void HandleCancelInput() {
        // Pause or unpause game and display or hide pause UI
        if (inGame && !isPaused && !playerController.IsInIntro() && !playerController.PlayerIsJumping() && !playerController.PlayerIsFalling()) {
            isPaused = true;
            uiManager.ShowLevelInfoText(true, "Paused");
            uiManager.ShowMainMenuButton(true);
            gameManagerAudioSource.PlayOneShot(_pauseAudioClip);

            // Switch active camera to Whirling Camera
            SwitchCamera(true);

        } else if (inGame && isPaused) {
            isPaused = false;
            uiManager.ShowLevelInfoText(false, "");
            uiManager.ShowMainMenuButton(false);
            gameManagerAudioSource.PlayOneShot(_unPauseAudioClip);

            // Switch active camera to Main Camera
            SwitchCamera(false);
        }
    }

    /********************************************************/
    private void HandleContinueInput() {
        
        // Either continue to next level or restart
        if (exitOpen && exitReached) {
            // Set this to not trigger level load twice
            exitOpen = false;
            LoadNextLevel();

        } else if (!isPlayerAlive) {
            isPlayerAlive = true;

            // We have to manually destroy instantiated prefabs before reloading current level
            Destroy(playerController.gameObject);
            Destroy(whirlCamera.gameObject);

            if (!gameOver) {
                StartCoroutine(LoadLevel(currentLevel));

            } else {
                StartCoroutine(LoadMainMenu());
            }
            
        }
    }

    /********************************************************/
    public void SendInput(string input) {

        // Cancel input is accepted at any time (except during level intro)
        if (input == "Cancel" && isPlayerAlive) {
            HandleCancelInput();

        // Jump input is accepted at end of level or when player has fallen off map
        } else if ((exitReached && input == "Jump") || (!isPlayerAlive && input == "Jump")) {
            HandleContinueInput();

        // Player movement input is accepted when game is not paused and exit is not reached and player is alive
        } else if (!isPaused && !exitReached && isPlayerAlive) {
                
            switch (input) {

                case "Forward":
                    playerController.HandleForwardMovement();
                    break;

                case "Jump":
                    playerController.HandleJump();
                    break;

                case "TurnRight":
                    playerController.HandleTurn(true);
                    break;

                case "TurnLeft":
                    playerController.HandleTurn(false);
                    break;

                case "TiltCameraUp":
                    playerController.HandleCameraTilt(1.0f);
                    break;

                case "TiltCameraDown":
                    playerController.HandleCameraTilt(-1.0f);
                    break;

                case "TiltReset":
                    playerController.HandleCameraTilt(0.0f);
                    break;

                default:
                    break;
            }
        }
    }

    /********************************************************/
    public string GetSceneName() {
        return SceneManager.GetActiveScene().name;
    }

    /********************************************************/
    public bool ExitIsOpen() {
        return exitOpen;
    }
}
