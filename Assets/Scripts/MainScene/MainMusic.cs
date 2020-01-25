using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMusic : MonoBehaviour {

    private AudioSource audioSource;
    public AudioClip satanFightClip;
    private bool musicIsPlaying = false;

    private void Start() {
        audioSource = gameObject.GetComponent<AudioSource>();
        
    }

    void Update () {
        if (InformationPass.Level == 10 && !musicIsPlaying) {
            musicIsPlaying = true;
            audioSource.volume = 0.5f;
            audioSource.clip = satanFightClip;
            audioSource.Play();
        }

	}
}
