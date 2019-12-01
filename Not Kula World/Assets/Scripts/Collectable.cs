using UnityEngine;

// Destroyes collectables, plays a sound and adds to score
public class Collectable : MonoBehaviour {

    [SerializeField]
    private int _points = 200;
    [SerializeField]
    private float _audioClipVolume = 0.5f;
    private AudioSource collectableAudioSource;

    // Start is called before the first frame update
    void Start() {
        collectableAudioSource = gameObject.GetComponent<AudioSource>();
    }

    /********************************************************/
    private void OnTriggerEnter(Collider other) {

        if (other.gameObject.tag == "Player" && gameObject.tag == "Key") {
            GameManager.instance.UpdateKeys();
        }

        if (other.gameObject.tag == "Player") {
            GameManager.instance.UpdateLevelScore(_points);
            AudioSource.PlayClipAtPoint(collectableAudioSource.clip, transform.position, _audioClipVolume);
            Destroy(gameObject);
        }
    }
}
