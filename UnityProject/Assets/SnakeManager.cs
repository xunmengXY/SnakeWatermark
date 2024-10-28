using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeManager : MonoBehaviour
{
    public static int CurrentCheckOutOfScreenTime = Snake.CheckOutOfScreenTime;//距离下一次检查出屏幕还有多少帧
    private static int CheckIndex = 0;//当前检测是否出屏幕的蛇

    public static Dictionary<Vector2, Vector2> SpawnField = new();//生成域（生成点->趋向方向 的字典）
    public static HashSet<Snake> SnakeSet = new();//储存所有蛇

    public static GameObject RootPrefab; // 蛇核心
    public static GameObject SnakeNodePrefab; // 蛇节点

    public GameObject _SnakeNodePrefab; 
    public GameObject _RootPrefab; 

    void Start()
    {
        //便于从检查器里设置GameObject
        RootPrefab = _RootPrefab;
        SnakeNodePrefab = _SnakeNodePrefab;

        //移走防止遮挡
        RootPrefab.transform.position = new Vector3(100, 100, 0);
        SnakeNodePrefab.transform.position = new Vector3(100, 100, 0);

        //确定生成域
        for (float i = 0; i < 0.99f; i += 0.05f)
        {
            SpawnField.Add(i * CameraBorder.UpperLeft + (1 - i) * CameraBorder.UpperRight, Vector2.down);
            SpawnField.Add(i * CameraBorder.LowerRight + (1 - i) * CameraBorder.LowerLeft, Vector2.up);
            SpawnField.Add(i * CameraBorder.LowerLeft + (1 - i) * CameraBorder.UpperLeft, Vector2.right);
            SpawnField.Add(i * CameraBorder.UpperRight + (1 - i) * CameraBorder.LowerRight, Vector2.left);
        }

        //生成初始在屏幕中间的蛇
        for (int i = 0; i < 10; i++)
        {
            Vector2 start = new Vector2(Random.Range(CameraBorder.xMin, CameraBorder.xMax), Random.Range(CameraBorder.yMin, CameraBorder.yMax));
            Vector2 tend = new Vector2(Random.Range(CameraBorder.xMin, CameraBorder.xMax), Random.Range(CameraBorder.yMin, CameraBorder.yMax));
            Snake.Generate(start, tend);
        }

        //生成屏幕四周的蛇
        for (int i = 0; i < 10; i++) Snake.Generate();

        //Camera.main.orthographicSize *= 1.1f;
    }
    void Update()
    {
        foreach (Snake snake in SnakeSet)
        {
            snake.SnakeUpdate();
        }
        if (CurrentCheckOutOfScreenTime <= 0)
        {
            CheckIndex++;
            if(CheckIndex >= SnakeSet.Count || CheckIndex < 0)
                CheckIndex = 0;
            var snake = SnakeSet.ElementAt(CheckIndex);
            if (snake.CheckOutOfScreen())
                snake.TransportToStart();
    
            CurrentCheckOutOfScreenTime = Snake.CheckOutOfScreenTime;
        }
        else
        {
            CurrentCheckOutOfScreenTime--;
        }
    }
}
