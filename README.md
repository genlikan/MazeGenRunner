# MazeGenRunner
Using Unity ML-Agents to train models with a DepthFirstSearch Maze Generator.

![](https://github.com/genlikan/MazeGenRunner/blob/main/DFS_Maze_Generator.gif)



**Maze Generator (MazeGenerator.cs):**

Responsible for generating the maze layout. It uses a specified node prefab to create a grid of maze nodes, applies a maze generation algorithm to determine the walls of each node, and places the agent and reward in the maze.
• Maze size and node size can be specified, and there’s support for using a seed for reproducibility.
• Dead ends are identified, and one is selected to place the reward.


**Maze Node (MazeNode.cs):**

Represents each cell or node in the maze. It can have walls removed and its state changed (e.g., available, current, completed) to reflect its role in the maze generation and navigation process.


**Maze Manager (MazeManager.cs):**

Manages the maze environment, including generating multiple mazes, spacing between mazes, and updating maze size based on curriculum parameters from ML-Agents.


**Roller Agent (RollerAgent.cs):**

The agent that navigates the maze. It uses a Rigidbody for movement and detects collisions with walls and the reward.


**Observations Defined**

The Roller Agent (RollerAgent.cs) collects the following observations for the learning process:

• The proportion of the maximum step count that haspassed.

• The agent’s velocity in the x and z directions.

• The agent’s local position and rotation.

These observations help the agent understand its current
state in the environment to make informed decisions.


**Actions Defined**

The Roller Agent can perform the following actions based
on the action received:

• Move forward or backward.

• Turn left or right.

These actions allow the agent to navigate through the maze.


**Reward System**

• The agent receives a small negative reward for each time step to encourage it to find the reward quickly.

• When the agent visits a new area (grid cell) it hasn’t been to before, it receives a positive reward.

• Hitting a wall incurs a negative reward, and collecting the maze reward gives a large positive reward.

• The agent is penalized for being stationary for too long to prevent it from getting stuck.


**Training Process**

• At the beginning of each episode, the agent’s velocity is reset, its list of visited areas is cleared, and the progress timeout timer is reset.

• During each step, the agent receives rewards based on its actions and the outcome of those actions (e.g., moving to a new area, hitting walls, collecting the reward).

• If the agent collects the reward or the progress timeout is reached, the episode ends, and the maze may be regenerated for the next episode.
