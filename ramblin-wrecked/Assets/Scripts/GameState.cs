﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour {

    public static int credits = 2;

    public static float CalculateGPA()
    {
        return Mathf.Min(4f, 5f * credits / TimeKeeper.GetTime());
    }
}
