using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MazeGenerator : MonoBehaviour
{
    [SerializeField] public MazeNode nodePrefab;
    [SerializeField] public Vector2Int mazeSize;
    [SerializeField] public float nodeSize;
    [SerializeField] private GameObject agentPrefab;
    [SerializeField] private GameObject rewardPrefab;
    public List<MazeNode> nodes = new List<MazeNode>();
    public List<MazeNode> deadEnds = new List<MazeNode>();
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public bool useSeed = false; // Control whether to use the seed or not

    public int seed = 0;

    // Public method to set the node prefab
    public void SetNodePrefab(MazeNode prefab)
    {
        nodePrefab = prefab;
    }

    // Public method to set the maze size
    public void SetMazeSize(Vector2Int size)
    {
        mazeSize = size;
        // Optionally, regenerate the maze if needed
    }
    // void Awake() {
    //     GenerateMazeInstant(mazeSize);
    // }
    private void Start()
    {
        if (useSeed)
        {
            InitSeed(seed);
        }
        GenerateMazeInstant(mazeSize);
        // StartCoroutine(GenerateMaze(mazeSize));
    }

    void InitSeed(int seed)
        {
            Random.InitState(seed);
        }

    void GenerateMazeInstant(Vector2Int size)
    {
        // Create nodes
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // Adjusted to account for the Maze Generator's position
                Vector3 nodePos = new Vector3(
                    (x - (size.x / 2f)) * nodeSize,
                    0,
                    (y - (size.y / 2f)) * nodeSize
                ) + transform.position; // Add the Maze Generator's position
                // Debug.Log(nodePos);
                MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(newNode);
            }
        }

        List<MazeNode> currentPath = new List<MazeNode>();
        List<MazeNode> completedNodes = new List<MazeNode>();
        // List<MazeNode> deadEnds = new List<MazeNode>();

        // Choose starting node
        currentPath.Add(nodes[Random.Range(0, nodes.Count)]);
        currentPath[0].SetState(NodeState.Current);
        StartPosition = currentPath[0].transform.position;
        // Debug.Log($"Start Path Node Position: {StartPosition}");

        while (completedNodes.Count < nodes.Count)
        {
            // Check nodes next to the current node
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirections = new List<int>();

            int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if (currentNodeX < size.x - 1)
            {
                // Check node to the right of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1);
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }
            if (currentNodeX > 0)
            {
                // Check node to the left of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }
            if (currentNodeY < size.y - 1)
            {
                // Check node above the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }
            if (currentNodeY > 0)
            {
                // Check node below the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }

            // Choose next node
            if (possibleDirections.Count > 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                MazeNode chosenNode = nodes[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                currentPath.Add(chosenNode);
                chosenNode.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }
        }

        IdentifyDeadEnds();
        SelectRewardPosition();
        PlaceAgentAndReward(); // New method call
    }

    void PlaceAgentAndReward()
    {
        if (StartPosition != Vector3.zero && EndPosition != Vector3.zero)
        {
            // Assuming StartPosition and EndPosition are already in world space
            Instantiate(agentPrefab, StartPosition, Quaternion.identity, transform);
            Instantiate(rewardPrefab, EndPosition, Quaternion.identity, transform);
        }
        else
        {
            Debug.LogError("MazeGenerator: StartPosition or EndPosition not set.");
        }
    }

    void IdentifyDeadEnds()
    {
        // deadEnds.Clear(); // Assuming deadEnds is also accessible class-wide
        foreach (MazeNode node in nodes) // Direct access to 'nodes'
        {
            if (node.WallCount() == 3)
            {
                deadEnds.Add(node);
            }
        }
    }

    void SelectRewardPosition()
    {
        // Make sure there are dead ends available
        if (deadEnds.Count > 0)
        {
            MazeNode rewardNode;
            int attempts = 0; // To prevent infinite loops

            do
            {
                // Randomly select a dead-end node
                rewardNode = deadEnds[Random.Range(0, deadEnds.Count)];
                
                // Increment attempts counter
                attempts++;

                // Break if all dead ends are checked to avoid infinite loop
                if (attempts > deadEnds.Count+50)
                {
                    Debug.LogError("No suitable position for the reward found.");
                    return;
                }

            // Repeat if the chosen dead-end is the same as the start position
            } while (rewardNode.transform.position == StartPosition);

            // Set EndPosition to the chosen dead-end's position
            EndPosition = rewardNode.transform.position;
            // Debug.Log($"Reward Node Position: {EndPosition}");
        }
        else
        {
            Debug.LogError("No dead ends available for placing the reward.");
        }
    }

    public void RegenerateMaze()
    {
        foreach (Transform child in transform)
        {
            GameObject.Destroy(child.gameObject);
        }
        nodes.Clear();
        deadEnds.Clear();
        GenerateMazeInstant(mazeSize);
    }

    IEnumerator GenerateMaze(Vector2Int size)
    {
        List<MazeNode> nodes = new List<MazeNode>();

        // Create nodes
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // Adjusted to account for the Maze Generator's position
                Vector3 nodePos = new Vector3(
                    (x - (size.x / 2f)) * nodeSize,
                    0,
                    (y - (size.y / 2f)) * nodeSize
                ) + transform.position; // Add the Maze Generator's position
                // Debug.Log(nodePos);

                MazeNode newNode = Instantiate(nodePrefab, nodePos, Quaternion.identity, transform);
                nodes.Add(newNode);

                yield return null;
            }
        }

        List<MazeNode> currentPath = new List<MazeNode>();
        List<MazeNode> completedNodes = new List<MazeNode>();

        // Choose starting node
        currentPath.Add(nodes[Random.Range(0, nodes.Count)]);
        currentPath[0].SetState(NodeState.Current);

        while (completedNodes.Count < nodes.Count)
        {
            // Check nodes next to the current node
            List<int> possibleNextNodes = new List<int>();
            List<int> possibleDirections = new List<int>();

            int currentNodeIndex = nodes.IndexOf(currentPath[currentPath.Count - 1]);
            int currentNodeX = currentNodeIndex / size.y;
            int currentNodeY = currentNodeIndex % size.y;

            if (currentNodeX < size.x - 1)
            {
                // Check node to the right of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + size.y]))
                {
                    possibleDirections.Add(1);
                    possibleNextNodes.Add(currentNodeIndex + size.y);
                }
            }
            if (currentNodeX > 0)
            {
                // Check node to the left of the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - size.y]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - size.y]))
                {
                    possibleDirections.Add(2);
                    possibleNextNodes.Add(currentNodeIndex - size.y);
                }
            }
            if (currentNodeY < size.y - 1)
            {
                // Check node above the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex + 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex + 1]))
                {
                    possibleDirections.Add(3);
                    possibleNextNodes.Add(currentNodeIndex + 1);
                }
            }
            if (currentNodeY > 0)
            {
                // Check node below the current node
                if (!completedNodes.Contains(nodes[currentNodeIndex - 1]) &&
                    !currentPath.Contains(nodes[currentNodeIndex - 1]))
                {
                    possibleDirections.Add(4);
                    possibleNextNodes.Add(currentNodeIndex - 1);
                }
            }

            // Choose next node
            if (possibleDirections.Count > 0)
            {
                int chosenDirection = Random.Range(0, possibleDirections.Count);
                MazeNode chosenNode = nodes[possibleNextNodes[chosenDirection]];

                switch (possibleDirections[chosenDirection])
                {
                    case 1:
                        chosenNode.RemoveWall(1);
                        currentPath[currentPath.Count - 1].RemoveWall(0);
                        break;
                    case 2:
                        chosenNode.RemoveWall(0);
                        currentPath[currentPath.Count - 1].RemoveWall(1);
                        break;
                    case 3:
                        chosenNode.RemoveWall(3);
                        currentPath[currentPath.Count - 1].RemoveWall(2);
                        break;
                    case 4:
                        chosenNode.RemoveWall(2);
                        currentPath[currentPath.Count - 1].RemoveWall(3);
                        break;
                }

                currentPath.Add(chosenNode);
                chosenNode.SetState(NodeState.Current);
            }
            else
            {
                completedNodes.Add(currentPath[currentPath.Count - 1]);

                currentPath[currentPath.Count - 1].SetState(NodeState.Completed);
                currentPath.RemoveAt(currentPath.Count - 1);
            }

            yield return new WaitForSeconds(0.05f);
        }
    }
}
