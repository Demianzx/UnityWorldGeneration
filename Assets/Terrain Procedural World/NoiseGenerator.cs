using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator 
{
   public static float[,] GenerateNoiseMap(int noiseSampleSize, float scale, int resolution = 1)
   {
       // Create a noise map with the size of the noiseSampleSize
       //returns a 2D array of floats
        float[,] noiseMap = new float[noiseSampleSize * resolution, noiseSampleSize * resolution];
    
        for (int y = 0; y < noiseSampleSize * resolution; y++)
        {
             for (int x = 0; x < noiseSampleSize * resolution; x++)
             {
                float sampleX = x / scale / resolution;
                float sampleY = y / scale / resolution;
    
                float perlinValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinValue;
             }
        }
    
        return noiseMap;
   }
   
}
