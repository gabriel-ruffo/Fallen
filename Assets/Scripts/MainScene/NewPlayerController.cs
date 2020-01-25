using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class NewPlayerController : MovingObject {

    public float restartLevelDelay = 1f;

    public int attack = InformationPass.Attack;
    public int corruption = InformationPass.Corruption;
    public int defense = InformationPass.Defense;
    private int knightHitPoints = InformationPass.Health;

	// Use this for initialization
    protected override void Start () {
        base.Start();
	}

    private void OnDisable() {
        // GameManager.instance.playerHitPoints = knightHitPoints;
    }
	
	// Update is called once per frame
	private void Update () {
        if (!GameManager.instance.playersTurn)
            return;

        int horizontal = 0;
        int vertical = 0;

        horizontal = (int)(Input.GetAxisRaw("Horizontal"));
        vertical = (int)(Input.GetAxisRaw("Vertical"));

        if (horizontal != 0)
            vertical = 0;

        if (horizontal != 0 || vertical != 0)
            AttemptMove<Boulder> (horizontal, vertical);
	}

    protected override void AttemptMove<T> (int xDir, int yDir) {
        base.AttemptMove<T>(xDir, yDir);

        RaycastHit2D hit;

        if (Move(xDir, yDir, out hit)) {
            // call sfx of sound manager to play move sound
        }

        CheckIfGameOver();

        GameManager.instance.playersTurn = false;
    }

    protected override void OnCantMove <T> (T component) {
        Debug.Log("Couldn't move!");
    }

    private void OnTriggerEnter2D (Collider2D other) {
        if (other.tag == "Exit") {
            Invoke("Restart", restartLevelDelay);
            enabled = false;
        } else if (other.tag == "Enemy") {
            knightHitPoints -= 10;
        }
    }

    private void CheckIfGameOver() {
        if (knightHitPoints <= 0){ 
            GameManager.instance.GameOver();
        }
    }

    private void Restart() {
        SceneManager.LoadScene(0);
    }
}
