using UnityEngine;
using System.Collections.Generic;

public class InfiniteMapGenerator : MonoBehaviour
{
    public GameObject[] chunkPrefabs; // 0: 평지, 1: 내리막, 2: 오르막
    public Transform player;
    public float flatChunkWidth = 20f;
    public float slopeChunkWidth = 10f;
    public float spawnDistance = 40f;
    public float despawnDistance = 60f;
    public int poolSize = 10;
    public int maxPoolSize = 15;
    public float downhillYDrop = 10f;
    public float uphillYOffset = 10f; // 오르막길(오른쪽) 및 내리막길(왼쪽)의 Y 위치를 올리는 오프셋

    private float lastChunkX = 0f;
    private float firstChunkX = 0f;
    private List<ChunkData> activeChunks = new List<ChunkData>(20);
    private Queue<GameObject> chunkPool = new Queue<GameObject>();
    private float checkInterval = 0.1f;
    private float lastCheckTime = 0f;
    private Rigidbody2D playerRb;

    private struct ChunkData
    {
        public GameObject chunk;
        public int type;
        public float width;
        public float startY;
        public float endY;
        public bool isLeft; // 왼쪽 방향인지
    }

    void Start()
    {
        if (chunkPrefabs == null || chunkPrefabs.Length == 0 || player == null)
        {
            Debug.LogError("chunkPrefabs or player is not assigned in the Inspector!");
            return;
        }

        playerRb = player.GetComponent<Rigidbody2D>();
        if (playerRb == null)
        {
            Debug.LogError("Player needs a Rigidbody2D component!");
            return;
        }

        InitializePool();

        float playerX = Mathf.Floor(player.position.x / flatChunkWidth) * flatChunkWidth;
        lastChunkX = playerX;
        firstChunkX = playerX;

        SpawnChunk(playerX - flatChunkWidth, 0, true);  // 왼쪽 평지
        SpawnChunk(playerX, 0, false);                  // 플레이어 위치 평지
        SpawnChunk(playerX + flatChunkWidth, 0, false); // 오른쪽 평지

        Debug.Log($"Initial chunks: Left={playerX - flatChunkWidth}, Center={playerX}, Right={playerX + flatChunkWidth}");
    }

    void Update()
    {
        if (Time.time - lastCheckTime >= checkInterval)
        {
            lastCheckTime = Time.time;

            if (activeChunks.Count == 0) return;

            float rightEdge = lastChunkX + activeChunks[activeChunks.Count - 1].width;
            float leftEdge = firstChunkX;

            if (player.position.x > rightEdge - spawnDistance)
            {
                int randomType = Random.Range(0, 3);
                SpawnChunk(rightEdge, randomType, false);
            }

            if (player.position.x < leftEdge + spawnDistance)
            {
                int randomType = Random.Range(0, 3);
                float newX = firstChunkX - GetChunkWidth(randomType);
                SpawnChunk(newX, randomType, true);
            }

            RepositionChunks();
        }
    }

