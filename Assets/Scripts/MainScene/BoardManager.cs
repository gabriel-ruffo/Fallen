using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour {
    // gonna have to change this up a bit to accept a number of different maps
    // possible a map class or enum or something with a number of preset maps
    public int columns = 10;
    public int rows = 10;
    public GameObject exit;
    public GameObject boulder;
    public GameObject[] floorTiles;
    public GameObject enemy1;
    public List<GameObject> boulderLocations;
    public GameObject knight;
    public GameObject angel;
    public GameObject pendemon;
    public GameObject blockingWall;
    public GameObject holyAltar;
    public GameObject evilAltar;

    public GameObject satan;

    private Transform boardHolder;
    private List<Vector3> gridPositions = new List<Vector3>();

    // initializes list
    void InitialiseList() {
        gridPositions.Clear();
        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++) {
                gridPositions.Add(new Vector3(x, y, 0.0f));
            }
        }
    }

    // Places background tiles and outer blocking walls
    void BoardSetup() {
        boardHolder = new GameObject("Board").transform;

        // set up a base of random floor tiles
        for (int x = 0; x < columns; x++) {
            for (int y = 0; y < rows; y++) {
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0.0f), Quaternion.identity) as GameObject;
                instance.transform.SetParent(boardHolder);
            }
        }

        // set up the blocking walls
        int left_up = -1;
        while (left_up < 10) {
            GameObject toInstantiate = blockingWall;
            GameObject instance = Instantiate(toInstantiate, new Vector3(-1, left_up++, 0.0f), Quaternion.identity) as GameObject;
            instance.transform.SetParent(boardHolder);
        }

        // set up the blocking walls
        int bottom_right = -1;
        while (bottom_right < 10) {
            GameObject instance = Instantiate(blockingWall, new Vector3(bottom_right++, -1, 0.0f), Quaternion.identity) as GameObject;
            instance.transform.SetParent(boardHolder);
        }
    }

    private void InitializeBoulders() {
        if (InformationPass.Level == 10) {
            boulderLocations.Add(Instantiate(boulder, new Vector3(0, 5, 0f), Quaternion.identity) as GameObject);
            boulderLocations.Add(Instantiate(boulder, new Vector3(1, 4, 0f), Quaternion.identity) as GameObject);
            boulderLocations.Add(Instantiate(boulder, new Vector3(8, 4, 0f), Quaternion.identity) as GameObject);
            boulderLocations.Add(Instantiate(boulder, new Vector3(9, 4, 0f), Quaternion.identity) as GameObject);
            boulderLocations.Add(Instantiate(boulder, new Vector3(7, 3, 0f), Quaternion.identity) as GameObject);
            return;
        }

        int boulders_count = Random.Range(1, 6);
        Debug.Log("Boulder count: " + boulders_count);

        int random_x, random_y;

        for (int i = 0; i < boulders_count; i++) {
            random_x = Random.Range(1, 9);
            random_y = Random.Range(1, 9);

            while (!ValidLocation(random_x, random_y)) {
                random_x = Random.Range(1, 9);
                random_y = Random.Range(1, 9);
            }

            boulderLocations.Add(Instantiate(boulder, new Vector3(random_x, random_y, 0f), Quaternion.identity) as GameObject);
        }
    }

    private void InitializeAltars(int level_seed) {
        if (level_seed == 10) {
            Instantiate(holyAltar, new Vector3(8, 1, 0f), Quaternion.identity);
            Instantiate(evilAltar, new Vector3(1, 1, 0f), Quaternion.identity);
            return;
        }

        float holyAltarChance = level_seed * 0.12f;
        float evilAltarChance = level_seed * 0.15f;

        Debug.Log("Holy Altar Chance: " + holyAltarChance);
        Debug.Log("Evil Altar Chance: " + evilAltarChance);

        int random_x;
        int random_y;
        Vector3 probe_pos = new Vector3(-999, -999, 0f);

        if (Random.Range(0.0f, 1.0f) <= holyAltarChance) {
            Debug.Log("Holy Altar chance passed!");
            random_x = Random.Range(1, 9);
            random_y = Random.Range(5, 10);
            while ((random_x + 1 == 9 && random_y + 1 == 9) || random_x + 1 == 9) {
                random_x = Random.Range(1, 9);
                random_y = Random.Range(5, 10);
            }

            probe_pos = new Vector3(random_x, random_y, 0f);

            Instantiate(holyAltar, probe_pos, Quaternion.identity);
        }

        if (Random.Range(0.0f, 1.0f) <= evilAltarChance) {
            Debug.Log("Evil Altar chance passed!");
            random_x = Random.Range(1, 9);
            random_y = Random.Range(5, 10);

            // if not null, a holy altar has been placed
            if (probe_pos.x != -999) {
                int check_x = (int)probe_pos.x;
                int check_y = (int)probe_pos.y;

                // remake random positions until in valid location
                while ((random_x == check_x && random_y == check_y)
                    || (random_x == check_x - 1 && random_y == check_y)
                    || (random_x == check_x + 1 && random_y == check_y)
                    || (random_x + 1 == 9 && random_y + 1 == 9)
                    || random_x + 1 == 9) {
                    random_x = Random.Range(1, 9);
                    random_y = Random.Range(5, 10);
                }
            }

            Instantiate(evilAltar, new Vector3(random_x, random_y, 0f), Quaternion.identity);
        }
    }

    private void InitializeEnemies(int level_seed) {
        int enemies_count = 1 + (int)(0.4 * (level_seed - 1));
        if (enemies_count == 0) enemies_count = 1;
        Debug.Log("Enemy Count: " + enemies_count);

        int random_x, random_y;

        for (int i = 0; i < enemies_count; i++) {
            random_x = Random.Range(1, 9);
            random_y = Random.Range(1, 9);

            while (!ValidLocation(random_x, random_y)) {
                random_x = Random.Range(1, 9);
                random_y = Random.Range(1, 9);
            }

            Instantiate(enemy1, new Vector3(random_x, random_y, 0f), Quaternion.identity);
        }

        InformationPass.EnemyCount = enemies_count;
    }

    private bool ValidLocation(int x, int y) {
        GameObject[] altars = GameObject.FindGameObjectsWithTag("Altar");
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject[] boulders = GameObject.FindGameObjectsWithTag("Boulder");

        int check_x;
        int check_y;

        if (altars.Length > 0) {
            foreach (GameObject altar in altars) {
                check_x = (int)altar.transform.position.x;
                check_y = (int)altar.transform.position.y;

                if ((x == check_x && y == check_y) || (x == check_x - 1 && y == check_y) || (x == check_x + 1 && y == check_y))
                    return false;
            }
        }

        if (player != null) {
            check_x = (int)player.transform.position.x;
            check_y = (int)player.transform.position.y;

            if (x == check_x && y == check_y)
                return false;
        }

        if (enemies.Length > 0) {
            foreach (GameObject enemy in enemies) {
                check_x = (int)enemy.transform.position.x;
                check_y = (int)enemy.transform.position.y;

                if (x == check_x && y == check_y)
                    return false;
            }
        }

        if (boulders.Length > 0) {
            foreach (GameObject boulder in boulders) {
                check_x = (int)boulder.transform.position.x;
                check_y = (int)boulder.transform.position.y;

                if (x == check_x && y == check_y)
                    return false;
            }
        }

        return true;
    }

    private void InitializeSatan() {
        Instantiate(satan, new Vector3(4.5f, 7, 0f), Quaternion.identity);
    }

    // Based on user's choice in character selection scene, initializes that
    // character type to the board at (0,0).
    private void InitializePlayer() {
        string character = InformationPass.Character;
        switch (character) {
            case "Knight":
                Instantiate(knight, new Vector3(0, 0, 0.0f), Quaternion.identity);
                if (!InformationPass.HasStarted) {
                    // 100, 20, 30, 20, 0.1f
                    SetInformation(100, 20, 30, 20, 0.1f);
                    InformationPass.HasStarted = true;
                }
                GameManager.instance.UpdateCorruption(0);
                break;
            case "Angel":
                Instantiate(angel, new Vector3(0, 0, 0.0f), Quaternion.identity);
                if (!InformationPass.HasStarted) {
                    // 30, 10, 10, 0, 0.2f
                    SetInformation(30, 10, 10, 0, 0.2f);
                    InformationPass.HasStarted = true;
                }
                GameManager.instance.UpdateCorruption(0);
                break;
            case "Demon":
                Instantiate(pendemon, new Vector3(0, 0, 0.0f), Quaternion.identity);
                if (!InformationPass.HasStarted) {
                    SetInformation(50, 20, 60, 50, 0.5f);
                    InformationPass.HasStarted = true;
                }
                GameManager.instance.UpdateCorruption(0);
                break;
            default: // default when starting straight from the main scene; mainly for debugging purposes
                Instantiate(knight, new Vector3(0, 0, 0.0f), Quaternion.identity);
                SetInformation(1000, 20, 100, 98, 0.1f);
                GameManager.instance.UpdateCorruption(0);
                break;
        }
    }

    // Private helper method to declutter switch-case statement.
    private void SetInformation(int health, int defense, int attack, int corruption, float sneak) {
        InformationPass.Health = health;
        InformationPass.Base_Defense = defense;
        InformationPass.Defense = defense;
        InformationPass.Base_Attack = attack;
        InformationPass.Attack = attack;
        InformationPass.Corruption = corruption;
        InformationPass.Base_Sneak = sneak;
        InformationPass.Sneak = sneak;
    }

    // Call all helper methods to initialize board items.
    public void SetupScene() {
        int level = InformationPass.Level;
        Debug.Log("Level: " + level);
        // check when level == 10: load satan level
        // set up the background tiles
        BoardSetup();
        // dont know why this is still here
        InitialiseList();
        // place the player in a known free space
        InitializePlayer();
        // randomly place altars given percentage based on level
        InitializeAltars(level);
        // randomly place enemies in free spaces, number based on level
        if (level < 10)
            InitializeEnemies(level);
        // place Satan
        if (level == 10) {
            InformationPass.EnemyCount = 0;
            GameManager.instance.facingSatan = true;
            GameManager.instance.UpdateDialogPanel(6);
            InitializeSatan();
        }
        // randomly place 1 to 7 boulders in free spaces
        InitializeBoulders();
        // instantiate the exit on a free top row space
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0.0f), Quaternion.identity);
    }
}
