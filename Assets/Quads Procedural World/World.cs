using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public struct PerlinSettings
{
    public float scale;
    public int octaves;
    public float heightScale;
    public float heightOffset;
    public float probability;


    public PerlinSettings(float scale, int octaves, float heightScale, float heightOffset, float probability)
    {
        this.scale = scale;
        this.octaves = octaves;
        this.heightScale = heightScale;
        this.heightOffset = heightOffset;
        this.probability = probability;
    }
}

public class World : MonoBehaviour
{
    public static Vector3Int worldDimensions = new Vector3Int(20, 3, 20);
     public static Vector3Int extraWorldDimensions = new Vector3Int(10, 3, 10);
    public static Vector3Int chunkDimensions = new Vector3Int(10, 10, 10);
    public GameObject chunkPrefab;
    public GameObject mCamera;
    public GameObject player;
    public Slider loadingBar;

    public static PerlinSettings surfaceSettings;
    public PerlinGrapher surface;

    public static PerlinSettings stoneSettings;
    public PerlinGrapher stone;

    public static PerlinSettings diamondTopSettings;
    public PerlinGrapher diamondTop;

    public static PerlinSettings diamondBottomSettings;
    public PerlinGrapher diamondBottom;

    public static PerlinSettings caveSettings;
    public Perlin3DGrapher caves;

    HashSet<Vector3Int> chunkChecker = new HashSet<Vector3Int>();
    HashSet<Vector2Int> chunkColumns = new HashSet<Vector2Int>();
    Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();

    Vector3Int lastBuildPosition = new Vector3Int(0, 0, 0);
    int drawRadius=3;

    Queue<IEnumerator> buildQueue = new Queue<IEnumerator>();

    IEnumerator BuildCoordinator(){
        while(true){
            if(buildQueue.Count > 0){
                yield return buildQueue.Dequeue();
            }
            else{
                yield return null;
            }
        }
    }



    // Start is called before the first frame update
    void Start()
    {
        loadingBar.maxValue = worldDimensions.x*worldDimensions.z;
        surfaceSettings = new PerlinSettings(surface.scale, surface.octaves, surface.heightScale, surface.heightOffset, surface.probability);
        stoneSettings = new PerlinSettings(stone.scale, stone.octaves, stone.heightScale, stone.heightOffset, stone.probability);
        diamondTopSettings = new PerlinSettings(diamondTop.scale, diamondTop.octaves, diamondTop.heightScale, diamondTop.heightOffset, diamondTop.probability);
        diamondBottomSettings = new PerlinSettings(diamondBottom.scale, diamondBottom.octaves, diamondBottom.heightScale, diamondBottom.heightOffset, diamondBottom.probability);
        caveSettings = new PerlinSettings(caves.scale, caves.octaves, caves.heightScale, caves.heightOffset, caves.DrawCutOff);
       StartCoroutine(BuildWorld());
    }

    void BuildChunkColumn(int x, int z, bool meshEnabled = true)
    {
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if(!chunkChecker.Contains(position)){
            GameObject chunk = Instantiate(chunkPrefab);
            
            chunkPrefab.name = "Chunk " + position.x + " " + position.y + " " + position.z;
            Chunk chunkScript = chunk.GetComponent<Chunk>();
            chunkScript.CreateChunk(chunkDimensions, position);
            chunkChecker.Add(position);
            chunks.Add(position, chunkScript);
            }
            
            chunks[position].meshRenderer.enabled = meshEnabled;
            
        }
        chunkColumns.Add(new Vector2Int(x, z));
    }
    IEnumerator BuildExtraWorld()
    {
        int zEnd = worldDimensions.z + extraWorldDimensions.z;
        int zStart = worldDimensions.z;
        int xEnd = worldDimensions.x + extraWorldDimensions.x;
        int xStart = worldDimensions.x;

        for (int z = zStart; z < zEnd; z++)
        {
            for (int x = 0; x < xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }
        }
        for (int z = 0; z < zEnd; z++)
        {
            for (int x = xStart; x < xEnd; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z, false);
                yield return null;
            }
        }

    }
    IEnumerator BuildWorld()
    {
         for (int z = 0; z < worldDimensions.z; z++)
        {            
            for (int x = 0; x < worldDimensions.z; x++)
            {
                BuildChunkColumn(x * chunkDimensions.x, z * chunkDimensions.z);
                loadingBar.value += 1;
                yield return null;
            }
            
        }
        mCamera.SetActive(false);
        

        int xpos = (worldDimensions.x * chunkDimensions.x) / 2;
        int zpos = (worldDimensions.z * chunkDimensions.z) / 2;      
        int ypos = (int)MeshUtils.fBM(xpos, zpos, surfaceSettings.octaves, surfaceSettings.scale, surfaceSettings.heightScale, surfaceSettings.heightOffset) + 5;
        player.transform.position = new Vector3Int(xpos, ypos, zpos);
        loadingBar.gameObject.SetActive(false);
        player.SetActive(true);
        lastBuildPosition = Vector3Int.CeilToInt(player.transform.position);
        StartCoroutine(BuildCoordinator());
        StartCoroutine(UpdateWorld());
        StartCoroutine(BuildExtraWorld());
    }
    WaitForSeconds wait = new WaitForSeconds(0.5f);
    IEnumerator UpdateWorld()
    {
        while (true)
        {
           if((lastBuildPosition - player.transform.position).magnitude > chunkDimensions.x){
                lastBuildPosition = Vector3Int.CeilToInt(player.transform.position);
                int posx = (int)((player.transform.position.x / chunkDimensions.x) * chunkDimensions.x);
                int posz = (int)((player.transform.position.z / chunkDimensions.z) * chunkDimensions.z);
                buildQueue.Enqueue(BuildRecursiveWorld(posx, posz, drawRadius));
                buildQueue.Enqueue(HideColumns(posx, posz));
           }
           yield return wait;
        }
    }

    public void HideChunkColumn(int x, int z){
        for (int y = 0; y < worldDimensions.y; y++)
        {
            Vector3Int position = new Vector3Int(x, y * chunkDimensions.y, z);
            if(chunkChecker.Contains(position)){
                chunks[position].meshRenderer.enabled = false;
            }
        }
    }

    IEnumerator HideColumns(int x, int z){
        Vector2Int playerPosition = new Vector2Int(x, z);
        foreach(Vector2Int chunk in chunkColumns){
            if((chunk - playerPosition).magnitude > drawRadius*chunkDimensions.x){
                HideChunkColumn(chunk.x, chunk.y);
            }
        }
        yield return null;
    }

    
    IEnumerator BuildRecursiveWorld(int x, int z, int rad){
        int nextrad = rad - 1;
        if(rad <= 0)
            yield break;
        
        BuildChunkColumn(x, z+chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z + chunkDimensions.z, nextrad));
        yield return null;

        BuildChunkColumn(x, z-chunkDimensions.z);
        buildQueue.Enqueue(BuildRecursiveWorld(x, z - chunkDimensions.z, nextrad));
        yield return null;

        BuildChunkColumn(x + chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x + chunkDimensions.x, z, nextrad));
        yield return null;

        BuildChunkColumn(x - chunkDimensions.x, z);
        buildQueue.Enqueue(BuildRecursiveWorld(x - chunkDimensions.x, z, nextrad));
        yield return null;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
