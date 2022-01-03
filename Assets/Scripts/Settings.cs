using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class Settings : ScriptableObject {
    [System.Serializable]
    public class BiomeColourSettings {
        //biome map
        public static Color deepOcean = new Color(45 / 255f, 75 / 255f, 155 / 255f, 1);
        public static Color ocean = new Color(65 / 255f, 105 / 255f, 255 / 255f, 1);
        public static Color lake = new Color(100 / 255f, 125 / 255f, 240 / 255f, 1);
        public static Color ice = new Color(220 / 255f, 230 / 255f, 240 / 255f, 1);
        public static Color river = new Color(100 / 255f, 125 / 255f, 240 / 255f, 1);
        public static Color snow = new Color(255 / 255f, 250 / 255f, 250 / 255f, 1);
        public static Color rocky = new Color(139 / 255f, 137 / 255f, 137 / 255f, 1);
        public static Color tundra = new Color(148 / 255f, 168 / 255f, 174 / 255f, 1);
        public static Color borealForest = new Color(100 / 255f, 170 / 255f, 140 / 255f, 1);
        public static Color grassland = new Color(155 / 255f, 188 / 255f, 122 / 255f, 1);
        public static Color forest = new Color(45 / 255f, 139 / 255f, 45 / 255f, 1);
        public static Color savanna = new Color(172 / 255f, 182 / 255f, 115 / 255f, 1);
        public static Color rainForest = new Color(200 / 255f, 210 / 255f, 24 / 255f, 1);
        public static Color beach = new Color(238 / 255f, 214 / 255f, 175 / 255f, 1);
        public static Color desert = new Color(238 / 255f, 185 / 255f, 165 / 255f, 1);
        public static Color red = new Color(255 / 255f, 12 / 255f, 13 / 255f, 1);
        public static Color unknown = new Color(0 / 255f, 12 / 255f, 13 / 255f, 1);
    }

    public int seed;

    public int mapSize = 500;
    public float noiseScale = 50f;

    [HideInInspector]
    public NoiseSettings noiseSettings;
    public HeightColor[] regions;
    public bool useFalloff = true;
    [Range(0, 1f)]
    public float seaLevel = 0f;
    [Range(0, 2f)]
    public float offsetTemperaturePeriod = 1.4f;
    public float offsetTemperatureCurveShift = 2.7f;
    [Range(-0.5f, 0.5f)]
    public float offsetTemperature = 0f;
    public int minOceanSize = 5000;

    [System.Serializable]
    public struct HeightColor {
        public string name;
        public float height;
        public Color color;
    }

}

