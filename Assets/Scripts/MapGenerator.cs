using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, BiomeMap, FalloffMap, TemperatureMap, PrecipitationMap };
    public DrawMode drawMode;

    public int mapSize = 500;
    public float noiseScale = 0f;
    float zoom2;

    public int octaves = 4;
    [Range(0, 1)]
    public float persistance = 0.5f;
    public float lacunarity = 2f;

    public float distortionStrength = 0.5f;

    public int seed;
    public TMP_Text seedText;
    public TMP_InputField inputSeed;
    public Vector2 offset;

    public int minOceanSize;

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

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize);
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

    public void GenerateMap() {
        zoom2 = mapSize / 100f;
        baseNoiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed + 66, noiseScale, zoom2, 2, 0.5f, lacunarity, offset, distortionStrength);
        mountainNoiseMap = Noise.GenerateNoiseMap(mapSize, mapSize, seed, noiseScale, zoom2 / 2f, octaves, persistance, lacunarity, offset, distortionStrength * 4);
        colorMap = new Color[mapSize * mapSize];
        elevationMap = new float[mapSize, mapSize];

        if (useFalloff) {
            for (int y = 0; y < mapSize; y++) {
                for (int x = 0; x < mapSize; x++) {
                    baseNoiseMap[x, y] = (float)Math.Tanh(baseNoiseMap[x, y] * 4f - 0.1f);
                    mountainNoiseMap[x, y] *= 2f;
                    mountainNoiseMap[x, y] = (mountainNoiseMap[x, y] + baseNoiseMap[x, y]) / 2f;
                    //elevationMap[x, y] = (float)Math.Tanh(falloffMap[x, y] + baseNoiseMap[x, y] * 2f - 0.1f);
                    elevationMap[x, y] = Mathf.Clamp(mountainNoiseMap[x, y] - falloffMap[x, y], -1f, 1f);
                }
            }
        } else {
            elevationMap = baseNoiseMap;
        }
        elevationMap = FillSmallOceans(elevationMap, (float)Math.Pow(2 * noiseScale, 2), seaLevel, seaLevel + 0.1f);
        elevationMap = FillSmallOceans(elevationMap, (float)Math.Pow(2 * noiseScale, 2), seaLevel + 0.01f, seaLevel + 0.05f);
        //fill small islands
        elevationMap = Utils.ReverseMatrix(FillSmallOceans(Utils.ReverseMatrix(elevationMap), 10, seaLevel, seaLevel + 0.05f));

        biomeGenerator = new BiomeGenerator(elevationMap, mapSize, seed, offsetTemperature, offsetTemperatureCurveShift, offsetTemperaturePeriod, seaLevel, noiseScale);

        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                ColorPixel(elevationMap[x, y], x, y);
            }
        }

        DisplayMap();
    }

    public float[,] FillSmallOceans(float[,] elevation, float minsize, float threshold, float target) {
        bool[,] visited = new bool[mapSize, mapSize];
        List<Tuple<int, int>> dirs = new();
        dirs.Add(new Tuple<int, int>(0, 1));
        dirs.Add(new Tuple<int, int>(1, 0));
        dirs.Add(new Tuple<int, int>(0, -1));
        dirs.Add(new Tuple<int, int>(-1, 0));

        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
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
                            if (i >= 0 && i < mapSize && j >= 0 && j < mapSize) {
                                if (!visited[i, j] && elevation[i, j] <= threshold) {
                                    opened.Push(new Tuple<int, int>(i, j));
                                    visited[i, j] = true;
                                    count += 1;
                                }
                            } else {
                                count += 2 * (int)noiseScale;
                            }
                        }
                    }
                    if (count < minsize) {
                        foreach (var point in closed) {
                            elevation[point.Item1, point.Item2] = target;
                            ColorPixel(elevation[x, y], point.Item1, point.Item2);
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
                colorMap[y * mapSize + x] = regions[i].color;
                break;
            }
        }
    }

    public void DisplayMap() {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(baseNoiseMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, mapSize, mapSize));
        } else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapSize)));
        } else if (drawMode == DrawMode.TemperatureMap) {
            biomeGenerator.GenerateTemperatureMap();
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.temperatureColorMap, mapSize, mapSize));
        } else if (drawMode == DrawMode.PrecipitationMap) {
            biomeGenerator.GenerateTemperatureMap();
            biomeGenerator.GenerateRainMap();
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.precipitationColorMap, mapSize, mapSize));
        } else if (drawMode == DrawMode.BiomeMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(biomeGenerator.GetBiomeMap(), mapSize, mapSize));
        }
    }

    // Constrain values on the Unity editor
    void OnValidate() {
        if (lacunarity < 1) {
            lacunarity = 1;
        }
        if (octaves < 0) {
            octaves = 0;
        }
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapSize);
    }

    public int GetSeed() {
        return seed;
    }


}
