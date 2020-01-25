using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AngelController : MonoBehaviour {
    Vector3 position;
    public float moveTime = 0.2f;
    private float inverseMoveTime;
    private int moveTiles = 1;

    private float restartLevelDelay = 1f;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private AudioSource audioSource;

    public AudioClip attackHitClip;
    public AudioClip attackHitEnemyClip;

    public GameObject attackAnimator;

    public GameObject validTiles;
    private GameObject upVT, upUpVT;
    private GameObject downVT, downDownVT;
    private GameObject rightVT, rightRightVT;
    private GameObject leftVT, leftLeftVT;

    private bool isMoving = false;
    private bool hitSomething = false;

    void Start() {
        position = transform.position;
        boxCollider = GetComponent<BoxCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        inverseMoveTime = 1f / moveTime;

        SetUpValidTiles();
    }

    public void SetUpValidTiles() {
        GameObject toInst = validTiles;

        upVT = Instantiate(toInst, new Vector3(position.x, position.y + 1, 0f), Quaternion.identity) as GameObject;
        upUpVT = Instantiate(toInst, new Vector3(position.x, position.y + 2, 0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInst, new Vector3(position.x, position.y - 1, 0f), Quaternion.identity) as GameObject;
        downDownVT = Instantiate(toInst, new Vector3(position.x, position.y - 2, 0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInst, new Vector3(position.x - 1, position.y, 0f), Quaternion.identity) as GameObject;
        leftLeftVT = Instantiate(toInst, new Vector3(position.x - 2, position.y, 0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInst, new Vector3(position.x + 1, position.y, 0f), Quaternion.identity) as GameObject;
        rightRightVT = Instantiate(toInst, new Vector3(position.x + 2, position.y, 0f), Quaternion.identity) as GameObject;

        RemoveInvalidTiles();
    }

    private void RemoveInvalidTiles() {
        List<GameObject> tiles = new List<GameObject>() { upVT, upUpVT, leftVT, leftLeftVT, downVT, downDownVT, rightVT, rightRightVT };

        foreach (GameObject tile in tiles) {
            Vector3 temp_pos = tile.transform.position;
            if (temp_pos.y > 9 || temp_pos.y < 0 || temp_pos.x > 9 || temp_pos.x < 0 || CheckObstacles(temp_pos.x, temp_pos.y))
                Destroy(tile);
        }
    }

    private bool CheckObstacles(float x, float y) {
        if (IsABoulder(x, y) || IsAnEnemy(x, y) || IsAnAltar(x, y))
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

    private bool IsAnEnemy(float x, float y) {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies) {
            if (enemy.transform.position.x == x && enemy.transform.position.y == y) {
                GameManager.instance.UpdateDialogPanel(1);
                return true;
            }
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

    private void UpdateValidTiles(Vector3 new_pos) {
        if (upVT) Destroy(upVT);
        if (upUpVT) Destroy(upUpVT);
        if (downVT) Destroy(downVT);
        if (downDownVT) Destroy(downDownVT);
        if (leftVT) Destroy(leftVT);
        if (leftLeftVT) Destroy(leftLeftVT);
        if (rightVT) Destroy(rightVT);
        if (rightRightVT) Destroy(rightRightVT);

        GameObject toInst = validTiles;

        upVT = Instantiate(toInst, new Vector3(new_pos.x, new_pos.y + 1, 0f), Quaternion.identity) as GameObject;
        upUpVT = Instantiate(toInst, new Vector3(new_pos.x, new_pos.y + 2, 0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInst, new Vector3(new_pos.x, new_pos.y - 1, 0f), Quaternion.identity) as GameObject;
        downDownVT = Instantiate(toInst, new Vector3(new_pos.x, new_pos.y - 2, 0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInst, new Vector3(new_pos.x - 1, new_pos.y, 0f), Quaternion.identity) as GameObject;
        leftLeftVT = Instantiate(toInst, new Vector3(new_pos.x - 2, new_pos.y, 0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInst, new Vector3(new_pos.x + 1, new_pos.y, 0f), Quaternion.identity) as GameObject;
        rightRightVT = Instantiate(toInst, new Vector3(new_pos.x + 2, new_pos.y, 0f), Quaternion.identity) as GameObject;

        position = new_pos;

        RemoveInvalidTiles();

        isMoving = false;
    }

    void FixedUpdate() {
        if (GameManager.instance.playersTurn && !isMoving) {
            hitSomething = false;

            boxCollider.enabled = false;
            RaycastHit2D hitup = Physics2D.Raycast(transform.position, Vector2.up, moveTiles);
            RaycastHit2D hitupup = Physics2D.Raycast(transform.position, Vector2.up, moveTiles * 2);
            RaycastHit2D hitdown = Physics2D.Raycast(transform.position, Vector2.down, moveTiles);
            RaycastHit2D hitdowndown = Physics2D.Raycast(transform.position, Vector2.down, moveTiles * 2);
            RaycastHit2D hitleft = Physics2D.Raycast(transform.position, Vector2.left, moveTiles);
            RaycastHit2D hitleftleft = Physics2D.Raycast(transform.position, Vector2.left, moveTiles * 2);
            RaycastHit2D hitright = Physics2D.Raycast(transform.position, Vector2.right, moveTiles);
            RaycastHit2D hitrightright = Physics2D.Raycast(transform.position, Vector2.right, moveTiles * 2);
            boxCollider.enabled = true;

            List<RaycastHit2D> raycasts = new List<RaycastHit2D> { hitup, hitupup, hitdown, hitdowndown, hitleft, hitleftleft, hitright, hitrightright };
            List<GameObject> confirmed_hits = new List<GameObject>();

            if (Input.GetKeyDown(KeyCode.Space)) {
                DisplayAttackAnimation();

                foreach (RaycastHit2D raycast in raycasts) {
                    if (raycast.collider != null) {
                        confirmed_hits.Add(raycast.collider.gameObject);
                        hitSomething = true;
                    }
                }

                IComparer<GameObject> myComparer = new ComparerClass();
                confirmed_hits.Sort(myComparer);

                if (hitSomething) {
                    if (confirmed_hits[0].tag.Equals("Satan")) {
                        Debug.Log("You tickled Satan!");
                        audioSource.PlayOneShot(attackHitEnemyClip);
                        GameManager.instance.HurtSatan();
                        ChangeTurns();
                    }
                    if (confirmed_hits[0].tag.Equals("Enemy")) {
                        Debug.Log("You hit an enemy!");
                        audioSource.PlayOneShot(attackHitEnemyClip);
                        confirmed_hits[0].GetComponent<EnemyController>().LoseHitPoints(InformationPass.Attack);
                        ChangeTurns();
                    } else if (confirmed_hits[0].tag.Equals("Altar")) {
                        if (confirmed_hits[0].name.Contains("EvilAltar")) {
                            GameManager.instance.EvilPrayer();
                        } else if (confirmed_hits[0].name.Contains("HolyAltar")) {
                            GameManager.instance.HolyPrayer();
                        }
                    } else {
                        Debug.Log("You hit a rock..?");
                        audioSource.PlayOneShot(attackHitClip);
                        ChangeTurns();
                    }
                } else {
                    audioSource.PlayOneShot(audioSource.clip);
                    ChangeTurns();
                }
            } else {
                if (Input.GetKey(KeyCode.A) && transform.position == position && hitleft.collider == null && !((position + Vector3.left * moveTiles).x < 0)) {
                    position += Vector3.left * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }

                if (Input.GetKey(KeyCode.D) && transform.position == position && hitright.collider == null && !((position + Vector3.right * moveTiles).x > 9)) {
                    position += Vector3.right * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }

                if (Input.GetKey(KeyCode.W) && transform.position == position && !((position + Vector3.up * moveTiles).y > 9)) {
                    if (hitup.collider == null) {
                        position += Vector3.up * moveTiles;
                    } else if (hitup.collider.gameObject.tag.Equals("Exit")) {
                        position += Vector3.up * moveTiles;
                        Invoke("Restart", restartLevelDelay);
                        enabled = false;
                    }
                    StartCoroutine(SmoothMovement(position));
                }

                if (Input.GetKey(KeyCode.S) && transform.position == position && hitdown.collider == null && !((position + Vector3.down * moveTiles).y < 0)) {
                    position += Vector3.down * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }
            }
        }
    }

    private void DisplayAttackAnimation() {
        GameObject attackAnimatorObject = Instantiate(attackAnimator, new Vector3(position.x, position.y, 0f), Quaternion.identity) as GameObject;
        Destroy(attackAnimatorObject, 0.4f);
        GameManager.instance.IncreaseTurnCount();
    }

    private void ChangeTurns() {
        if (InformationPass.EnemyCount != 0) {
            GameManager.instance.playersTurn = false;
            GameManager.instance.enemysTurn = true;
        }
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

        if (InformationPass.EnemyCount != 0) {
            GameManager.instance.playersTurn = false;
            GameManager.instance.enemysTurn = true;
        }

        GameManager.instance.IncreaseTurnCount();
    }

    private class ComparerClass : IComparer<GameObject> {
        public int Compare(GameObject x, GameObject y) {
            string x_string = x.tag;
            string y_string = y.tag;

            // Satan > Enemy
            if (x_string.Equals("Satan") && y_string.Equals("Enemy")) return -1;

            // Satan > Altar
            if (x_string.Equals("Satan") && y_string.Equals("Altar")) return -1;

            // Satan > Boulder
            if (x_string.Equals("Satan") && y_string.Equals("Boulder")) return -1;

            // Enemy < Satan
            if (x_string.Equals("Enemy") && y_string.Equals("Satan")) return 1;

            // Enemy > Altar
            if (x_string.Equals("Enemy") && y_string.Equals("Altar")) return -1;

            // Enemy > Boulder
            if (x_string.Equals("Enemy") && y_string.Equals("Boulder")) return -1;

            // Altar < Satan
            if (x_string.Equals("Altar") && y_string.Equals("Satan")) return 1;

            // Altar < Enemy
            if (x_string.Equals("Altar") && y_string.Equals("Enemy")) return 1;

            // Altar > Boulder
            if (x_string.Equals("Altar") && y_string.Equals("Boulder")) return -1;

            // Boulder < Satan
            if (x_string.Equals("Boulder") && y_string.Equals("Satan")) return 1;

            // Boulder < Altar
            if (x_string.Equals("Boulder") && y_string.Equals("Altar")) return 1;

            // Boulder < Enemy
            if (x_string.Equals("Boulder") && y_string.Equals("Enemy")) return 1;

            return 0;
        }
    }

    private void Restart() {
        Debug.Log("You win!");
        InformationPass.Level += 1;
        if (InformationPass.Level >= 11) {
            InformationPass.Level = 1;
            InformationPass.HasStarted = false;
            SceneManager.LoadScene("win_screen");
        } else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}
