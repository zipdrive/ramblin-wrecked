﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeKeeper : MonoBehaviour {

    static float time;
    static float dt;
    public static bool isPaused;

    public bool pauseAtStart = false;

	// Use this for initialization
	void Start () {
        time = 0f;
        isPaused = pauseAtStart;
	}
	
	// Update is called once per frame
	void Update () {
        dt = isPaused ? 0f : Time.deltaTime;
        time += dt;
	}

    public static float GetTime()
    {
        return time;
    }

    public static float GetDeltaTime()
    {
        return dt;
    }
}
