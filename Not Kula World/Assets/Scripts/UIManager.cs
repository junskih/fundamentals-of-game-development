using System.Collections;
using UnityEngine;
using UnityEngine.UI;

// Handles the user interface
public class UIManager : MonoBehaviour {

    // Main menu UI
    private Button newGameButton;
    private Button loadGameButton;
    private Button quitGameButton;
    private Text loadGameButtonText;
    private Text quitGameButtonText;

    // Save data menu UI
    private Button loadSaveButton;
    private Button backButton;
    private Text saveDataButtonText;

    // Level UI
    private Text levelScoreText;
    private Text totalScoreText;
    private Text levelNumberText;
    private Text levelInfoText;
    private Text promptText;
    private Image keyImage1;
    private Image keyImage2;
    private Image keyImage3;
    private Button mainMenuButton;
    
    [SerializeField]
    private Sprite _keyCollectedSprite;

    // Timing
    [SerializeField]
    private float _scoreCountDuration = 0.2f;

    /********************************************************/
    public void EnableMenuButtons() {
        // Get reference to buttons and set onclick functions
        newGameButton = GameObject.Find("New Game Button").GetComponent<Button>();
        newGameButton.onClick.AddListener(GameManager.instance.NewGame);

        loadGameButton = GameObject.Find("Load Game Button").GetComponent<Button>();
        loadGameButton.onClick.AddListener(GameManager.instance.LoadGame);

        quitGameButton = GameObject.Find("Quit Game Button").GetComponent<Button>();
        quitGameButton.onClick.AddListener(GameManager.instance.QuitGame);
    }

    /********************************************************/
    public void ToggleSaveDataMenu(bool toggle, int currentLevel, int totalScore) {

        if (toggle) {
            // Hide New Game button
            newGameButton.gameObject.SetActive(false);

            // Switch texts
            loadGameButtonText = GameObject.Find("Load Game Button Text").GetComponent<Text>();
            quitGameButtonText = GameObject.Find("Quit Game Button Text").GetComponent<Text>();
            loadGameButtonText.text = "Level: " + currentLevel + "\n" + "Score: " + totalScore;
            quitGameButtonText.text = "Back";

            // Switch onclick functions
            loadGameButton.onClick.RemoveListener(GameManager.instance.LoadGame);
            quitGameButton.onClick.RemoveListener(GameManager.instance.QuitGame);
            loadGameButton.onClick.AddListener(GameManager.instance.LoadSave);
            quitGameButton.onClick.AddListener(GameManager.instance.Back);

        } else {

            // Show New Game button
            newGameButton.gameObject.SetActive(true);

            // Switch texts
            loadGameButtonText.text = "Load Game";
            quitGameButtonText.text = "Quit Game";

            // Switch onclick functions
            loadGameButton.onClick.RemoveListener(GameManager.instance.LoadSave);
            quitGameButton.onClick.RemoveListener(GameManager.instance.Back);
            loadGameButton.onClick.AddListener(GameManager.instance.LoadGame);
            quitGameButton.onClick.AddListener(GameManager.instance.QuitGame);

        }
    }

    /********************************************************/
    public void Refresh() {
        // Find references to UI elements after level load
        levelScoreText = GameObject.Find("Level Score Text").GetComponent<Text>();
        totalScoreText = GameObject.Find("Total Score Text").GetComponent<Text>();
        levelNumberText = GameObject.Find("Level Number Text").GetComponent<Text>();
        levelInfoText = GameObject.Find("Level Info Text").GetComponent<Text>();
        promptText = GameObject.Find("Prompt Text").GetComponent<Text>();

        keyImage1 = GameObject.Find("Key Collected Image 1").GetComponent<Image>();
        keyImage2 = GameObject.Find("Key Collected Image 2").GetComponent<Image>();
        keyImage3 = GameObject.Find("Key Collected Image 3").GetComponent<Image>();

        mainMenuButton = GameObject.Find("Main Menu Button").GetComponent<Button>();
        mainMenuButton.onClick.AddListener(GameManager.instance.ReturnToMainMenu);

        // Hide non-needed UI
        ShowLevelInfoText(false, "");
        ShowPromptText(false, "");
    }

    /********************************************************/
    public IEnumerator UpdateLevelScoreText(int levelScore) {

        // Gradually increase/decrease level score text
        float startScore = int.Parse(levelScoreText.text);
        float addScore = levelScore - startScore;
        float currentScore;

        float startTime = Time.time;
        float countProgress;

        while ((Time.time - startTime) <= _scoreCountDuration) {
            countProgress = (Time.time - startTime) / _scoreCountDuration;
            currentScore = Mathf.RoundToInt(startScore + (addScore * countProgress));
            levelScoreText.text = currentScore.ToString();

            yield return null;
        }

        levelScoreText.text = levelScore.ToString();
    }

    /********************************************************/
    public IEnumerator UpdateTotalScoreText(int totalScore) {

        // Gradually increase/decrease total score text
        float startScore = int.Parse(totalScoreText.text);
        float addScore = totalScore - startScore;
        float currentScore;

        float startTime = Time.time;
        float countProgress;

        while ((Time.time - startTime) <= _scoreCountDuration) {
            countProgress = (Time.time - startTime) / _scoreCountDuration;
            currentScore = Mathf.RoundToInt(startScore + (addScore * countProgress));
            totalScoreText.text = currentScore.ToString();

            yield return null;
        }

        totalScoreText.text = totalScore.ToString();
    }

    /********************************************************/
    public void UpdateLevelNumberText(int levelNr) {
        levelNumberText.text = "Level " + levelNr;
    }

    /********************************************************/
    public void ShowLevelInfoText(bool active, string text) {
        levelInfoText.gameObject.SetActive(active);
        levelInfoText.text = text;
    }

    /********************************************************/
    public void ShowPromptText(bool active, string text) {
        promptText.gameObject.SetActive(active);
        promptText.text = text;
    }

    /********************************************************/
    public void ShowMainMenuButton(bool active) {
        mainMenuButton.gameObject.SetActive(active);
    }

    /********************************************************/
    public void SetInitialKeyImages(int levelKeysLeft) {
        // There can be at most 3 keys in a level
        Color color;
        float alpha = 1.0f;

        // How could this be done in a more elegant way?
        if (levelKeysLeft > 2) {
            color = keyImage3.color;
            color.a = alpha;
            keyImage3.color = color;
        }

        if (levelKeysLeft > 1) {
            color = keyImage2.color;
            color.a = alpha;
            keyImage2.color = color;
        }

        if (levelKeysLeft > 0) {
            color = keyImage1.color;
            color.a = alpha;
            keyImage1.color = color;
        }
    }

    /********************************************************/
    public void UpdateKeyImages(int levelKeysLeft) {

        Color color;
        float alpha = 1.0f;

        // Change image to keycollected
        if (levelKeysLeft == 2) {
            keyImage3.sprite = _keyCollectedSprite;
            color = keyImage3.color;
            color.a = alpha;
            keyImage3.color = color;
        }

        if (levelKeysLeft == 1) {
            keyImage2.sprite = _keyCollectedSprite;
            color = keyImage2.color;
            color.a = alpha;
            keyImage2.color = color;
        }

        if (levelKeysLeft == 0) {
            keyImage1.sprite = _keyCollectedSprite;
            color = keyImage1.color;
            color.a = alpha;
            keyImage1.color = color;
        }
    }
}
