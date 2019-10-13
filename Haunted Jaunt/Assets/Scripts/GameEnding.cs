using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEnding : MonoBehaviour {

    public float fadeDuration = 1.0f;
    public float displayDuration = 1.0f;

    public GameObject player;

    bool isPlayerAtExit;
    bool isPlayerCaught;

    public CanvasGroup exitBackgroundImageCanvasGroup;
    public CanvasGroup caughtBackgroundImageCanvasGroup;

    float timer;

    public AudioSource exitAudio;
    public AudioSource caughtAudio;
    bool hasAudioPlayed;

    // Start is called before the first frame update
    void Start() {
        
    }

    // Update is called once per frame
    void Update() {
        if (isPlayerAtExit) {
            EndLevel(exitBackgroundImageCanvasGroup, false, exitAudio);

        } else if (isPlayerCaught) {
            EndLevel(caughtBackgroundImageCanvasGroup, true, caughtAudio);
        }
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject == player) {
            isPlayerAtExit = true;
        }
    }

    public void CaughtPlayer() {
        isPlayerCaught = true;
    }

    void EndLevel(CanvasGroup imageCanvasGroup, bool restart, AudioSource audioSource) {
        if (!hasAudioPlayed) {
            audioSource.Play();
            hasAudioPlayed = true;
        }

        timer += Time.deltaTime;
        imageCanvasGroup.alpha = timer / fadeDuration;

        if (timer > fadeDuration + displayDuration) {

            if (restart) {
                SceneManager.LoadScene(0);

            } else {
                Application.Quit();
            }
            
        }
    }
}
