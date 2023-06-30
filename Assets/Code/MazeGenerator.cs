using System.Collections.Generic;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{
    public struct Int3
    {
        public int x, y, z;

        public Int3(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public static Int3 operator +(Int3 a, Int3 b)
        {
            return new Int3(a.x + b.x, a.y + b.y, a.z + b.z);
        }

        public static bool operator ==(Int3 a, Int3 b)
        {
            if (a.x == b.x && a.y == b.y && a.z == b.z)
                return true;
            return false;
        }

        public static bool operator !=(Int3 a, Int3 b)
        {
            if (a.x == b.x && a.y == b.y && a.z == b.z)
                return false;
            return true;
        }

        public override bool Equals(object o)
        {
            if (o is Int3 i)
                return this == i;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", x, y, z);
        }
    }

    public Int3 Size;
    public int Loops;
    public GameObject BlueWall;
    public GameObject GreenWall;
    public GameObject RedWall;

    private Player player;
    private Transform goal;
    private float scale = 3.0f;
    private GameObject[,,,] walls;
    private bool[,,] visited;
    private List<Int3> positions;
    private Int3 position;
    private int total;
    private float startTime;

    private void Awake()
    {
        player = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Player>();
        goal = GameObject.FindGameObjectWithTag("Finish").transform;
    }

    public void GenerateMaze()
    {
        float before = Time.realtimeSinceStartup;
        player.Respawn();
        goal.position = new Vector3(Size.x - 1, Size.y - 1, Size.z - 1) * scale;

        DeleteWalls();
        CreateWalls();

        positions = new List<Int3>();
        position = new Int3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z));
        total = 0;
        MarkLocation();

        while (total < Size.x * Size.y * Size.z)
        {
            List<Int3> directions = new List<Int3>();
            if (!visited[position.x + 2, position.y + 1, position.z + 1])
                directions.Add(new Int3(1, 0, 0));
            if (!visited[position.x, position.y + 1, position.z + 1])
                directions.Add(new Int3(-1, 0, 0));
            if (!visited[position.x + 1, position.y + 2, position.z + 1])
                directions.Add(new Int3(0, 1, 0));
            if (!visited[position.x + 1, position.y, position.z + 1])
                directions.Add(new Int3(0, -1, 0));
            if (!visited[position.x + 1, position.y + 1, position.z + 2])
                directions.Add(new Int3(0, 0, 1));
            if (!visited[position.x + 1, position.y + 1, position.z])
                directions.Add(new Int3(0, 0, -1));

            if (directions.Count == 0)
            {
                position = positions[positions.Count - 2];
                positions.RemoveAt(positions.Count - 1);
                continue;
            }

            Int3 direction = directions[Random.Range(0, directions.Count)];
            DestroyWall(direction);
            position += direction;
            MarkLocation();
        }
        Loops = Mathf.Min(NumInnerWalls(), Loops);
        for (int i = 0; i < Loops; i++)
        {
            while (true)
            {
                position = new Int3(Random.Range(0, Size.x), Random.Range(0, Size.y), Random.Range(0, Size.z));
                List<Int3> directions = new();
                if (position.x < Size.x - 1)
                    directions.Add(new Int3(1, 0, 0));
                if (position.x > 0)
                    directions.Add(new Int3(-1, 0, 0));
                if (position.y < Size.y - 1)
                    directions.Add(new Int3(0, 1, 0));
                if (position.y > 0)
                    directions.Add(new Int3(0, -1, 0));
                if (position.z < Size.z - 1)
                    directions.Add(new Int3(0, 0, 1));
                if (position.z > 0)
                    directions.Add(new Int3(0, 0, -1));

                while (directions.Count > 0)
                {
                    Int3 direction = directions[Random.Range(0, directions.Count)];
                    if (ContainsWall(direction))
                    {
                        DestroyWall(direction);
                        goto Next;
                    }
                    directions.Remove(direction);
                }
            }
        Next:
            continue;
        }
        startTime = Time.realtimeSinceStartup;
        print("Generated maze in: " + (startTime - before) + "s");
        player.ToggleScoreText();
        player.ChangeScoreText("Size: " + Size.x + " x " + Size.y + " x " + Size.z + " (Loops: " + Loops + ")");
        player.Invoke("ToggleScoreText", 2.0f);
    }

    public int GetTime()
    {
        return (int)(Time.realtimeSinceStartup - startTime);
    }

    private void MarkLocation()
    {
        visited[position.x + 1, position.y + 1, position.z + 1] = true;
        positions.Add(position);
        total++;
    }

    private void DestroyWall(Int3 dir)
    {
        if (dir == new Int3(0, -1, 0))
        {
            Destroy(walls[position.x, position.y, position.z, 0]);
            Destroy(walls[position.x, position.y - 1, position.z, 1]);
        } 
        else if (dir == new Int3(0, 1, 0))
        {
            Destroy(walls[position.x, position.y, position.z, 1]);
            Destroy(walls[position.x, position.y + 1, position.z, 0]);
        }
        else if (dir == new Int3(-1, 0, 0))
        {
            Destroy(walls[position.x, position.y, position.z, 2]);
            Destroy(walls[position.x - 1, position.y, position.z, 3]);
        }
        else if (dir == new Int3(1, 0, 0))
        {
            Destroy(walls[position.x, position.y, position.z, 3]);
            Destroy(walls[position.x + 1, position.y, position.z, 2]);
        }
        else if (dir == new Int3(0, 0, -1))
        {
            Destroy(walls[position.x, position.y, position.z, 4]);
            Destroy(walls[position.x, position.y, position.z - 1, 5]);
        }
        else if (dir == new Int3(0, 0, 1))
        {
            Destroy(walls[position.x, position.y, position.z, 5]);
            Destroy(walls[position.x, position.y, position.z + 1, 4]);
        }
    }

    private bool ContainsWall(Int3 dir)
    {
        if (dir == new Int3(0, -1, 0))
            return walls[position.x, position.y, position.z, 0] != null;
        else if (dir == new Int3(0, 1, 0))
            return walls[position.x, position.y, position.z, 1] != null;
        else if (dir == new Int3(-1, 0, 0))
            return walls[position.x, position.y, position.z, 2] != null;
        else if (dir == new Int3(1, 0, 0))
            return walls[position.x, position.y, position.z, 3] != null;
        else if (dir == new Int3(0, 0, -1))
            return walls[position.x, position.y, position.z, 4] != null;
        else if (dir == new Int3(0, 0, 1))
            return walls[position.x, position.y, position.z, 5] != null;
        else
            throw new System.Exception();
    }

    private void CreateWalls()
    {
        walls = new GameObject[Size.x, Size.y, Size.z, 6];
        visited = new bool[Size.x + 2, Size.y + 2, Size.z + 2];
        for (int i = 0; i < Size.x; i++)
        {
            for (int j = 0; j < Size.y; j++)
            {
                for (int k = 0; k < Size.z; k++)
                {
                    walls[i, j, k, 0] = Instantiate(GreenWall, new Vector3(scale * i, scale * (j - 0.5f), scale * k), Quaternion.Euler(0.0f, 0.0f, 0.0f));      // 0 bottom
                    walls[i, j, k, 1] = Instantiate(GreenWall, new Vector3(scale * i, scale * (j + 0.5f), scale * k), Quaternion.Euler(180.0f, 0.0f, 0.0f));    // 1 top
                    walls[i, j, k, 2] = Instantiate(RedWall, new Vector3(scale * (i - 0.5f), scale * j, scale * k), Quaternion.Euler(0.0f, 0.0f, -90.0f));      // 2 left
                    walls[i, j, k, 3] = Instantiate(RedWall, new Vector3(scale * (i + 0.5f), scale * j, scale * k), Quaternion.Euler(0.0f, 0.0f, 90.0f));       // 3 right
                    walls[i, j, k, 4] = Instantiate(BlueWall, new Vector3(scale * i, scale * j, scale * (k - 0.5f)), Quaternion.Euler(90.0f, 0.0f, 0.0f));      // 4 back
                    walls[i, j, k, 5] = Instantiate(BlueWall, new Vector3(scale * i, scale * j, scale * (k + 0.5f)), Quaternion.Euler(-90.0f, 0.0f, 0.0f));     // 5 front
                }
            }
        }
        for (int i = 0; i < Size.x + 2; i++)
        {
            for (int j = 0; j < Size.y + 2; j++)
            {
                visited[i, j, 0] = true;
                visited[i, j, Size.z + 1] = true;
            }
        }
        for (int i = 0; i < Size.x + 2; i++)
        {
            for (int j = 0; j < Size.z + 2; j++)
            {
                visited[i, 0, j] = true;
                visited[i, Size.y + 1, j] = true;
            }
        }
        for (int i = 0; i < Size.y + 2; i++)
        {
            for (int j = 0; j < Size.z + 2; j++)
            {
                visited[0, i, j] = true;
                visited[Size.x + 1, i, j] = true;
            }
        }
    }

    private int NumInnerWalls()
    {
        return Size.x * Size.y * (Size.z - 1) + Size.x * Size.z * (Size.y - 1) + Size.y * Size.z * (Size.x - 1) - (Size.x * Size.y * Size.z - 1);
    }

    private void DeleteWalls()
    {
        if (walls == null)
            return;
        for (int i = 0; i < walls.GetLength(0); i++)
        {
            for (int j = 0; j < walls.GetLength(1); j++)
            {
                for (int k = 0; k < walls.GetLength(2); k++)
                {
                    for (int m = 0; m < walls.GetLength(3); m++)
                    {
                        if (walls[i, j, k, m] == null)
                            continue;
                        Destroy(walls[i, j, k, m]);
                    }
                }
            }
        }
    }
}
