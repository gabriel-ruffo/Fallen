using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public float turnDelay = .1f;
    public static GameManager instance = null;
    public BoardManager boardScript;
    [HideInInspector] public bool playersTurn = true;
    [HideInInspector] public bool enemysTurn = false;
    [HideInInspector] public bool hasPrayed = false;
    [HideInInspector] public bool satansTurn = false;
    [HideInInspector] public int playerTurnCount = 0;
    private GameObject statsPanel;
    private GameObject dialogPanel;
    private AudioSource audioSource;

    public AudioClip evilPrayerClip;
    public AudioClip holyPrayerClip;
    public AudioClip satanHit1Clip;
    public AudioClip satanHit2Clip;
    public AudioClip disbeliefClip;
    public AudioClip satanDeathClip;

    private bool won = false;
    public bool facingSatan = false;
    // Use this for initialization
    void Awake() {
        if (instance == null) {
            instance = this;
        } else if (instance != this)
            Destroy(gameObject);

        //DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    public void RestartLevel() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        audioSource = GetComponent<AudioSource>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    // Attack  += 1/2 of corruption
    // Defense -= 1/4 of corruption
    // Sneak   -= corruption / 400
    public void UpdateCorruption(int change) {
        InformationPass.Corruption += change;
        if (InformationPass.Corruption < 0)
            InformationPass.Corruption = 0;

        // Grab the base stats
        int attack = InformationPass.Base_Attack;
        int defense = InformationPass.Base_Defense;
        float sneak = InformationPass.Base_Sneak;
        int corruption = InformationPass.Corruption;

        attack += (int)Mathf.Floor(corruption * 0.5f);
        defense -= (int)Mathf.Floor(corruption * 0.25f);
        sneak -= corruption / 500;

        if (defense < 0) defense = 0;
        if (sneak < 0) sneak = 0;

        // Update the in-use stats
        InformationPass.Attack = attack;
        InformationPass.Defense = defense;
        InformationPass.Sneak = sneak;

        UpdateTextPanel();
    }

    public void HurtPlayer(int loss) {
        int adjusted_loss = (int)Mathf.Floor(loss - (loss * InformationPass.Defense / 100));
        Debug.Log("Lost: " + adjusted_loss + " hitpoints!");
        InformationPass.Health -= adjusted_loss;
        Debug.Log("New hitpoints: " + InformationPass.Health);
        UpdateTextPanel();
        UpdateDialogPanel(2);

        CheckIfGameOver();
    }

    public void HurtSatan() {
        int raw_damage = InformationPass.Attack;
        int multiplier = InformationPass.Corruption;
        if (multiplier > 100) multiplier = 99;

        int adjusted_damage = raw_damage - (raw_damage * multiplier / 100);
        InformationPass.SatanHealth -= adjusted_damage;

        UpdateDialogPanel(8);

        if (Random.Range(0f, 1f) <= 0.5f)
            audioSource.PlayOneShot(satanHit1Clip);
        else
            audioSource.PlayOneShot(satanHit2Clip);

        if (InformationPass.SatanHealth <= 0) {
            // TODO: rest of game end case
            audioSource.PlayOneShot(satanDeathClip);
            won = true;
            playersTurn = false;
            StartCoroutine("RemoveSatan");
        }
    }

    IEnumerator RemoveSatan() {
        yield return new WaitForSeconds(4);
        GameObject satan_reference = GameObject.FindGameObjectWithTag("Satan");
        Material satan_material = satan_reference.GetComponent<Renderer>().material;
        Color satan_color = satan_material.color;
        satan_material.color = new Color(satan_color.r, satan_color.g, satan_color.b, 0.0f);
        satan_reference.GetComponent<EdgeCollider2D>().enabled = false;
        playersTurn = true;
        UpdateDialogPanel(7);
    }

    public void EvilPrayer() {
        if (!hasPrayed) {
            Debug.Log("You prayed at an evil altar!");
            audioSource.PlayOneShot(evilPrayerClip);
            UpdateCorruption(30);
            hasPrayed = true;
            DeactivateAltars();
            UpdateDialogPanel(5);
        }
    }

    public void HolyPrayer() {
        if (!hasPrayed) {
            Debug.Log("You prayed at a holy altar!");
            audioSource.PlayOneShot(holyPrayerClip);
            InformationPass.Health += 5 * InformationPass.Level;
            UpdateCorruption(-10);
            hasPrayed = true;
            DeactivateAltars();
            UpdateDialogPanel(4);
        }
    }

    private void DeactivateAltars() {
        // get references to altars in game
        GameObject[] altars = GameObject.FindGameObjectsWithTag("Altar");
        GameObject holy_altar = null;
        GameObject evil_altar = null;

        if (altars.Length == 2) {
            if (altars[0].name.Contains("HolyAltar")) {
                holy_altar = altars[0];
                evil_altar = altars[1];
            } else {
                holy_altar = altars[1];
                evil_altar = altars[0];
            }

            Material holyAltar_material = holy_altar.GetComponent<Renderer>().material;
            Color ha_color = holyAltar_material.color;
            holyAltar_material.color = new Color(ha_color.r, ha_color.g, ha_color.b, 0.5f);

            Material evilAltar_material = evil_altar.GetComponent<Renderer>().material;
            Color ea_color = evilAltar_material.color;
            evilAltar_material.color = new Color(ea_color.r, ea_color.g, ea_color.b, 0.5f);
        } else if (altars.Length == 1) {
            if (altars[0].name.Contains("HolyAltar")) {
                holy_altar = altars[0];
            } else {
                evil_altar = altars[0];
            }

            if (holy_altar != null) {
                Material holyAltar_material = holy_altar.GetComponent<Renderer>().material;
                Color ha_color = holyAltar_material.color;
                holyAltar_material.color = new Color(ha_color.r, ha_color.g, ha_color.b, 0.5f);
            } else {
                Material evilAltar_material = evil_altar.GetComponent<Renderer>().material;
                Color ea_color = evilAltar_material.color;
                evilAltar_material.color = new Color(ea_color.r, ea_color.g, ea_color.b, 0.5f);
            }
        }
    }

    public void IncreaseTurnCount() {
        if (!facingSatan) return;
        if (!won) {
            playerTurnCount++;
            if (playerTurnCount == 3) {
                satansTurn = true;
                playersTurn = false;
                playerTurnCount = 0;
            }
        } else {
            playerTurnCount = 0;
        }
    }

    public void InitGame() {
        boardScript.SetupScene();
        UpdateTextPanel();
        UpdateDialogPanel();
    }

    public void CheckIfGameOver() {
        if (InformationPass.Health <= 0) {
            Invoke("GameOver", 0.2f);
        }
    }

    // on GameOver, send user back to menu screen
    public void GameOver() {
        playersTurn = false;
        enabled = false;
        Debug.Log("You lose!");
        InformationPass.Level = 1;
        InformationPass.HasStarted = false;
        SceneManager.LoadScene("dead_screen");
    }

    private void UpdateTextPanel() {
        statsPanel = GameObject.FindGameObjectWithTag("Panel");
        Text[] stats = statsPanel.GetComponentsInChildren<Text>();

        int defense_difference = InformationPass.Base_Defense - InformationPass.Defense;
        int attack_difference = InformationPass.Attack - InformationPass.Base_Attack;

        stats[0].text = "Level:              " + InformationPass.Level;
        stats[1].text = "Health:         " + InformationPass.Health;
        stats[2].text = "Defense:        " + InformationPass.Defense + " (" + InformationPass.Base_Defense + " - " + defense_difference + ")";
        stats[3].text = "Attack :        " + InformationPass.Attack + " (" + InformationPass.Base_Attack + " + " + attack_difference + ")";
        stats[4].text = "Corruption: " + InformationPass.Corruption;
    }

    public void UpdateDialogPanel(int dialog_index = 0) {
        string character = InformationPass.Character;
        dialogPanel = GameObject.FindGameObjectWithTag("Dialog");
        Text dialog_text = dialogPanel.GetComponentInChildren<Text>();

        // dialog_index:    0 - First entering Hell 
        //                  1 - First encountering Lesser Demon
        //                  2 - First time getting hit
        //                  3 - Killing an enemy
        //                  4 - Praying at Holy Altar
        //                  5 - Praying at Evil Altar
        //                  6 - First meeting Satan
        //                  7 - Killing Satan  
        //                  8 - Hurting Satan
        switch (character) {
            case "Knight":
                switch (dialog_index) {
                    case 0:
                        if (InformationPass.Level < 4)
                            dialog_text.text = "Knight: Time to find the wretched beast.";
                        else if (InformationPass.Level >= 4 && InformationPass.Level < 6)
                            dialog_text.text = "Knight: More enemies... I must be getting closer.";
                        else if (InformationPass.Level >= 6 && InformationPass.Level < 9)
                            dialog_text.text = "Knight: Three of them... I need to be smart about this.";
                        else if (InformationPass.Level == 9)
                            dialog_text.text = "Knight: Four now? Something tells me he's near.";
                        break;
                    case 1:
                        dialog_text.text = "Knight: A common demon. Weak when alone, but formidable in a group. Best not to be spotted.";
                        break;
                    case 2:
                        dialog_text.text = "Knight: Gah! I need to keep my distance or finish him quickly!";
                        break;
                    case 3:
                        dialog_text.text = "Knight: You were in my way.";
                        break;
                    case 4:
                        dialog_text.text = "Knight: Guard me in my quest, O Angels above.";
                        break;
                    case 5:
                        dialog_text.text = "Knight: I feel a great surge of power within me...";
                        break;
                    case 6:
                        dialog_text.text = "Knight: The great Betrayer himself. Best to stay on my toes...";
                        break;
                    case 7:
                        dialog_text.text = "Knight: It is finished.";
                        break;
                    case 8:
                        if (InformationPass.Corruption >= 50)
                            dialog_text.text = "Knight: He's feeding on my corruption..!";
                        else if (InformationPass.Corruption < 50)
                            dialog_text.text = "Knight: He's weaker to righteousness!";
                        break;
                }
                break;
            case "Angel":
                switch (dialog_index) {
                    case 0:
                        if (InformationPass.Level < 4)
                            dialog_text.text = "Angel: For righteousness, I go.";
                        else if (InformationPass.Level >= 4 && InformationPass.Level < 6)
                            dialog_text.text = "Angel: More of them? They must know a solider of God is here.";
                        else if (InformationPass.Level >= 6 && InformationPass.Level < 9)
                            dialog_text.text = "Angel: This is starting to get tricky.";
                        else if (InformationPass.Level == 9)
                            dialog_text.text = "Angel: I'm so close... I can feel his presence.";
                        break;
                    case 1:
                        dialog_text.text = "Angel: Little pest. Easily squishable or avoidable.";
                        break;
                    case 2:
                        dialog_text.text = "Angel: Foul beast! Get your hands off of me!";
                        break;
                    case 3:
                        dialog_text.text = "Angel: I am finished with you... Yet I feel worse.";
                        break;
                    case 4:
                        dialog_text.text = "Angel: Forgive me my sins, Heavenly Father.";
                        break;
                    case 5:
                        dialog_text.text = "Angel: I... I must not give in to temptation. But the power...";
                        break;
                    case 6:
                        dialog_text.text = "Angel: The one who brought shame to our name. He deserves no mercy, not even from my Lord!";
                        break;
                    case 7:
                        dialog_text.text = "Angel: May the rest of the wretched souls in this foul place find rest in his absence.";
                        break;
                    case 8:
                        if (InformationPass.Corruption >= 50)
                            dialog_text.text = "Angel: I do so little damage... I have failed you, Father.";
                        else if (InformationPass.Corruption < 50)
                            dialog_text.text = "Angel: The Holy One will always prevail!";
                        break;
                }
                break;
            case "Demon":
                switch (dialog_index) {
                    case 0:
                        if (InformationPass.Level < 4)
                            dialog_text.text = "Penitent Demon: YEAAAAAH! LET'S GO!";
                        else if (InformationPass.Level >= 4 && InformationPass.Level < 6)
                            dialog_text.text = "Penitent Demon: C'mon fellas, three's a crowd.";
                        else if (InformationPass.Level >= 6 && InformationPass.Level < 9)
                            dialog_text.text = "Penitent Demon: Someone get a stripper cause this is a PARTY!";
                        else if (InformationPass.Level == 9)
                            dialog_text.text = "Penitent Demon: I'm feelin the big man himself is riiiiiight around here...";
                        break;
                    case 1:
                        dialog_text.text = "Penitent Demon: Yo, Bob! You still owe me for that book I lent you!";
                        break;
                    case 2:
                        if (InformationPass.Level < 10)
                            dialog_text.text = "Penitent Demon: What the..? C'mon, Bob, you know I was just kidding!!";
                        if (InformationPass.Level == 10)
                            dialog_text.text = "Penitent Demon: The bright red pixels!! It buuuurrnnss!!";
                        break;
                    case 3:
                        dialog_text.text = "Penitent Demon: You just got Destroy(this.gameObject)ed!!";
                        break;
                    case 4:
                        dialog_text.text = "Penitent Demon: Ugh... I hate being all goody-goody.";
                        break;
                    case 5:
                        dialog_text.text = "Penitent Demon: UNLIMITEEED... POOWWAAHHHHH!!";
                        break;
                    case 6:
                        dialog_text.text = "Penitent Demon: Aaaahahaha... Well, I guess this is it. Time for me to take over!";
                        break;
                    case 7:
                        dialog_text.text = "Penitent Demon: Psshh, that wasn't so hard...";
                        break;
                    case 8:
                        if (InformationPass.Corruption >= 50)
                            dialog_text.text = "Penitent Demon: Gaaah... Maybe I shouldn't have horribly murderificated all those people..";
                        else if (InformationPass.Corruption < 50)
                            dialog_text.text = "Penitent Demon: I'm really doin some damage here... Maybe this whole evil thing is overrated. Only maybe.";
                        break;
                }
                break;
        }
    }

    void Update() {
        if (playersTurn)
            return;
    }
}
