using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using System.Linq;

public class Snake
{
    //生成常量

    public const int NumberOfNodes = 50; //蛇节点数量
    public const float NodeDistance = 0.5f; // 每个蛇节点之间的距离
    public const float CurveTangentLength = 0.2f; //蛇曲线圆滑程度

    public const int CheckOutOfScreenTime = 50;//多少帧检测下一条蛇是否出屏幕

    //蛇行为常量

    public const float InitSpeed = (MaxSpeed + MinSpeed) / 2;//初速度大小
    public const float MaxSpeed = 2.5f;//最大速度
    public const float MinSpeed = 1.5f;//最小速度
    public const float MaxDeviationAngle = 1f;//偏离趋向方向最大角度
    
    public const float BasicNormalExpectation = 0.01f;//法向基础力数学期望
    public const float BasicNormalRange = 0.004f;//法向基础力波动范围（半极差）
    public const float MicroNormalRange = 0.001f;//法向随机力波动范围（半极差）
    public const int NormalTimeExpectation = 800;//法向持续时间数学期望
    public const int NormalTimeRange = 200;//法向持续时间波动范围（半极差）
    
    public const float BasicTangentExpectation = 0.001f;//切向基础力数学期望
    public const float BasicTangentRange = 0.0005f;//切向基础力波动范围（半极差）
    public const float MicroTangentRange = 0.0001f;//切向随机力波动范围（半极差）
    public const int TangentTimeExpectation = 500;//切向持续时间数学期望
    public const int TangentTimeRange = 200;//切向持续时间波动范围（半极差）

    //非静态变量

    public Vector2 TendDirection;//趋向方向

    private GameObject RootObject;//蛇核心节点
    private Spline Spline;
    private GameObject[] SnakeNodes = new GameObject[NumberOfNodes]; // 存储所有蛇节点
    private Vector2[] Positions = new Vector2[NumberOfNodes];//存储所有蛇位置
    private GameObject Head{ get { return SnakeNodes[0]; } }

    private float BasicNormal;//当前基础法向力
    private float BasicTangent;//当前基础切向力

    private int NormalChangeTime;//距离下一次改变法向基础力还有多少帧
    private int TangentChangeTime;//距离下一次改变切向基础力还有多少帧

    private int IsFront;//切向力向前还是后
    private int IsRight; //法向力向右还是左

    private Vector2 PreviousVelo;//上一帧速度向量
    private Vector2 CurrentVelo;//本帧速度向量

