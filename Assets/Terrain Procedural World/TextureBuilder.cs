using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextureBuilder : MonoBehaviour
{
    //builds a texture from a list of colors
   public static Texture2D BuildTexture(float[,] noiseMap, TerrainType[] terrainTypes)
    {
        int width = noiseMap.GetLength(0);
        int height = noiseMap.GetLength(1);
   
       //create color array for the pixels
       Color[] pixels = new Color[noiseMap.Length];

        //calculate the length of the pixels
       int pixelLength = noiseMap.GetLength(0);

        //loop through each pixel
       for(int i = 0; i < pixelLength; i++)
       {
           for(int j = 0; j < pixelLength; j++)
           {
               foreach(TerrainType terrainType in terrainTypes)
               {
                   if(noiseMap[i,j] <= terrainType.threshold)
                   {
                       pixels[i + j * width] = terrainType.color;
                       break;
                   }
               }
           }
       }

        //create a new texture and set it up.
         Texture2D texture = new Texture2D(pixelLength, pixelLength);
         texture.wrapMode = TextureWrapMode.Clamp;
         texture.filterMode = FilterMode.Point;
         texture.SetPixels(pixels);
         texture.Apply();

         return texture;
   }
}
