using UnityEngine;

// Loads GameManager
public class GameLoader : MonoBehaviour {

    public GameObject gameManager;

    // Awake is called before Start
    void Awake() {
        if (GameManager.instance == null) {
            Instantiate(gameManager);
        }
        DontDestroyOnLoad(this.gameObject);
    }
}