    public static bool Generate()//在生成域中生成蛇，生成失败返回false
    {
        System.Random random = new();
        var startPair = SnakeManager.SpawnField.ElementAt(random.Next(SnakeManager.SpawnField.Count));
        return Generate(startPair.Key, startPair.Value);
    }
    public static bool Generate(Vector2 startPos, Vector2 tendDirection)//指定位置和趋向方向生成蛇，生成失败返回false
    {
        //检查碰撞箱是否重叠
        Vector2 instDrift = -tendDirection.normalized * NodeDistance;
        List<Collider2D> hitColliders = new();
        for (int i = 0; i < NumberOfNodes; i++)
        {
            hitColliders.AddRange(Physics2D.OverlapCircleAll(startPos + i * instDrift, NodeDistance * 1.5f));
        }
        if (hitColliders.Count != 0)
            return false;

        //生成蛇
        SnakeManager.SnakeSet.Add(new Snake(startPos, tendDirection));
        return true;
    }
    private Snake(Vector2 startPos, Vector2 tendDirection)
    {
        //设置诸变量

        TendDirection = tendDirection;

        RootObject = SnakeManager.Instantiate(SnakeManager.RootPrefab, Vector3.zero, Quaternion.identity);
        RootObject.name = "Snake" + SnakeManager.SnakeSet.Count;
        RootObject.GetComponent<SpriteShapeController>().spline.Clear();

        BasicNormal = Random.Range(BasicNormalExpectation - BasicNormalRange, BasicNormalExpectation + BasicNormalRange);
        BasicTangent = Random.Range(BasicTangentExpectation - BasicTangentRange, BasicTangentExpectation + BasicTangentRange);

        NormalChangeTime = Random.Range(NormalTimeExpectation - NormalTimeRange, NormalTimeExpectation + NormalTimeRange);
        TangentChangeTime = Random.Range(TangentTimeExpectation - TangentTimeRange, TangentTimeExpectation + TangentTimeRange);

        IsFront = Random.value > 0.5 ? 1 : -1;
        IsRight = Random.value > 0.5 ? 1 : -1;

        PreviousVelo = tendDirection.normalized * InitSpeed;
        CurrentVelo = PreviousVelo;

        Spline = RootObject.GetComponent<SpriteShapeController>().spline;

        //定义辅助变量
        GameObject previousNode = null;
        GameObject currentNode;
        Vector2 previousPos = startPos;
        Vector2 currentPos;
        Vector2 instDrift = -tendDirection.normalized * NodeDistance;
        Rigidbody2D rigidbody;

        //生成蛇节点
        for (int i = 0; i < NumberOfNodes; i++)
        {
            // 更新位置
            if (i == 0)
                currentPos = startPos;
            else
                currentPos = previousPos + instDrift;

            // 创建蛇节点
            currentNode = SnakeManager.Instantiate(SnakeManager.SnakeNodePrefab, currentPos, Quaternion.identity);
            currentNode.name = "Node" + i;
            currentNode.transform.SetParent(RootObject.transform); 
            SnakeNodes[i] = currentNode;

            //配置Rigidbody
            rigidbody = currentNode.GetComponent<Rigidbody2D>();
            rigidbody.isKinematic = false;
            if (i == 0)
                rigidbody.mass = 1;//可以设置头和身体质量不同

            // 添加DistanceJoint并配置
            if (i != 0)
            {
                DistanceJoint2D distanceJoint = currentNode.AddComponent<DistanceJoint2D>();
                distanceJoint.connectedBody = previousNode.GetComponent<Rigidbody2D>();
                distanceJoint.autoConfigureConnectedAnchor = false;
                distanceJoint.autoConfigureDistance = false;
                distanceJoint.distance = NodeDistance;
                distanceJoint.maxDistanceOnly = false;
                distanceJoint.enableCollision = true;
            }

            //配置Collider
            currentNode.GetComponent<CircleCollider2D>().radius = NodeDistance / 2.01f;

            // 更新位置
            previousPos = currentPos;
            previousNode = currentNode;
        }

        //设定初速度
        Head.GetComponent<Rigidbody2D>().velocity = CurrentVelo;
    }
    public bool TransportToStart()
    {
        //尝试在随机一点生成
        System.Random random = new();
        KeyValuePair<Vector2, Vector2> startPair;
        for (int i = 0; i < 5; i++)
        {
            startPair = SnakeManager.SpawnField.ElementAt(random.Next(SnakeManager.SpawnField.Count));
            if (Reset(startPair.Key, startPair.Value))
                return true;
        }

        ////尝试在每一个点生成,不够均匀
        //foreach (var pair in SnakeManager.SpawnField)
        //{
        //    if (Reset(pair.Key, pair.Value))
        //        return true;
        //}

        //仍然生成失败
        return false;
    }
    public bool Reset(Vector2 startPos, Vector2 tendDirection)
    {
        Vector2[] poss = new Vector2[NumberOfNodes];

        //检测碰撞箱是否重叠,重叠返回false
        for (int i = 0; i < Positions.Length; i++)
        {
            poss[i] = startPos + i * (-tendDirection.normalized * NodeDistance); //改变蛇垂直于进入边
            //poss[i] = Positions[i] - (Positions[0] - startPair.Key); //原样搬动蛇，可能造成突然出现在屏幕中间

            if (Physics2D.OverlapCircleAll(poss[i], NodeDistance * 1.5f).Length != 0)
                return false;
        }

        //设定蛇的位置，初速度和趋向
        for (int i = 0; i < Positions.Length; i++)
        {
            SnakeNodes[i].transform.position = poss[i];
        }
        TendDirection = tendDirection;
        Head.GetComponent<Rigidbody2D>().velocity = tendDirection.normalized * InitSpeed;

        //渲染原因，重设RootObject
        var rootObject = SnakeManager.Instantiate(SnakeManager.RootPrefab, Vector3.zero, Quaternion.identity);
        rootObject.name = RootObject.name;
        rootObject.GetComponent<SpriteShapeController>().spline.Clear();
        foreach (var node in SnakeNodes)
        {
            node.transform.SetParent(rootObject.transform);
        }
        Spline = rootObject.GetComponent<SpriteShapeController>().spline;

        SnakeManager.Destroy(RootObject);
        RootObject = rootObject;

        return true;
    }
    public void SnakeUpdate()
    {
        //存储之前的速度
        PreviousVelo = CurrentVelo;

        //存储位置
        for (int i = 0; i < Positions.Length; i++)
        {
            Positions[i] = SnakeNodes[i].transform.position;
        }

        //附着贴图
        for (int i = 0; i < SnakeNodes.Length; i++)
        {
            if (i != 0 && i != SnakeNodes.Length - 1)
                Spline.SetPoint(i, Positions[i], Positions[i - 1], Positions[i + 1]);
            else
                Spline.SetPoint(i, Positions[i]);
        }   

        //调整受力
        if (NormalChangeTime <= 0)
        {
            IsRight = -IsRight;

            //生成基础力的大小
            BasicNormal = Random.Range(BasicNormalExpectation - BasicNormalRange, BasicNormalExpectation + BasicNormalRange);

            //设置时间
            NormalChangeTime = Random.Range(NormalTimeExpectation - NormalTimeRange, NormalTimeExpectation + NormalTimeRange);

            Debug.Log($"N change to{BasicNormal * IsRight}\nspeed{CurrentVelo.magnitude}\n" );
        }
        else
        {
            NormalChangeTime--;
        }
        if (TangentChangeTime <= 0)
        {
            IsFront = -IsFront;

            //生成基础力的大小
            BasicTangent = Random.Range(BasicTangentExpectation - BasicTangentRange, BasicTangentExpectation + BasicTangentRange);

            //设置时间
            TangentChangeTime = Random.Range(TangentTimeExpectation - TangentTimeRange, TangentTimeExpectation + TangentTimeRange);

            Debug.Log($"T change:{BasicTangent * IsFront}\nspeed{CurrentVelo.magnitude}\n" );
        }
        else
        {
            TangentChangeTime--;
        }

        //应用切向与法向力
        CurrentVelo += (BasicTangent + Random.Range(-MicroTangentRange, MicroTangentRange)) * IsFront * CurrentVelo.normalized;

        CurrentVelo = CurrentVelo.Rotate(
            (BasicNormal + Random.Range(-MicroNormalRange, MicroNormalRange)) * IsRight * CurrentVelo.magnitude//速度乘数
            );

        //限制偏离角度
        if (CurrentVelo.IncludedAngle(TendDirection ) > MaxDeviationAngle)
        {
            CurrentVelo = CurrentVelo.magnitude * TendDirection.normalized.Rotate(-MaxDeviationAngle);
        }
        else if (CurrentVelo.IncludedAngle(TendDirection) < -MaxDeviationAngle)
        {
            CurrentVelo = CurrentVelo.magnitude *TendDirection.normalized.Rotate(MaxDeviationAngle);
        }
     
        //限制最大最小速度
        if (CurrentVelo.magnitude > MaxSpeed)
        {
            CurrentVelo = CurrentVelo.normalized * MaxSpeed;
        }
        if (CurrentVelo.magnitude < MinSpeed)
        {
            CurrentVelo = CurrentVelo.normalized * MinSpeed;
        }

        //实际设置速度
        SnakeNodes[0].GetComponent<Rigidbody2D>().velocity = CurrentVelo;
    }
    public bool CheckOutOfScreen()
    {
        foreach (Vector2 pos in Positions)
        {
            if (!(pos.x > CameraBorder.xMax || pos.x < CameraBorder.xMin || pos.y > CameraBorder.yMax || pos.y < CameraBorder.yMin))
                return false;
        }
        return true;
    }
}