using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class NoiseSettings : ScriptableObject {
    [Range(1, 10)]
    public int octaves = 6;
    [Range(0f, 1f)]
    public float persistance = 0.7f;

    [Range(1f, 10f)]
    public float lacunarity = 2f;

    public float distortionStrength = 0.5f;

    public Vector2 offset;

    // Constrain values on the Unity editor


}
