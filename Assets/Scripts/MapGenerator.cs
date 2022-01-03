using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;


public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, BiomeMap, FalloffMap, TemperatureMap, PrecipitationMap };
    public DrawMode drawMode;


    float zoom2;

    public Settings settings;

    public NoiseSettings noiseSettings;

    [HideInInspector]
    public bool noiseSettingsFaldout;

    public int seed;
    TMP_Text seedText;
    TMP_InputField inputSeed;

    public bool useFalloff;

    public bool autoUpdate;

    public TerrainType[] regions;

    float[,] falloffMap;
    float[,] baseNoiseMap, mountainNoiseMap, elevationMap;
    [Range(0, 1f)]
    public float seaLevel;
    BiomeGenerator biomeGenerator;
    [Range(0, 2f)]
    public float offsetTemperaturePeriod = 0.7f;
    public float offsetTemperatureCurveShift = 0f;
    [Range(-0.5f, 0.5f)]
    public float offsetTemperature = 0f;
    Color[] colorMap;
    public int minOceanSize = 5000;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(settings.mapSize);
        zoom2 = settings.mapSize / 100f;
    }

    void Start() {
        System.Random prng = new();
        seed = prng.Next();
        seedText.text = seed.ToString();
        GenerateMap();
    }

    void Update() {
        if (Input.GetButton("Generate")) {
            GenerateMapFromButton();
        }
        if (Input.GetButton("RandomSeed")) {
            System.Random prng = new();
            seed = prng.Next();
            seedText.text = seed.ToString();
            GenerateMap();
        }
        if (Input.GetButton("Quit")) {
            Application.Quit();

        }
    }
    void OnValidate() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(settings.mapSize);
    }

    public void GenerateMapFromButton() {
        if (!inputSeed.text.Equals("")) {
            try {
                seed = Convert.ToInt32(inputSeed.text);
                GenerateMap();
                seedText.text = seed.ToString();
            } catch (System.Exception) {
                Debug.LogError("Seed format not recognizable");
                throw;
            }
        }
    }

    void Initialize() {
        GenerateHeightMap();
    }

    void GenerateHeightMap() {
        baseNoiseMap = Noise.GenerateNoiseMap(settings.mapSize, settings.mapSize, seed + 66, settings.noiseScale, zoom2, 2, 0.5f,
                                                        noiseSettings.lacunarity, noiseSettings.offset, noiseSettings.distortionStrength);
        mountainNoiseMap = Noise.GenerateNoiseMap(settings.mapSize, settings.mapSize, seed, settings.noiseScale, zoom2 / 2f,
                                                noiseSettings.octaves, noiseSettings.persistance, noiseSettings.lacunarity, noiseSettings.offset, noiseSettings.distortionStrength * 4);
        colorMap = new Color[settings.mapSize * settings.mapSize];
        elevationMap = new float[settings.mapSize, settings.mapSize];

        for (int y = 0; y < settings.mapSize; y++) {
            for (int x = 0; x < settings.mapSize; x++) {
                baseNoiseMap[x, y] = (float)Math.Tanh(baseNoiseMap[x, y] * 4f - 0.1f);
                mountainNoiseMap[x, y] *= 2f;
                mountainNoiseMap[x, y] = (mountainNoiseMap[x, y] + baseNoiseMap[x, y]) / 2f;
                //elevationMap[x, y] = (float)Math.Tanh(falloffMap[x, y] + baseNoiseMap[x, y] * 2f - 0.1f);
                if (useFalloff) {
                    elevationMap[x, y] = Mathf.Clamp(mountainNoiseMap[x, y] - falloffMap[x, y], -1f, 1f);
                } else {
                    elevationMap[x, y] = Mathf.Clamp(mountainNoiseMap[x, y], -1f, 1f);
                }
            }
        }
        // fill inland oceans
        elevationMap = FillSmallOceans(elevationMap, minOceanSize, seaLevel, seaLevel + 0.1f);
        elevationMap = FillSmallOceans(elevationMap, minOceanSize, seaLevel + 0.01f, seaLevel + 0.05f);
        // fill small islands
        elevationMap = Utils.ReverseMatrix(FillSmallOceans(Utils.ReverseMatrix(elevationMap), 10, seaLevel, seaLevel + 0.05f));
    }

    void GenerateBiomeMap() {
        biomeGenerator = new BiomeGenerator(elevationMap, settings.mapSize, seed, offsetTemperature, offsetTemperatureCurveShift, offsetTemperaturePeriod, seaLevel, settings.noiseScale);
    }

    public void GenerateMap() {

        GenerateHeightMap();
        GenerateBiomeMap();

        for (int y = 0; y < settings.mapSize; y++) {
            for (int x = 0; x < settings.mapSize; x++) {
                ColorPixel(elevationMap[x, y], x, y);
            }
        }

        DisplayMap();
    }

    public float[,] FillSmallOceans(float[,] elevation, float minsize, float threshold, float target) {
        bool[,] visited = new bool[settings.mapSize, settings.mapSize];
        List<Tuple<int, int>> dirs = new();
        dirs.Add(new Tuple<int, int>(0, 1));
        dirs.Add(new Tuple<int, int>(1, 0));
        dirs.Add(new Tuple<int, int>(0, -1));
        dirs.Add(new Tuple<int, int>(-1, 0));

        for (int y = 0; y < settings.mapSize; y++) {
            for (int x = 0; x < settings.mapSize; x++) {
                if (elevation[x, y] <= threshold && !visited[x, y]) {
                    Stack<Tuple<int, int>> opened = new();
                    Stack<Tuple<int, int>> closed = new();
                    int count = 0;
                    opened.Push(new Tuple<int, int>(x, y));
                    visited[x, y] = true;
                    count += 1;

                    while (opened.Count > 0) {
                        Tuple<int, int> temp = opened.Pop();
                        closed.Push(temp);
                        foreach (var dir in dirs) {
                            int i = dir.Item1 + temp.Item1, j = dir.Item2 + temp.Item2;
                            if (i >= 0 && i < settings.mapSize && j >= 0 && j < settings.mapSize) {
                                if (!visited[i, j] && elevation[i, j] <= threshold) {
                                    opened.Push(new Tuple<int, int>(i, j));
                                    visited[i, j] = true;
                                    count += 1;
                                }
                            } else {
                                count += 2 * (int)settings.noiseScale;
                            }
                        }
                    }
                    if (count < minsize) {
                        foreach (var point in closed) {
                            elevation[point.Item1, point.Item2] = target;
                        }
                    }
                }
            }
        }
        return elevation;
    }

    public void ColorPixel(float value, int x, int y) {
        for (int i = 0; i < regions.Length; i++) {
            if (value <= regions[i].height) {
                colorMap[y * settings.mapSize + x] = regions[i].color;
                break;
            }
        }
    }

    public void DisplayMap() {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(baseNoiseMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, settings.mapSize, settings.mapSize));
        } else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(settings.mapSize)));
        } else if (drawMode == DrawMode.TemperatureMap) {
            biomeGenerator.GenerateTemperatureMap();
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.temperatureColorMap, settings.mapSize, settings.mapSize));
        } else if (drawMode == DrawMode.PrecipitationMap) {
            biomeGenerator.GenerateTemperatureMap();
            biomeGenerator.GenerateRainMap();
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.precipitationColorMap, settings.mapSize, settings.mapSize));
        } else if (drawMode == DrawMode.BiomeMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.GetBiomeMap(), settings.mapSize, settings.mapSize));
        }
    }

    public void OnNoiseSettingsUpdated() {
        if (autoUpdate) {
            GenerateBiomeMap();
            DisplayMap();
        }
    }

    public void OnBiomeSettingsUpdated() {
        if (autoUpdate) {
            GenerateMap();
        }
    }

    public int GetSeed() {
        return seed;
    }
}

[System.Serializable]
public struct TerrainType {
    public string name;
    public float height;
    public Color color;
}

