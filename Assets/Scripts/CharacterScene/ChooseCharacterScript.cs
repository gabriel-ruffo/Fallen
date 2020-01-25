using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/*
 * Script for text fields in character selection scene. Updates
 * static class with character choice and loads the next scene.
 * 
 */
public class ChooseCharacterScript : MonoBehaviour {

    public void Highlight() {
        Text text = GetComponent<Text>();
        Outline outline = text.GetComponent<Outline>();
        outline.effectColor = new Color(190f, 0f, 0f);
    }

    public void UnHighlight() {
        Text text = GetComponent<Text>();
        Outline outline = text.GetComponent<Outline>();

        outline.effectColor = new Color(0, 0, 0);
    }

    public void ChooseCharacter() {
        Text text = GetComponent<Text>();
        string character = text.name.Substring(0, text.name.Length - 4);
        InformationPass.Character = character;
        SceneManager.LoadScene("main");
    }
}
