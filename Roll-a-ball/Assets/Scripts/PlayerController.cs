using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour {

    private Rigidbody rb;

    [SerializeField]
    private float speed = 2.0f;
    private int score;

    public Text scoreText;
    public Text gameOverText;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
        score = 0;
        UpdateScore();
    }

    // Update is called once per frame
    void Update() {
        
    }

    void FixedUpdate() {
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        Vector3 force = new Vector3(moveHorizontal, 0.0f, moveVertical) * speed;

        rb.AddForce(force);
    }

    void OnTriggerEnter(Collider other) {

        if (other.gameObject.CompareTag("Collectible")) {
            Destroy(other.gameObject);
            score++;
            UpdateScore();
        }
    }

    void UpdateScore() {
        scoreText.text = "Score: " + score.ToString();

        if (score >= 14) {
            gameOverText.text = "Game Over";
        }
    }
}
