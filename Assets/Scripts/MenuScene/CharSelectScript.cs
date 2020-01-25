using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharSelectScript : MonoBehaviour {

    public void GoToCharacterSelect() {
        SceneManager.LoadScene("character_select");
    }
}
