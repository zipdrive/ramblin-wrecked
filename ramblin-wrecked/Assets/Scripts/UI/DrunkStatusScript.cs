﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DrunkStatusScript : MonoBehaviour {

    FixedPlayerMovement pm;
    Text text; 

	// Use this for initialization
	void Start () {
        pm = GameObject.FindGameObjectWithTag("Player").GetComponent<FixedPlayerMovement>();
        text = GetComponentInChildren<Text>();
	}
	
	// Update is called once per frame
	void Update () {
        int D = pm.curDizzyDuration < 0 ? 0 : pm.curDizzyDuration;
        text.text = System.String.Format("{0,0:D1}:{1,0:D2}", D / 60, D % 60);
	}
}
