using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class EnemyController : MonoBehaviour {

    public int damage = 20;
    public int defense = 0;
    private int enemyHitPoints = 30;

    public Vector3 position;
    public float moveTime = 0.2f;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private Animator animator;
    private AudioSource audioSource;

    public AudioClip foundYouAudioClip;
    public AudioClip lostYouAudioClip;

    public GameObject foundYouAnimator;
    public GameObject lostYouAnimator;

    private float inverseMoveTime;

    public GameObject validTiles;

    private GameObject upVT;
    private GameObject downVT;
    private GameObject rightVT;
    private GameObject leftVT;
    private GameObject uprightVT;
    private GameObject upleftVT;
    private GameObject downrightVT;
    private GameObject downleftVT;

    private bool isMoving = false;
    private bool seek = false;
    private bool hasFoundYou = false;

    // Use this for initialization
    void Start() {
        position = transform.position;

        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        inverseMoveTime = 1f / moveTime;

        SetUpValidTiles();
    }

    public void SetUpValidTiles() {
        GameObject toInstantiate = validTiles;
        upVT = Instantiate(toInstantiate, new Vector3(position.x, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInstantiate, new Vector3(position.x, position.y - 1, 0.0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y, 0.0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y, 0.0f), Quaternion.identity) as GameObject;
        uprightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        upleftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downrightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y - 1, 0.0f), Quaternion.identity) as GameObject;
        downleftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y - 1, 0.0f), Quaternion.identity) as GameObject;

        RemoveInvalidTiles();
    }

    private void UpdateValidTiles(Vector3 new_pos) {
        if (upVT)
            Destroy(upVT);
        if (downVT)
            Destroy(downVT);
        if (leftVT)
            Destroy(leftVT);
        if (rightVT)
            Destroy(rightVT);
        if (uprightVT)
            Destroy(uprightVT);
        if (upleftVT)
            Destroy(upleftVT);
        if (downrightVT)
            Destroy(downrightVT);
        if (downleftVT)
            Destroy(downleftVT);

        GameObject toInstantiate = validTiles;
        upVT = Instantiate(toInstantiate, new Vector3(new_pos.x, new_pos.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInstantiate, new Vector3(new_pos.x, new_pos.y - 1, 0.0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInstantiate, new Vector3(new_pos.x - 1, new_pos.y, 0.0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInstantiate, new Vector3(new_pos.x + 1, new_pos.y, 0.0f), Quaternion.identity) as GameObject;
        uprightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        upleftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downrightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y - 1, 0.0f), Quaternion.identity) as GameObject;
        downleftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y - 1, 0.0f), Quaternion.identity) as GameObject;

        position = new_pos;

        RemoveInvalidTiles();

        isMoving = false;
    }

    private void RemoveInvalidTiles() {
        List<GameObject> tiles = new List<GameObject>() { upVT, downVT, leftVT, rightVT, uprightVT, upleftVT, downrightVT, downleftVT };

        // check all bounds of all tiles
        for (int i = 0; i < tiles.Count; i++) {
            Vector3 temp_pos = tiles[i].transform.position;
            if (temp_pos.y > 9 || temp_pos.y < 0 || temp_pos.x > 9 || temp_pos.x < 0 || CheckObstacles(temp_pos.x, temp_pos.y))
                Destroy(tiles[i]);
        }
    }

    private bool CheckObstacles(float x, float y) {
        if (IsABoulder(x, y) || IsAPlayer(x, y) || IsAnEnemy(x, y) || IsAnAltar(x, y))
            return true;

        return false;
    }

    private bool IsABoulder(float x, float y) {
        List<GameObject> boulders = GameManager.instance.boardScript.boulderLocations;

        foreach (GameObject boulder in boulders) {
            if (boulder.transform.position.x == x && boulder.transform.position.y == y)
                return true;
        }

        return false;
    }

    private bool IsAPlayer(float x, float y) {
        // get reference to player
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        // loop over all players -- using foreach for safety
        foreach (GameObject player in players) {
            // check location passed against player's location
            if (player.transform.position.x == x && player.transform.position.y == y) {
                // hurt the player, show animation, and play noise
                GameManager.instance.HurtPlayer(damage);
                audioSource.PlayOneShot(audioSource.clip);
                animator.SetTrigger("circularAttack");

                // roll for sneak chance
                if (Random.Range(0f, 1f) <= InformationPass.Sneak) {
                    DisplayLostYouAnimation();
                    audioSource.PlayOneShot(lostYouAudioClip);
                    hasFoundYou = false;
                    seek = false;
                } else {
                    if (!hasFoundYou) {
                        DisplayFoundYouAnimation();
                        audioSource.PlayOneShot(foundYouAudioClip);
                        hasFoundYou = true;
                    }
                    seek = true;
                }
                return true;
            }
        }

        return false;
    }

    private void DisplayLostYouAnimation() {
        GameObject lostYouObject = Instantiate(lostYouAnimator, new Vector3(position.x, position.y + 1, 0f), Quaternion.identity) as GameObject;
        Animator controller = lostYouObject.GetComponent<Animator>();
        Animation animation = controller.GetComponent<Animation>();
        Destroy(lostYouObject, animation.clip.length);
    }

    private void DisplayFoundYouAnimation() {
        GameObject foundYouObject = Instantiate(foundYouAnimator, new Vector3(position.x, position.y + 1, 0f), Quaternion.identity) as GameObject;
        Animator controller = foundYouObject.GetComponent<Animator>();
        Animation animation = controller.GetComponent<Animation>();
        Destroy(foundYouObject, animation.clip.length);
    }

    private bool IsAnEnemy(float x, float y) {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) {
            if (enemy.transform.position.x == x && enemy.transform.position.y == y)
                return true;
        }

        return false;
    }

    private bool IsAnAltar(float x, float y) {
        GameObject[] altars = GameObject.FindGameObjectsWithTag("Altar");
        foreach (GameObject altar in altars) {
            if (altar.transform.position.x == x && altar.transform.position.y == y) {
                return true;
            }
        }
        return false;
    }

    void FixedUpdate() {
        if (GameManager.instance.enemysTurn && !isMoving) {
            if (seek) {
                SeekPlayer();
            } else {
                MoveRandomly();
            }
        }
    }

    private void MoveRandomly() {
        int action = Random.Range(0, 4);
        switch (action) {
            case 0:
                MoveUp();
                break;
            case 1:
                MoveDown();
                break;
            case 2:
                MoveLeft();
                break;
            case 3:
                MoveRight();
                break;
            default:
                break;
        }
    }

    private void SeekPlayer() {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        Vector3 player_position = player.transform.position;
        position = transform.position;

        // if negative, player is left of enemy
        // if negative, player is below enemy
        float x_dif = player_position.x - position.x;
        float y_diff = player_position.y - position.y;

        // player is up and to the right of enemy
        if (x_dif > 0 && y_diff > 0) {
            bool up = CanMove(0);
            bool right = CanMove(1);
            if (up && right) {
                if (Random.Range(0, 2) == 0) {
                    MoveUp();
                    return;
                } else {
                    MoveRight();
                    return;
                }
            }
            // blocked on the right
            if (up && !right) {
                MoveUp();
                return;
            }
            // blocked above
            if (!up && right) {
                MoveRight();
                return;
            }
        }

        // player is up and to the left of enemy
        if (x_dif < 0 && y_diff > 0) {
            bool up = CanMove(0);
            bool left = CanMove(3);
            if (up && left) {
                if (Random.Range(0, 2) == 0) {
                    MoveUp();
                    return;
                } else {
                    MoveLeft();
                    return;
                }
            }
            // blocked on the left
            if (up && !left) {
                MoveUp();
                return;
            }
            // blocked above
            if (!up && left) {
                MoveLeft();
                return;
            }
        }

        // player is down and right of enemy
        if (x_dif > 0 && y_diff < 0) {
            bool down = CanMove(2);
            bool right = CanMove(1);
            if (down && right) {
                if (Random.Range(0, 2) == 0) {
                    MoveDown();
                    return;
                } else {
                    MoveRight();
                    return;
                }
            }
            // blocked on the right
            if (down && !right) {
                MoveDown();
                return;
            }
            // blocked below
            if (!down && right) {
                MoveRight();
                return;
            }
        }

        // player is down and to the left of enemy
        if (x_dif < 0 && y_diff < 0) {
            bool down = CanMove(2);
            bool left = CanMove(3);
            if (down && left) {
                if (Random.Range(0, 2) == 0) {
                    MoveDown();
                    return;
                } else {
                    MoveLeft();
                    return;
                }
            }
            // blocked on the left
            if (down && !left) {
                MoveDown();
                return;
            }
            // blocked below
            if (!down && left) {
                MoveLeft();
                return;
            }
        }

        // player is directly above the enemy
        if (x_dif == 0 && y_diff > 0) {
            bool up = CanMove(0);
            if (up) {
                MoveUp();
                return;
            } else {
                MoveRandomly();
                return;
            }
        }

        // player is directly below the enemy
        if (x_dif == 0 && y_diff < 0) {
            bool down = CanMove(2);
            if (down) {
                MoveDown();
                return;
            } else {
                MoveRandomly();
                return;
            }
        }

        // player is directly to the right of the enemy
        if (x_dif > 0 && y_diff == 0) {
            bool right = CanMove(1);
            if (right) {
                MoveRight();
                return;
            } else {
                MoveRandomly();
                return;
            }
        }

        // player is directly to the left of the enemy
        if (x_dif < 0 && y_diff == 0) {
            bool left = CanMove(3);
            if (left) {
                MoveLeft();
                return;
            } else {
                MoveRandomly();
                return;
            }
        }

    }

    // 0 = up
    // 1 = right
    // 2 = down
    // 3 = left
    private bool CanMove(int direction) {
        switch (direction) {
            case 0:
                boxCollider.enabled = false;
                RaycastHit2D moveUp = Physics2D.Raycast(transform.position, Vector2.up, 1);
                boxCollider.enabled = true;

                if (moveUp.collider != null) {
                    if (moveUp.collider.gameObject.tag.Equals("Enemy")) {
                        if (transform.position == position && !((position + Vector3.up).y > 9) && !(IsGoingToHitABoulder(position + Vector3.up))) {
                            return true;
                        }
                    }
                } else if (moveUp.collider == null && transform.position == position && !((position + Vector3.up).y > 9))
                    return true;
                break;
            case 1:
                boxCollider.enabled = false;
                RaycastHit2D moveRight = Physics2D.Raycast(transform.position, Vector2.right, 1);
                boxCollider.enabled = true;

                if (moveRight.collider != null) {
                    if (moveRight.collider.gameObject.tag.Equals("Enemy")) {
                        if (transform.position == position && !((position + Vector3.right).x > 9) && !(IsGoingToHitABoulder(position + Vector3.right)))
                            return true;
                    }
                } else if (transform.position == position && moveRight.collider == null && !((position + Vector3.right).x > 9))
                    return true;
                break;
            case 2:
                boxCollider.enabled = false;
                RaycastHit2D moveDown = Physics2D.Raycast(transform.position, Vector2.down, 1);
                boxCollider.enabled = true;

                if (moveDown.collider != null) {
                    if (moveDown.collider.gameObject.tag.Equals("Enemy")) {
                        if (transform.position == position && !((position + Vector3.down).y < 0) && !(IsGoingToHitABoulder(position + Vector3.down)))
                            return true;
                    }
                } else if (transform.position == position && moveDown.collider == null && !((position + Vector3.down).y < 0))
                    return true;
                break;
            case 3:
                boxCollider.enabled = false;
                RaycastHit2D moveLeft = Physics2D.Raycast(transform.position, Vector2.left, 1);
                boxCollider.enabled = true;

                if (moveLeft.collider != null) {
                    if (moveLeft.collider.gameObject.tag.Equals("Enemy")) {
                        if (transform.position == position && !((position + Vector3.left).x < 0) && !(IsGoingToHitABoulder(position + Vector3.left)))
                            return true;
                    }
                } else if (transform.position == position && moveLeft.collider == null && !((position + Vector3.left).x < 0))
                    return true;
                break;
        }

        return false;
    }

    private void MoveUp() {
        boxCollider.enabled = false;
        RaycastHit2D hitup = Physics2D.Raycast(transform.position, Vector2.up, 1);
        boxCollider.enabled = true;

        if (hitup.collider != null) {
            if (hitup.collider.gameObject.tag.Equals("Enemy")) {
                if (transform.position == position && !((position + Vector3.up).y > 9)) {
                    position += Vector3.up;
                    StartCoroutine(SmoothMovement(position));
                }
            } else {
                StartCoroutine(SmoothMovement(position));
            }
        } else if (hitup.collider == null) {
            if (transform.position == position && !((position + Vector3.up).y > 9)) {
                position += Vector3.up;
                StartCoroutine(SmoothMovement(position));
            }
        }
    }

    private void MoveDown() {
        boxCollider.enabled = false;
        RaycastHit2D hitdown = Physics2D.Raycast(transform.position, Vector2.down, 1);
        boxCollider.enabled = true;

        if (hitdown.collider != null) {
            if (hitdown.collider.gameObject.tag.Equals("Enemy")) {
                if (transform.position == position && !((position + Vector3.down).y < 0)) {
                    position += Vector3.down;
                    StartCoroutine(SmoothMovement(position));
                }
            } else {
                StartCoroutine(SmoothMovement(position));
            }
        } else if (hitdown.collider == null) {
            if (transform.position == position && !((position + Vector3.down).y < 0)) {
                position += Vector3.down;
                StartCoroutine(SmoothMovement(position));
            }
        }
    }

    private void MoveLeft() {
        boxCollider.enabled = false;
        RaycastHit2D hitleft = Physics2D.Raycast(transform.position, Vector2.left, 1);
        boxCollider.enabled = true;

        if (hitleft.collider != null) {
            if (hitleft.collider.gameObject.tag.Equals("Enemy")) {
                if (transform.position == position && !((position + Vector3.left).x < 0)) {
                    position += Vector3.left;
                    StartCoroutine(SmoothMovement(position));
                }
            } else {
                StartCoroutine(SmoothMovement(position));
            }
        } else if (hitleft.collider == null) {
            if (transform.position == position && !((position + Vector3.left).x < 0)) {
                position += Vector3.left;
                StartCoroutine(SmoothMovement(position));
            }
        }
    }

    private void MoveRight() {
        boxCollider.enabled = false;
        RaycastHit2D hitright = Physics2D.Raycast(transform.position, Vector2.right, 1);
        boxCollider.enabled = true;

        if (hitright.collider != null) {
            if (hitright.collider.gameObject.tag.Equals("Enemy")) {
                if (transform.position == position && !((position + Vector3.right).x > 9)) {
                    position += Vector3.right;
                    StartCoroutine(SmoothMovement(position));
                }
            } else {
                StartCoroutine(SmoothMovement(position));
            }
        } else if (hitright.collider == null) {
            if (transform.position == position && !((position + Vector3.right).x > 9)) {
                position += Vector3.right;
                StartCoroutine(SmoothMovement(position));
            }
        }
    }

    private bool IsGoingToHitABoulder(Vector3 pos) {
        List<GameObject> boulders = GameManager.instance.boardScript.boulderLocations;
        foreach (GameObject boulder in boulders) {
            int boulder_x = (int)boulder.transform.position.x;
            int boulder_y = (int)boulder.transform.position.y;
            if (pos.x == boulder_x && pos.y == boulder_y)
                return true;
        }
        return false;
    }

    public void LoseHitPoints(int loss) {
        enemyHitPoints -= loss;
        CheckIfDead();
    }

    private void CheckIfDead() {
        if (enemyHitPoints <= 0) {
            InformationPass.EnemyCount--;
            GameManager.instance.UpdateCorruption(10);
            FreeAll();
        }
    }

    private void FreeAll() {
        List<GameObject> tiles = new List<GameObject>() { upVT, downVT, leftVT, rightVT, uprightVT, upleftVT, downrightVT, downleftVT };
        // free all valid move tiles
        for (int i = 0; i < tiles.Count; i++) {
            if (tiles[i])
                Destroy(tiles[i]);
        }
        GameManager.instance.UpdateDialogPanel(3);
        // free gameobject
        Destroy(this.gameObject);
    }

    protected IEnumerator SmoothMovement(Vector3 end) {
        isMoving = true;
        float sqrRemainingDistance = (transform.position - end).sqrMagnitude;

        while (sqrRemainingDistance > float.Epsilon) {
            Vector3 newPosition = Vector3.MoveTowards(rb2D.position, end, inverseMoveTime * Time.deltaTime);
            rb2D.MovePosition(newPosition);
            sqrRemainingDistance = (transform.position - end).sqrMagnitude;
            yield return null;
        }

        UpdateValidTiles(end);
        GameManager.instance.playersTurn = true;
        GameManager.instance.enemysTurn = false;
    }
}
