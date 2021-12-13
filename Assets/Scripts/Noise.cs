using System;
using System.Collections;
using UnityEngine;

public static class Noise {
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, float zoom2, int octaves, float persistance, float lacunarity, Vector2 offset, float distortionStrength) {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new(seed);
        Vector2[] octaveOffset = new Vector2[octaves];
        for (int i = 0; i < octaves; i++) {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) + offset.y;
            octaveOffset[i] = new Vector2(offsetX, offsetY);
        }

        if (scale <= 0) {
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        // Scroll the map olways in the center, not in the up right corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {

                float amplitude = 1;
                float frequency = 1;
                float noiseHeight = 0;

                for (int i = 0; i < octaves; i++) {
                    float sampleX = (x - halfWidth) / scale * frequency + octaveOffset[i].x / scale * frequency;
                    float sampleY = (y - halfHeight) / scale * frequency + octaveOffset[i].y / scale * frequency;

                    float perlinValue = (DistortedNoise(sampleX / zoom2, sampleY / zoom2, distortionStrength, zoom2) * 2f) - 1f;
                    //float perlinValue = 2 * (0.5f - MathF.Abs(0.5f - DistortedNoise(sampleX, sampleY, distortionStrength)));

                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;
                }

                if (noiseHeight > maxNoiseHeight) {
                    maxNoiseHeight = noiseHeight;
                } else if (noiseHeight < minNoiseHeight) {
                    minNoiseHeight = noiseHeight;
                }

                noiseMap[x, y] = noiseHeight;
            }
        }

        // normalize noise map and then remap to [-1,1]
        for (int y = 0; y < mapHeight; y++) {
            for (int x = 0; x < mapWidth; x++) {
                float normal = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
                noiseMap[x, y] = Mathf.Lerp(-1f, 1f, normal);
            }
        }
        return noiseMap;
    }

    public static float DistortedNoise(float x, float y, float distortionStrength, float scale) {
        float xDistortion = distortionStrength * Distort(x / scale, y / scale);
        float yDistortion = distortionStrength * Distort(x / scale, y / scale);
        return (Mathf.PerlinNoise(x + xDistortion / scale, y + yDistortion / scale) * 2f) - 1f;
    }

    public static float Distort(float x, float y) {
        float wiggleDensity = 2.7f;
        return (Mathf.PerlinNoise(x * wiggleDensity, y * wiggleDensity) * 2f) - 1f;
    }
}
