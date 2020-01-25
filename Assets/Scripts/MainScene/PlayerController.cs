using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour {
    Vector3 position;
    public float moveTime = 0.2f;
    private int moveTiles = 1;

    private float restartLevelDelay = 1f;

    private BoxCollider2D boxCollider;
    private Rigidbody2D rb2D;
    private AudioSource audioSource;
    private float inverseMoveTime;

    public AudioClip attackHitClip;
    public AudioClip attackHitEnemyClip;

    public GameObject attackAnimator;

    public GameObject validTiles;
    private GameObject upVT;
    private GameObject downVT;
    private GameObject rightVT;
    private GameObject leftVT;

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
        GameObject toInstantiate = validTiles;
        upVT = Instantiate(toInstantiate, new Vector3(position.x, position.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInstantiate, new Vector3(position.x, position.y - 1, 0.0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInstantiate, new Vector3(position.x - 1, position.y, 0.0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInstantiate, new Vector3(position.x + 1, position.y, 0.0f), Quaternion.identity) as GameObject;

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

        GameObject toInstantiate = validTiles;
        upVT = Instantiate(toInstantiate, new Vector3(new_pos.x, new_pos.y + 1, 0.0f), Quaternion.identity) as GameObject;
        downVT = Instantiate(toInstantiate, new Vector3(new_pos.x, new_pos.y - 1, 0.0f), Quaternion.identity) as GameObject;
        leftVT = Instantiate(toInstantiate, new Vector3(new_pos.x - 1, new_pos.y, 0.0f), Quaternion.identity) as GameObject;
        rightVT = Instantiate(toInstantiate, new Vector3(new_pos.x + 1, new_pos.y, 0.0f), Quaternion.identity) as GameObject;

        position = new_pos;

        RemoveInvalidTiles();

        isMoving = false;
    }

    private void RemoveInvalidTiles() {
        if (upVT.transform.position.y > 9 || CheckObstacles(upVT.transform.position.x, upVT.transform.position.y))
            Destroy(upVT);

        if (downVT.transform.position.y < 0 || CheckObstacles(downVT.transform.position.x, downVT.transform.position.y))
            Destroy(downVT);

        if (leftVT.transform.position.x < 0 || CheckObstacles(leftVT.transform.position.x, leftVT.transform.position.y))
            Destroy(leftVT);

        if (rightVT.transform.position.x > 9 || CheckObstacles(rightVT.transform.position.x, rightVT.transform.position.y))
            Destroy(rightVT);
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

    public class ComparerClass : IComparer<GameObject> {
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

    void FixedUpdate() {
        if (GameManager.instance.playersTurn && !isMoving) {
            hitSomething = false;

            boxCollider.enabled = false;
            RaycastHit2D hitup = Physics2D.Raycast(transform.position, Vector2.up, moveTiles);
            RaycastHit2D hitdown = Physics2D.Raycast(transform.position, Vector2.down, moveTiles);
            RaycastHit2D hitleft = Physics2D.Raycast(transform.position, Vector2.left, moveTiles);
            RaycastHit2D hitright = Physics2D.Raycast(transform.position, Vector2.right, moveTiles);
            boxCollider.enabled = true;

            // make a list of raycasts to iterate over
            List<RaycastHit2D> raycasts = new List<RaycastHit2D>() { hitright, hitup, hitleft, hitdown };
            // initialize a list of confirmed raycast hits
            List<GameObject> confirmed_hits = new List<GameObject>();

            if (Input.GetKeyDown(KeyCode.Space)) {
                DisplayAttackAnimation();

                // check all raycasts for a collision
                for (int i = 0; i < raycasts.Count; i++) {
                    if (raycasts[i].collider != null) {
                        confirmed_hits.Add(raycasts[i].collider.gameObject);

                        hitSomething = true;
                    }
                }

                IComparer<GameObject> myComparer = new ComparerClass();
                confirmed_hits.Sort(myComparer);
                // ArrayList is now in order of Enemy > Altar > Boulder
                // no need to check if confirmed_hits is null because of hitSomething
                if (hitSomething) {
                    if (confirmed_hits[0].tag.Equals("Satan")) {
                        Debug.Log("You tickled Satan!");
                        audioSource.PlayOneShot(attackHitEnemyClip);
                        GameManager.instance.HurtSatan();
                        ChangeTurns();
                    } else if (confirmed_hits[0].tag.Equals("Enemy")) {
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
                // A -> Left
                if (Input.GetKey(KeyCode.A) && transform.position == position && hitleft.collider == null && !((position + Vector3.left * moveTiles).x < 0)) {
                    position += Vector3.left * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }

                // D -> Right
                if (Input.GetKey(KeyCode.D) && transform.position == position && hitright.collider == null && !((position + Vector3.right * moveTiles).x > 9)) {
                    position += Vector3.right * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }

                // W -> Up
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

                // S -> Down
                if (Input.GetKey(KeyCode.S) && transform.position == position && hitdown.collider == null && !((position + Vector3.down * moveTiles).y < 0)) {
                    position += Vector3.down * moveTiles;
                    StartCoroutine(SmoothMovement(position));
                }
            }
        }
    }

    private void DisplayAttackAnimation() {
        GameObject attackAnimatorObject = Instantiate(attackAnimator, new Vector3(position.x, position.y, 0f), Quaternion.identity) as GameObject;
        Animator controller = attackAnimator.GetComponent<Animator>();
        Animation animation = controller.GetComponent<Animation>();
        Destroy(attackAnimatorObject, animation.clip.length);
        GameManager.instance.IncreaseTurnCount();
    }

    // Switch player and enemy turns as long as there are enemies
    private void ChangeTurns() {
        if (InformationPass.EnemyCount != 0) {
            GameManager.instance.playersTurn = false;
            GameManager.instance.enemysTurn = true;
        }
    }

    // Move smoothly using logic from Roguelike 2D Tutorial
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

    private void Restart() {
        Debug.Log("You beat this floor!");
        InformationPass.Level += 1;
        if (InformationPass.Level >= 11) {
            InformationPass.Level = 1;
            InformationPass.HasStarted = false;
            SceneManager.LoadScene("win_screen");
        } else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }
}
