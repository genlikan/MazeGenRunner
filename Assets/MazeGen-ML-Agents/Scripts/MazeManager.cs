using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.SideChannels;
using UnityEngine;
using Unity.MLAgents;

public class MazeManager : MonoBehaviour
{
    public GameObject mazeGeneratorPrefab; // Assign your "Maze Generator" prefab in the Inspector
    public MazeNode nodePrefab; // Add this line to reference your MazeNode prefab
    public int mazesPerRow = 3; // How many mazes to place in a single row
    public float spacingBetweenMazes = 1f; // The spacing between each maze
    public Vector2Int defaultMazeSize = new Vector2Int(4, 4); // Default size for each maze
    public float nodeSize = 1.0f;
    // public GameObject agentPrefab;
    // public GameObject rewardPrefab;
    public static MazeManager Instance { get; private set; }

        void Awake() {
            if (Instance == null) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            } else {
                Destroy(gameObject);
            }
        }
    public void RegenerateMazes()
    {
        // Destroy existing maze instances
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Generate new mazes
        GenerateMazes();
    }
    void Start()
    {
        UpdateMazeSizeFromCurriculum();
        GenerateMazes();
    }
    void UpdateMazeSizeFromCurriculum()
        {
            // Fetch the current value of mazeSize set by the curriculum
            var envParameters = Academy.Instance.EnvironmentParameters;
            int mazeSizeX = (int)envParameters.GetWithDefault("mazeSize_x", defaultMazeSize.x);
            int mazeSizeY = (int)envParameters.GetWithDefault("mazeSize_y", defaultMazeSize.y);
            
            defaultMazeSize = new Vector2Int(mazeSizeX, mazeSizeY);
        }

    void GenerateMazes()
    {
        float mazePhysicalWidth = defaultMazeSize.x * nodeSize;
        float mazePhysicalHeight = defaultMazeSize.y * nodeSize;

        for (int x = 0; x < mazesPerRow; x++)
        {
            for (int y = 0; y < mazesPerRow; y++)
            {
                Vector3 position = new Vector3(
                    x * (mazePhysicalWidth + spacingBetweenMazes) - (mazesPerRow / 2.0f * mazePhysicalWidth) + mazePhysicalWidth / 2,
                    0,
                    y * (mazePhysicalHeight + spacingBetweenMazes) - (mazesPerRow / 2.0f * mazePhysicalHeight) + mazePhysicalHeight / 2);

                GameObject mazeInstance = Instantiate(mazeGeneratorPrefab, position, Quaternion.identity, transform);
                mazeInstance.name = $"Maze_{x}_{y}";
                MazeGenerator mazeGen = mazeInstance.GetComponent<MazeGenerator>();
                // Debug.Log(mazeGen.EndPosition);
                // Debug.Log(mazeInstance);
                if (mazeGen != null)
                {
                    mazeGen.SetNodePrefab(nodePrefab);
                    mazeGen.SetMazeSize(defaultMazeSize);
                }
            }
        }
    }
}
