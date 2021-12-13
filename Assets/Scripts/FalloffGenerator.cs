using System;
using System.Collections;
using UnityEngine;

public class FalloffGenerator : MonoBehaviour {
    public static float[,] GenerateFalloffMap(int size) {
        float[,] map = new float[size, size];
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                float x = (i / (float)size) * 2f - 1f;
                float y = (j / (float)size) * 2f - 1f;
                //float r = Mathf.Sqrt(Mathf.Pow(i - 0.5f * size / 50f, 2) + 0.5f * Mathf.Pow(j - 0.5f * size / 50f, 2)) / 5f;
                //map[i, j] = (float)Math.Tanh((1f - r) * 5f);
                //square falloff map:
                //float value = Mathf.Max(Mathf.Abs(x), Mathf.Abs(y));
                //squircle falloff map:
                float value = Mathf.Sqrt(Mathf.Pow(x, 4) + Mathf.Pow(y, 4));
                map[i, j] = Evaluate(value);
            }
        }
        return map;
    }

    static float Evaluate(float value) {
        float a = 3f;
        float b = 2.7f;
        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}