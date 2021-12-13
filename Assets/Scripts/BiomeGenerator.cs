using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class BiomeGenerator {

    public Color[] temperatureColorMap, precipitationColorMap;

    public float[,] temperatureMap;
    public float[,] rainMap;
    float[,] fillMap;
    int[,] origins;
    float[,] elevationMap;
    float[,] flowMap;
    float[,] unevenessMap;

    float[,] gradX;
    float[,] gradY;

    int mapSize;
    int seed;
    float offsetTemperatureCurveShift;
    float offsetTemperature;
    float offsetTemperaturePeriod;
    float seaLevel, noiseScale;

    private List<Tuple<int, int>> dirs;

    Dictionary<string, Color> biomeColor = new Dictionary<string, Color>(){
            {"DeepOcean",Settings.BiomeColourSettings.deepOcean},
            {"Ocean",Settings.BiomeColourSettings.ocean},
            {"Lake",Settings.BiomeColourSettings.lake},
            {"Ice",Settings.BiomeColourSettings.ice},
            {"River",Settings.BiomeColourSettings.river},
            {"Snow",Settings.BiomeColourSettings.snow},
            {"Rocky",Settings.BiomeColourSettings.rocky},
            {"Tundra",Settings.BiomeColourSettings.tundra},
            {"BorealForest",Settings.BiomeColourSettings.borealForest},
            {"Grassland",Settings.BiomeColourSettings.grassland},
            {"Forest",Settings.BiomeColourSettings.forest},
            {"Savanna",Settings.BiomeColourSettings.savanna},
            {"RainForest",Settings.BiomeColourSettings.rainForest},
            {"Beach",Settings.BiomeColourSettings.beach},
            {"Desert",Settings.BiomeColourSettings.desert},
            {"Red",Settings.BiomeColourSettings.red},
            {"Unknown",Settings.BiomeColourSettings.unknown}
        };


    public BiomeGenerator(float[,] elevationMap, int mapSize, int seed, float offsetTemperature, float offsetTemperatureCurveShift, float offsetTemperaturePeriod, float seaLevel, float noiseScale) {
        this.elevationMap = elevationMap;
        this.mapSize = mapSize;
        this.offsetTemperatureCurveShift = offsetTemperatureCurveShift;
        this.offsetTemperature = offsetTemperature;
        this.offsetTemperaturePeriod = offsetTemperaturePeriod;
        this.seaLevel = seaLevel;
        dirs = new List<Tuple<int, int>> {
            new Tuple<int, int>(0, 1),
            new Tuple<int, int>(1, 0),
            new Tuple<int, int>(0, -1),
            new Tuple<int, int>(-1, 0)
        };
        this.seed = seed;
        this.noiseScale = noiseScale;
    }

    public void GenerateTemperatureMap() {
        temperatureMap = new float[mapSize, mapSize];
        temperatureColorMap = new Color[mapSize * mapSize];
        for (int y = 0; y < mapSize; y++) {
            float latitude = (y / noiseScale) / (mapSize / 100);

            for (int x = 0; x < mapSize; x++) {
                //float latTemperature;

                float latTemperature = Mathf.Pow(Mathf.Cos((latitude + offsetTemperatureCurveShift) / offsetTemperaturePeriod), 2) * 0.8f + 0.1f;

                if (elevationMap[x, y] >= seaLevel) {
                    temperatureMap[x, y] = latTemperature * (1.1f - Mathf.Clamp(1.2f * (elevationMap[x, y] - seaLevel), 0.1f, 1.1f)) + offsetTemperature;
                } else {
                    temperatureMap[x, y] = latTemperature + offsetTemperature;
                }

                temperatureColorMap[y * mapSize + x] = Color.Lerp(Color.blue, Color.red, temperatureMap[x, y]);

                if (temperatureMap[x, y] <= 0.505f && temperatureMap[x, y] >= 0.495f) {
                    temperatureColorMap[y * mapSize + x] = Color.green;
                } else if (temperatureMap[x, y] <= 0.305f && temperatureMap[x, y] >= 0.295f) {
                    temperatureColorMap[y * mapSize + x] = Color.blue;
                } else if (temperatureMap[x, y] <= 0.802f && temperatureMap[x, y] >= 0.798f) {
                    temperatureColorMap[y * mapSize + x] = Color.yellow;
                }
            }
        }
    }

    public void GenerateRainMap() {
        float[,] dist = new float[mapSize, mapSize];
        rainMap = new float[mapSize, mapSize];

        // Create a priority queue
        PriorityQueue<Tuple<float, int, int>> priorityHeap = new(true);

        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                if (elevationMap[x, y] <= seaLevel) {
                    float distPenality = Mathf.Min(Mathf.Max(1f - temperatureMap[x, y], 0f), 1f) * noiseScale * 0.8f;
                    priorityHeap.Enqueue(distPenality, new(distPenality, x, y));
                }
                dist[x, y] = float.PositiveInfinity; //initialize dist matrix with infinity
            }
        }

        while (priorityHeap.Count > 0) {
            Tuple<float, int, int> temp = priorityHeap.Dequeue();
            float d = temp.Item1;
            int x = temp.Item2;
            int y = temp.Item3;

            if (dist[x, y] > d) {
                dist[x, y] = d;
                foreach (Tuple<int, int> dir in dirs) {
                    int i = x + dir.Item1;
                    int j = y + dir.Item2;
                    if (0 <= i && i < mapSize && 0 <= j && j < mapSize) {
                        if (elevationMap[i, j] > seaLevel) {
                            float cost = (0.7f * (elevationMap[i, j] - seaLevel)) + (0.5f * (1f + dir.Item1)) + (0.5f * temperatureMap[i, j]);
                            if ((elevationMap[i, j] - seaLevel) > 0.5f) { cost += 2f; }
                            if (dist[i, j] > (d + cost)) {
                                float newDist = d + cost;
                                priorityHeap.Enqueue(newDist, new Tuple<float, int, int>(newDist, i, j));
                            }
                        }
                    }
                }
            }
        }

        precipitationColorMap = new Color[mapSize * mapSize];
        // The precipitation is negatively exponent to the travel cost
        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {

                if (elevationMap[x, y] > seaLevel) {
                    rainMap[x, y] = Mathf.Exp(-(dist[x, y] / noiseScale) / 0.7f);
                    precipitationColorMap[y * mapSize + x] = Color.Lerp(Color.blue, Color.yellow, rainMap[x, y]);
                } else {
                    precipitationColorMap[y * mapSize + x] = new Color32(5, 195, 221, 128);
                }
                if (rainMap[x, y] <= 0.81f && rainMap[x, y] >= 0.79f) {
                    precipitationColorMap[y * mapSize + x] = new Color32(0, 135, 62, 255);
                } else if (rainMap[x, y] <= 0.305f && rainMap[x, y] >= 0.295f) {
                    precipitationColorMap[y * mapSize + x] = Color.yellow;
                } else if (rainMap[x, y] <= 0.102f && rainMap[x, y] >= 0.098f) {
                    precipitationColorMap[y * mapSize + x] = Color.red;
                }

            }
        }
    }

    //get lakes and waterflow
    public void CalculateWater() {
        origins = new int[mapSize, mapSize];
        fillMap = new float[mapSize, mapSize];
        bool[,] drained = new bool[mapSize, mapSize];
        PriorityQueue<Tuple<float, int, int, int>> priorityHeap = new(true);
        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                if (elevationMap[x, y] <= seaLevel - 0.01f) {
                    priorityHeap.Enqueue(0f, new Tuple<float, int, int, int>(0f, x, y, 0));
                    fillMap[x, y] = elevationMap[x, y];
                } else {
                    fillMap[x, y] = 0f;
                }

            }
        }
        System.Random rand = new(seed + 42);
        while (priorityHeap.Count > 0) {
            Tuple<float, int, int, int> temp = priorityHeap.Dequeue();
            float f = temp.Item1;
            int x = temp.Item2, y = temp.Item3, o = temp.Item4;
            if (!drained[x, y]) {
                drained[x, y] = true;
                fillMap[x, y] = f;
                origins[x, y] = (o + 2) % 4;
                foreach (var dir in dirs) {
                    int oo = dirs.IndexOf(dir);
                    int i = x + dir.Item1, j = y + dir.Item2;
                    if (0 <= i && i < mapSize && 0 <= j && j < mapSize) {
                        float ff = Mathf.Max(f, Mathf.Clamp(elevationMap[i, j], 0f, 1f)) + ((float)rand.NextDouble() * 0.001f);
                        priorityHeap.Enqueue(ff, new(ff, i, j, oo));
                    }
                }
            }
        }
        for (int y = 0; y < mapSize; y++) {
            for (int x = 0; x < mapSize; x++) {
                fillMap[x, y] -= Mathf.Clamp01(elevationMap[x, y]);
            }
        }
    }

    /*
     * For valleys which is too big compared to the percipitation, I fill them into plateau. 
     * I left the smaller valleys for lakes and shrink them a bit based on percipitation
    */
    public void FilloutLakes() {
        bool[,] visited = new bool[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                if (fillMap[x, y] > 0.01f && !visited[x, y]) {
                    Stack<Tuple<int, int>> opened = new();
                    Stack<Tuple<int, int>> closed = new();
                    int count = 0;
                    float rainCount = 0f;
                    float evaporationCount = 0f;
                    float avgFill = 0f;

                    opened.Push(new(x, y));
                    visited[x, y] = true;
                    count += 1;
                    rainCount += rainMap[x, y];
                    evaporationCount += temperatureMap[x, y];
                    avgFill += fillMap[x, y];

                    while (opened.Count > 0) {
                        Tuple<int, int> temp = opened.Pop();
                        int i = temp.Item1, j = temp.Item2;
                        closed.Push(temp);
                        foreach (var dir in dirs) {
                            int ii = i + dir.Item1, jj = j + dir.Item2;
                            if (0 <= ii && ii < mapSize && 0 <= jj && jj < mapSize) {
                                if (!visited[ii, jj] && fillMap[ii, jj] > 0.01f) {
                                    opened.Push(new(ii, jj));
                                    visited[ii, jj] = true;
                                    count += 1;
                                    rainCount += rainMap[ii, jj];
                                    evaporationCount += temperatureMap[ii, jj];
                                    avgFill += fillMap[ii, jj];
                                }
                            }
                        }
                    }
                    avgFill /= count;
                    if (count < 5 || rainCount / evaporationCount <= 0.5f) {
                        foreach (var p in closed) {
                            elevationMap[p.Item1, p.Item2] += fillMap[p.Item1, p.Item2];
                            fillMap[p.Item1, p.Item2] = 0f;
                        }
                    } else {
                        float proportion = Mathf.Max(0f, Mathf.Min(1f, 2f * (rainCount / evaporationCount - 0.5f)));
                        foreach (var p in closed) {
                            float toRaise = Mathf.Min(fillMap[p.Item1, p.Item2], (1f - proportion) * avgFill);
                            elevationMap[p.Item1, p.Item2] += toRaise;
                            fillMap[p.Item1, p.Item2] -= toRaise;
                        }
                    }
                }
            }
        }
    }

    public void GenerateFlowMap() {
        flowMap = new float[mapSize, mapSize];
        Array.Copy(rainMap, 0, flowMap, 0, rainMap.Length);
        int[,] degrees = new int[mapSize, mapSize];
        Stack<Tuple<int, int, float>> opened = new();
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                if (elevationMap[x, y] > seaLevel) {
                    foreach (var dir in dirs) {
                        int oo = dirs.IndexOf(dir);
                        int i = x + dir.Item1, j = y + dir.Item2;
                        if (0 <= i && i < mapSize && 0 <= j && j < mapSize) {
                            if (elevationMap[i, j] > seaLevel) {
                                if (origins[i, j] == (oo + 2) % 4) { // found it
                                    degrees[x, y] += 1;
                                }
                            }
                        }
                    }
                    if (degrees[x, y] == 0) {
                        degrees[x, y] = 1;
                        opened.Push(new(x, y, 0f));
                    }
                }
            }
        }
        while (opened.Count > 0) {
            Tuple<int, int, float> temp = opened.Pop();
            int x = temp.Item1, y = temp.Item2;
            flowMap[x, y] += temp.Item3;
            degrees[x, y] -= 1;
            if (degrees[x, y] == 0) {
                int origin = origins[x, y];
                int i = x + dirs[origin].Item1;
                int j = y + dirs[origin].Item2;
                if (elevationMap[i, j] > seaLevel) {
                    opened.Push(new(i, j, flowMap[x, y]));
                    if (flowMap[x, y] > 1.5f / noiseScale && elevationMap[x, y] + fillMap[x, y] < elevationMap[i, j] + fillMap[i, j]) {
                        elevationMap[i, j] = elevationMap[x, y] + fillMap[x, y] - fillMap[i, j];
                    }
                }
            }
        }

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                flowMap[x, y] = flowMap[x, y] / Mathf.Pow(noiseScale, 2);
            }
        }
    }

    public void GenerateUnevenessMap() {
        float[,] ef = new float[mapSize, mapSize];
        gradX = new float[mapSize, mapSize];
        gradY = new float[mapSize, mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                ef[x, y] = Mathf.Clamp01(elevationMap[x, y]) + fillMap[x, y];
            }
        }

        float[,] efShiftColumn = new float[mapSize, mapSize];
        Array.Copy(ef, 0, efShiftColumn, 0, ef.Length);
        efShiftColumn = Utils.ShiftMat(1, 1, efShiftColumn);

        float[,] efShiftRow = new float[mapSize, mapSize];
        Array.Copy(ef, 0, efShiftRow, 0, ef.Length);
        efShiftRow = Utils.ShiftMat(1, 0, efShiftRow);

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                gradX[x, y] = (ef[x, y] - efShiftColumn[x, y]) * noiseScale;
                gradY[x, y] = (ef[x, y] - efShiftRow[x, y]) * noiseScale;
                gradX[x, 0] = 0f; // column 0 all at 0
                gradY[0, y] = 0f; // row 0 all at 0
            }
        }

        float[,] gradXShiftColumn = new float[mapSize, mapSize];
        Array.Copy(gradX, 0, gradXShiftColumn, 0, gradX.Length);
        gradXShiftColumn = Utils.ShiftMat(-1, 1, gradXShiftColumn);

        float[,] gradYShiftRow = new float[mapSize, mapSize];
        Array.Copy(gradY, 0, gradYShiftRow, 0, gradY.Length);
        gradYShiftRow = Utils.ShiftMat(-1, 0, gradYShiftRow);

        unevenessMap = new float[mapSize, mapSize];

        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                unevenessMap[x, y] = Mathf.Sqrt(Mathf.Pow(gradX[x, y], 2) + Mathf.Pow(gradY[x, y], 2) + Mathf.Pow(gradXShiftColumn[x, y], 2) + Mathf.Pow(gradYShiftRow[x, y], 2)) / 2f;
            }
        }
    }

    public string GetBiome(float elevation, float temperature, float fill, float rain, float flow, float uneveness) {
        if (elevation < -0.4f) {
            if (temperature - (0.5f * elevation) < 0.5f) {
                return "Ice";
            } else {
                return "DeepOcean";
            }
        } else if (elevation < 0f) {
            if (temperature - (0.5f * elevation) < 0.4f) {
                return "Ice";
            } else {
                return "Ocean";
            }
        }

        if (fill > 0.01f) {
            if (temperature < 0.4f) {
                return "Ice";
            } else {
                return "Lake";
            }
        } else if (flow > 1.5f / noiseScale) {
            return "River";
        }

        if (elevation < 0.07f) {
            return "Beach";
        }

        return GetBiomeLand(temperature, rain + 0.5f * flow, uneveness);
    }

    public string GetBiomeLand(float temperature, float rain, float uneveness) {
        if (!Utils.linearBound3(temperature, rain, uneveness, 0.3f, 0.5f, -1.5f)) {
            if (temperature < 0.2f) {
                return "Snow";
            } else {
                return "Rocky";
            }
        }

        if (!Utils.linearBound2(temperature, rain, 0.4f, 1f)) {
            return "Tundra";
        }

        if (!Utils.linearBound2(1 - temperature, rain, 1f - (-0.2f), 0.3f)) {
            if (Utils.linearBound2(temperature, rain, 0.6f, -1f)) {
                return "Desert";
            } else if (!Utils.linearBound2(1f - temperature, rain, 1f - (-1f), 0.1f)) {
                return "Desert";
            }
            return "Grassland";
        }

        if (temperature > 0.7f) {
            if (rain > 0.7f) {
                return "RainForest";
            } else {
                return "Savanna";
            }
        }

        if (temperature < 0.4f) {
            return "BorealForest";
        }

        if (rain > 0.7f) {
            return "RainForest";
        }

        if (rain < 0.3f) {
            return "Grassland";
        }
        return "Forest";
    }

    public Color[] GetColorBiomeGraph() {
        Color[] colorMap = new Color[mapSize * mapSize];
        for (int i = 0; i < mapSize; i++) {
            for (int j = 0; j < mapSize; j++) {
                string name = GetBiomeLand((float)i / mapSize, (float)j / mapSize, 0f);
                colorMap[(mapSize - 1 - j) * mapSize + (mapSize - 1 - i)] = biomeColor[name];
            }
        }
        return colorMap;
    }


    public Color[] GetBiomeMap() {
        GenerateTemperatureMap();
        GenerateRainMap();
        CalculateWater();
        FilloutLakes();
        GenerateFlowMap();
        GenerateUnevenessMap();
        Color[] biomeMap = new Color[mapSize * mapSize];
        float shade; //= new float[mapSize * mapSize];
        for (int x = 0; x < mapSize; x++) {
            for (int y = 0; y < mapSize; y++) {
                string name = GetBiome(elevationMap[x, y], temperatureMap[x, y], fillMap[x, y], rainMap[x, y], flowMap[x, y], unevenessMap[x, y]);
                shade = Mathf.Cos(Mathf.Atan(gradX[x, y]) - 0.1f) * 0.4f + 0.6f;
                biomeMap[y * mapSize + x] = new Color(Mathf.Pow(biomeColor[name].r * shade, 1.2f), Mathf.Pow(biomeColor[name].g * shade, 1.2f), Mathf.Pow(biomeColor[name].b * shade, 1.2f));
            }
        }
        return biomeMap;
    }
}



