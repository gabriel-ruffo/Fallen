using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* 
 * Static class that is passed around scripts that update/refresh
 * player stats.
 */
public static class InformationPass {

    // base_* fields contain the starting stats from which
    // adjusted values are calculated using corruption
    private static string character_selected;
    private static int health, defense, base_defense, attack, base_attack, corruption;
    private static float sneak, base_sneak;
    private static int enemyCount;
    private static bool hasStarted;
    private static int level = 1;
    private static int satanHealth = 500;

    // Character type between Knight, Angel, and Penitent Demon
    public static string Character {
        get {
            return character_selected;
        }
        set {
            character_selected = value;
        }
    }

    // Health stat
    public static int Health {
        get {
            return health;
        }
        set {
            health = value;
        }
    }

    // Base Defense stat
    public static int Base_Defense {
        get {
            return base_defense;
        }
        set {
            base_defense = value;
        }
    }

    // Defense stat
    public static int Defense {
        get {
            return defense;
        }
        set {
            defense = value;
        }
    }

    // Base Attack stat
    public static int Base_Attack {
        get {
            return base_attack;
        }
        set {
            base_attack = value;
        }
    }

    // Attack stat
    public static int Attack {
        get {
            return attack;
        }
        set {
            attack = value;
        }
    }

    // Corruption stat
    public static int Corruption {
        get {
            return corruption;
        }
        set {
            corruption = value;
        }
    }

    // Base Sneak stat
    public static float Base_Sneak {
        get {
            return base_sneak;
        }
        set {
            base_sneak = value;
        }
    }

    // Sneak stat
    public static float Sneak {
        get {
            return sneak;
        }
        set {
            sneak = value;
        }
    }

    // Number of enemies on the field
    public static int EnemyCount {
        get {
            return enemyCount;
        }
        set {
            enemyCount = value;
        }
    }

    // Whether or not the player has started playing
    public static bool HasStarted {
        get {
            return hasStarted;
        }
        set {
            hasStarted = value;
        }
    }

    // The current Level of the game
    public static int Level {
        get {
            return level;
        }
        set {
            level = value;
        }
    }

    // Satan's health
    public static int SatanHealth {
        get {
            return satanHealth;
        }
        set {
            satanHealth = value;
        }
    }
}
