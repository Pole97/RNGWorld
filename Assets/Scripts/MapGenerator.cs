using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class MapGenerator : MonoBehaviour {

    public enum DrawMode { NoiseMap, ColorMap, BiomeMap, FalloffMap, TemperatureMap, PrecipitationMap };
    public DrawMode drawMode;
    float zoom;

    public Settings settings;

    [HideInInspector]
    public bool noiseSettingsFaldout;
    [HideInInspector]
    public bool settingsFaldout;

    public TMP_Text seedText;

    public TMP_InputField inputSeed;

    public bool autoUpdate;

    float[,] falloffMap;
    float[,] baseNoiseMap, mountainNoiseMap, elevationMap;

    BiomeGenerator biomeGenerator;
    Color[] colorMap;

    void Awake() {
        falloffMap = FalloffGenerator.GenerateFalloffMap(settings.mapSize);
    }

    void Start() {
        System.Random prng = new();
        settings.seed = prng.Next();
        seedText.text = settings.seed.ToString();
        GenerateMap();
    }

    void Update() {
        if (Input.GetButton("Generate")) {
            GenerateMapFromButton();
        }
        if (Input.GetButton("RandomSeed")) {
            System.Random prng = new();
            settings.seed = prng.Next();
            seedText.text = settings.seed.ToString();
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
                settings.seed = Convert.ToInt32(inputSeed.text);
                GenerateMap();
                seedText.text = settings.seed.ToString();
            } catch (System.Exception) {
                Debug.LogError("Seed format not recognizable");
                throw;
            }
        }
    }

    void GenerateHeightMap() {
        zoom = settings.mapSize / 100f;
        falloffMap = FalloffGenerator.GenerateFalloffMap(settings.mapSize);
        baseNoiseMap = Noise.GenerateNoiseMap(settings.mapSize, settings.mapSize, settings.seed + 66, settings.noiseScale, zoom, 2, 0.5f,
                                                settings.noiseSettings.lacunarity, settings.noiseSettings.offset, settings.noiseSettings.distortionStrength);
        mountainNoiseMap = Noise.GenerateNoiseMap(settings.mapSize, settings.mapSize, settings.seed, settings.noiseScale, zoom / 2f,
                                                   settings.noiseSettings.octaves, settings.noiseSettings.persistance, settings.noiseSettings.lacunarity, settings.noiseSettings.offset, settings.noiseSettings.distortionStrength * 4);
        colorMap = new Color[settings.mapSize * settings.mapSize];
        elevationMap = new float[settings.mapSize, settings.mapSize];


        for (int y = 0; y < settings.mapSize; y++) {
            for (int x = 0; x < settings.mapSize; x++) {
                baseNoiseMap[x, y] = (float)Math.Tanh(baseNoiseMap[x, y] * 4f - 0.1f);
                mountainNoiseMap[x, y] *= 2f;
                mountainNoiseMap[x, y] = (mountainNoiseMap[x, y] + baseNoiseMap[x, y]) / 2f;
                if (settings.useFalloff) {
                    elevationMap[x, y] = Mathf.Clamp(mountainNoiseMap[x, y] - falloffMap[x, y], -1f, 1f);
                } else {
                    elevationMap[x, y] = Mathf.Clamp(mountainNoiseMap[x, y], -1f, 1f);
                }
            }
        }
        // fill inland oceans
        elevationMap = FillSmallOceans(elevationMap, settings.minOceanSize, settings.seaLevel, settings.seaLevel + 0.1f);
        elevationMap = FillSmallOceans(elevationMap, settings.minOceanSize, settings.seaLevel + 0.01f, settings.seaLevel + 0.05f);
        // fill small islands
        elevationMap = Utils.ReverseMatrix(FillSmallOceans(Utils.ReverseMatrix(elevationMap), 10, settings.seaLevel, settings.seaLevel + 0.05f));
    }

    void GenerateBiomeMap() {
        biomeGenerator = new BiomeGenerator(elevationMap, settings.mapSize, settings.seed, settings.amplitude, settings.offsetTemperature,
                                            settings.offsetTemperatureCurveShift, settings.offsetTemperaturePeriod, settings.seaLevel, settings.noiseScale);
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
        for (int i = 0; i < settings.regions.Length; i++) {
            if (value <= settings.regions[i].height) {
                colorMap[y * settings.mapSize + x] = settings.regions[i].color;
                break;
            }
        }
    }

    public void DisplayMap() {
        MapDisplay display = FindObjectOfType<MapDisplay>();
        if (drawMode == DrawMode.NoiseMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(elevationMap));
        } else if (drawMode == DrawMode.ColorMap) {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colorMap, settings.mapSize, settings.mapSize));
        } else if (drawMode == DrawMode.FalloffMap) {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(settings.mapSize)));
        } else if (drawMode == DrawMode.TemperatureMap) {
            biomeGenerator.GenerateTemperatureMap(); ;
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
            GenerateMap();
        }
    }

    public void OnSettingsUpdated() {
        if (autoUpdate) {
            falloffMap = FalloffGenerator.GenerateFalloffMap(settings.mapSize);
            GenerateMap();
        }
    }

    public int GetSeed() {
        return settings.seed;
    }
    private void SaveTexture(Texture2D texture, string name) {
        //first Make sure you're using RGB24 as your texture format
        Texture2D texture2D = new Texture2D(settings.mapSize, settings.mapSize, TextureFormat.RGB24, false);
        //then Save To Disk as PNG
        byte[] bytes = texture.EncodeToPNG();
        var dirPath = Application.dataPath + "/SavedImages";
        if (!System.IO.Directory.Exists(dirPath)) {
            System.IO.Directory.CreateDirectory(dirPath);
        }
        var img_name = dirPath + "/img_" + name + ".png";
        if (!System.IO.File.Exists(img_name)) {
            System.IO.File.WriteAllBytes(img_name, bytes);
            Debug.Log(bytes.Length / 1024 + "Kb was saved as: " + dirPath);
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
}