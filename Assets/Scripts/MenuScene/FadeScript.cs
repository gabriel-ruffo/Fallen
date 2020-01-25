using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScript : MonoBehaviour {

    private float duration = 0.1f;
    private bool lowering = true;
	
	void Update () {
        Material material = GetComponent<Renderer>().material;

        Color color = material.color;
        float alpha = 0;

        if (lowering) {
            alpha = color.a - (duration * Time.deltaTime);
            if (alpha <= 0.3) {
                lowering = false;
            }
        } else {
            alpha = color.a + (duration * Time.deltaTime);
            if (alpha >= 0.9) {
                lowering = true;
            }
        }
        
        material.color = new Color(color.r, color.g, color.b, alpha);
	}
}
