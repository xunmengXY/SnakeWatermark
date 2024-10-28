using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SnakeManager : MonoBehaviour
{
    public static int CurrentCheckOutOfScreenTime = Snake.CheckOutOfScreenTime;//������һ�μ�����Ļ���ж���֡
    private static int CheckIndex = 0;//��ǰ����Ƿ����Ļ����

    public static Dictionary<Vector2, Vector2> SpawnField = new();//���������ɵ�->������ ���ֵ䣩
    public static HashSet<Snake> SnakeSet = new();//����������

    public static GameObject RootPrefab; // �ߺ���
    public static GameObject SnakeNodePrefab; // �߽ڵ�

    public GameObject _SnakeNodePrefab; 
    public GameObject _RootPrefab; 

    void Start()
    {
        //���ڴӼ����������GameObject
        RootPrefab = _RootPrefab;
        SnakeNodePrefab = _SnakeNodePrefab;

        //���߷�ֹ�ڵ�
        RootPrefab.transform.position = new Vector3(100, 100, 0);
        SnakeNodePrefab.transform.position = new Vector3(100, 100, 0);

        //ȷ��������
        for (float i = 0; i < 0.99f; i += 0.05f)
        {
            SpawnField.Add(i * CameraBorder.UpperLeft + (1 - i) * CameraBorder.UpperRight, Vector2.down);
            SpawnField.Add(i * CameraBorder.LowerRight + (1 - i) * CameraBorder.LowerLeft, Vector2.up);
            SpawnField.Add(i * CameraBorder.LowerLeft + (1 - i) * CameraBorder.UpperLeft, Vector2.right);
            SpawnField.Add(i * CameraBorder.UpperRight + (1 - i) * CameraBorder.LowerRight, Vector2.left);
        }

        //���ɳ�ʼ����Ļ�м����
        for (int i = 0; i < 10; i++)
        {
            Vector2 start = new Vector2(Random.Range(CameraBorder.xMin, CameraBorder.xMax), Random.Range(CameraBorder.yMin, CameraBorder.yMax));
            Vector2 tend = new Vector2(Random.Range(CameraBorder.xMin, CameraBorder.xMax), Random.Range(CameraBorder.yMin, CameraBorder.yMax));
            Snake.Generate(start, tend);
        }

        //������Ļ���ܵ���
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
