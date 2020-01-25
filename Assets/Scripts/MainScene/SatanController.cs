using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SatanController : MonoBehaviour {
    private AudioSource audioSource;
    public AudioClip basicAttackClip;
    public AudioClip bigAttackClip;
    public AudioClip thisIsGodClip;
    public AudioClip getOutClip;

    public GameObject basicAttackAnimator;
    public GameObject bigAttackAnimator;
    public GameObject nearAttackAnimator;

    private EdgeCollider2D edgeCollider;
    private Vector3 position;

    void Start() {
        audioSource = gameObject.GetComponent<AudioSource>();
        edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
        position = transform.position;
    }

    void FixedUpdate() {
        if (GameManager.instance.satansTurn) {
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player.transform.position.y == 4 && (player.transform.position.x >= 2 && player.transform.position.x <= 7)) {
                audioSource.PlayOneShot(getOutClip);
                DisplayAttackAnimation(2);
                GameManager.instance.HurtPlayer(30);
            } else if (player.transform.position.y >= 3) {
                audioSource.PlayOneShot(bigAttackClip);
                DisplayAttackAnimation(1);
                CheckIfHit(player.transform.position, 1);
            } else {
                audioSource.PlayOneShot(basicAttackClip);
                DisplayAttackAnimation(0);
                CheckIfHit(player.transform.position, 0);
            }

            GameManager.instance.satansTurn = false;
            GameManager.instance.playersTurn = true;
        }
    }

    private void CheckIfHit(Vector3 players_position, int attack_type) {
        switch (attack_type) {
            case 0:
                if ((players_position.x >= 4 && players_position.x <= 5) &&
                    (players_position.y >= 1 && players_position.y <= 5)) {
                    GameManager.instance.HurtPlayer(20);
                }
                break;
            case 1:
                if ((players_position.x >= 3 && players_position.x <= 6) &&
                    (players_position.y >= 1 && players_position.y <= 5)) {
                    GameManager.instance.HurtPlayer(30);
                }
                break;
        }
    }

    private void DisplayAttackAnimation(int type) {
        switch (type) {
            case 0:
                GameObject basicAttackAnimatorObject = Instantiate(basicAttackAnimator, new Vector3(position.x, position.y - 1.5f, 0f), Quaternion.identity) as GameObject;
                Animator controller = basicAttackAnimator.GetComponent<Animator>();
                Animation animation = controller.GetComponent<Animation>();
                Destroy(basicAttackAnimatorObject, animation.clip.length);
                break;
            case 1:
                GameObject bigAttackAnimatorObject = Instantiate(bigAttackAnimator, new Vector3(position.x, position.y - 1.5f, 0f), Quaternion.identity) as GameObject;
                Animator controller2 = bigAttackAnimator.GetComponent<Animator>();
                Animation animation2 = controller2.GetComponent<Animation>();
                Destroy(bigAttackAnimatorObject, animation2.clip.length);

                if (Random.Range(0f, 1f) <= 0.15f)
                    audioSource.PlayOneShot(thisIsGodClip);
                break;
            case 2:
                GameObject nearAttackAnimatorObject = Instantiate(nearAttackAnimator, new Vector3(4.5f, 6.5f, 0f), Quaternion.identity) as GameObject;
                Animator controller3 = nearAttackAnimatorObject.GetComponent<Animator>();
                Animation animation3 = controller3.GetComponent<Animation>();
                Destroy(nearAttackAnimatorObject, animation3.clip.length);
                break;
        }
    }
}
