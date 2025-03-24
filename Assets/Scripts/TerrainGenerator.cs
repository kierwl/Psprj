using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] private int width = 256;
    [SerializeField] private int height = 256;
    [SerializeField] private int depth = 20;
    [SerializeField] private float scale = 20f;
    [SerializeField] private float heightScale = 10f;
    
    private TerrainData terrainData;
    private Terrain terrain;

    void Start()
    {
        // 지형 데이터 생성
        terrainData = new TerrainData();
        terrainData.heightmapResolution = width + 1;
        terrainData.size = new Vector3(width, depth, height);

        // 노이즈 기반 높이맵 생성
        float[,] heights = new float[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float xCoord = (float)x / width * scale;
                float yCoord = (float)y / height * scale;
                heights[x, y] = Mathf.PerlinNoise(xCoord, yCoord) * heightScale;
            }
        }
        terrainData.SetHeights(0, 0, heights);

        // 지형 오브젝트 생성
        terrain = gameObject.AddComponent<Terrain>();
        terrain.terrainData = terrainData;
    }
} 