    void InitializePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            int randomIndex = Random.Range(0, chunkPrefabs.Length);
            GameObject chunk = Instantiate(chunkPrefabs[randomIndex], Vector2.zero, Quaternion.identity);
            chunk.SetActive(false);
            chunkPool.Enqueue(chunk);
        }
    }

    float GetChunkWidth(int chunkType)
    {
        return (chunkType == 0) ? flatChunkWidth : slopeChunkWidth;
    }

    void SpawnChunk(float xPosition, int chunkType, bool isLeft)
    {
        if (chunkType < 0 || chunkType >= chunkPrefabs.Length || chunkPrefabs[chunkType] == null)
        {
            Debug.LogError($"Invalid chunkType {chunkType} or prefab is null!");
            return;
        }

        GameObject chunk;
        float chunkWidth = GetChunkWidth(chunkType);

        if (chunkPool.Count > 0)
        {
            chunk = chunkPool.Dequeue();
            if (chunk.name != chunkPrefabs[chunkType].name + "(Clone)")
            {
                Destroy(chunk);
                chunk = Instantiate(chunkPrefabs[chunkType], Vector2.zero, Quaternion.identity);
            }
        }
        else
        {
            chunk = Instantiate(chunkPrefabs[chunkType], Vector2.zero, Quaternion.identity);
        }

        // X축 반전 제거: 항상 기본 방향 유지
        chunk.transform.localScale = Vector3.one;

        chunk.SetActive(true);

        ChunkData chunkData = new ChunkData { chunk = chunk, type = chunkType, width = chunkWidth, isLeft = isLeft };
        activeChunks.Insert(isLeft ? 0 : activeChunks.Count, chunkData);

        if (isLeft)
        {
            firstChunkX = xPosition;
        }
        else
        {
            lastChunkX = xPosition;
        }

        AdjustChunkHeight(ref chunkData, xPosition);
        activeChunks[isLeft ? 0 : activeChunks.Count - 1] = chunkData;

        Debug.Log($"Spawned chunk at X={xPosition}, Type={chunkType}, Width={chunkWidth}, Y={chunk.transform.position.y}, Direction={(isLeft ? "Left" : "Right")}");
    }

    void AdjustChunkHeight(ref ChunkData chunkData, float xPosition)
    {
        float prevEndY = 0f;

        // 이전 청크의 끝점 Y 가져오기
        int prevIndex = chunkData.isLeft ? 1 : activeChunks.Count - 2;
        if (prevIndex >= 0 && prevIndex < activeChunks.Count)
        {
            prevEndY = activeChunks[prevIndex].endY;
        }

        // 현재 청크의 시작 Y 설정
        chunkData.startY = prevEndY;

        // 현재 청크의 끝점 Y 계산
        float currentEndY = chunkData.startY;
        if (chunkData.isLeft)
        {
            if (chunkData.type == 2) // 오르막길을 내리막길처럼
            {
                currentEndY -= downhillYDrop;
            }
            else if (chunkData.type == 1) // 내리막길을 오르막길처럼
            {
                currentEndY += downhillYDrop;
            }
        }
        else
        {
            if (chunkData.type == 1) // 내리막길
            {
                currentEndY -= downhillYDrop;
            }
            else if (chunkData.type == 2) // 오르막길
            {
                currentEndY += downhillYDrop;
            }
        }

        chunkData.endY = currentEndY;

        // 청크 위치 조정
        float positionY = chunkData.startY;
        if (chunkData.isLeft)
        {
            // 왼쪽 방향: 내리막길(type=1, 오르막처럼 동작)일 경우 위치를 10 유닛 올림
            if (chunkData.type == 1)
            {
                positionY += uphillYOffset;
            }
        }
        else
        {
            // 오른쪽 방향: 오르막길(type=2)일 경우 위치를 10 유닛 올림
            if (chunkData.type == 2)
            {
                positionY += uphillYOffset;
            }
        }

        chunkData.chunk.transform.position = new Vector2(xPosition, positionY);

        Debug.Log($"Adjusted chunk: PrevEnd Y={prevEndY}, Start Y={chunkData.startY}, End Y={chunkData.endY}, Position Y={positionY}, Type={chunkData.type}, Direction={(chunkData.isLeft ? "Left" : "Right")}");
    }

    void RepositionChunks()
    {
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            float chunkX = activeChunks[i].chunk.transform.position.x;
            float distanceFromPlayer = Mathf.Abs(player.position.x - chunkX);

            if (distanceFromPlayer > despawnDistance)
            {
                GameObject chunk = activeChunks[i].chunk;
                chunk.SetActive(false);
                if (chunkPool.Count < maxPoolSize)
                {
                    chunkPool.Enqueue(chunk);
                }
                else
                {
                    Destroy(chunk);
                }
                activeChunks.RemoveAt(i);

                if (i == 0 && activeChunks.Count > 0)
                {
                    firstChunkX = activeChunks[0].chunk.transform.position.x;
                }
                else if (i == activeChunks.Count && activeChunks.Count > 0)
                {
                    lastChunkX = activeChunks[activeChunks.Count - 1].chunk.transform.position.x;
                }
            }
        }
    }
}