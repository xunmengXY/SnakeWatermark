using UnityEngine;
using System.Collections.Generic;
using UnityEngine.U2D;
using System.Linq;

public class Snake
{
    //���ɳ���

    public const int NumberOfNodes = 50; //�߽ڵ�����
    public const float NodeDistance = 0.5f; // ÿ���߽ڵ�֮��ľ���
    public const float CurveTangentLength = 0.2f; //������Բ���̶�

    public const int CheckOutOfScreenTime = 50;//����֡�����һ�����Ƿ����Ļ

    //����Ϊ����

    public const float InitSpeed = (MaxSpeed + MinSpeed) / 2;//���ٶȴ�С
    public const float MaxSpeed = 2.5f;//����ٶ�
    public const float MinSpeed = 1.5f;//��С�ٶ�
    public const float MaxDeviationAngle = 1f;//ƫ�����������Ƕ�
    
    public const float BasicNormalExpectation = 0.01f;//�����������ѧ����
    public const float BasicNormalRange = 0.004f;//���������������Χ���뼫�
    public const float MicroNormalRange = 0.001f;//���������������Χ���뼫�
    public const int NormalTimeExpectation = 800;//�������ʱ����ѧ����
    public const int NormalTimeRange = 200;//�������ʱ�䲨����Χ���뼫�
    
    public const float BasicTangentExpectation = 0.001f;//�����������ѧ����
    public const float BasicTangentRange = 0.0005f;//���������������Χ���뼫�
    public const float MicroTangentRange = 0.0001f;//���������������Χ���뼫�
    public const int TangentTimeExpectation = 500;//�������ʱ����ѧ����
    public const int TangentTimeRange = 200;//�������ʱ�䲨����Χ���뼫�

    //�Ǿ�̬����

    public Vector2 TendDirection;//������

    private GameObject RootObject;//�ߺ��Ľڵ�
    private Spline Spline;
    private GameObject[] SnakeNodes = new GameObject[NumberOfNodes]; // �洢�����߽ڵ�
    private Vector2[] Positions = new Vector2[NumberOfNodes];//�洢������λ��
    private GameObject Head{ get { return SnakeNodes[0]; } }

    private float BasicNormal;//��ǰ����������
    private float BasicTangent;//��ǰ����������

    private int NormalChangeTime;//������һ�θı䷨����������ж���֡
    private int TangentChangeTime;//������һ�θı�������������ж���֡

    private int IsFront;//��������ǰ���Ǻ�
    private int IsRight; //���������һ�����

    private Vector2 PreviousVelo;//��һ֡�ٶ�����
    private Vector2 CurrentVelo;//��֡�ٶ�����

    public static bool Generate()//���������������ߣ�����ʧ�ܷ���false
    {
        System.Random random = new();
        var startPair = SnakeManager.SpawnField.ElementAt(random.Next(SnakeManager.SpawnField.Count));
        return Generate(startPair.Key, startPair.Value);
    }
    public static bool Generate(Vector2 startPos, Vector2 tendDirection)//ָ��λ�ú������������ߣ�����ʧ�ܷ���false
    {
        //�����ײ���Ƿ��ص�
        Vector2 instDrift = -tendDirection.normalized * NodeDistance;
        List<Collider2D> hitColliders = new();
        for (int i = 0; i < NumberOfNodes; i++)
        {
            hitColliders.AddRange(Physics2D.OverlapCircleAll(startPos + i * instDrift, NodeDistance * 1.5f));
        }
        if (hitColliders.Count != 0)
            return false;

        //������
        SnakeManager.SnakeSet.Add(new Snake(startPos, tendDirection));
        return true;
    }
    private Snake(Vector2 startPos, Vector2 tendDirection)
    {
        //���������

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

        //���帨������
        GameObject previousNode = null;
        GameObject currentNode;
        Vector2 previousPos = startPos;
        Vector2 currentPos;
        Vector2 instDrift = -tendDirection.normalized * NodeDistance;
        Rigidbody2D rigidbody;

        //�����߽ڵ�
        for (int i = 0; i < NumberOfNodes; i++)
        {
            // ����λ��
            if (i == 0)
                currentPos = startPos;
            else
                currentPos = previousPos + instDrift;

            // �����߽ڵ�
            currentNode = SnakeManager.Instantiate(SnakeManager.SnakeNodePrefab, currentPos, Quaternion.identity);
            currentNode.name = "Node" + i;
            currentNode.transform.SetParent(RootObject.transform); 
            SnakeNodes[i] = currentNode;

            //����Rigidbody
            rigidbody = currentNode.GetComponent<Rigidbody2D>();
            rigidbody.isKinematic = false;
            if (i == 0)
                rigidbody.mass = 1;//��������ͷ������������ͬ

            // ���DistanceJoint������
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

            //����Collider
            currentNode.GetComponent<CircleCollider2D>().radius = NodeDistance / 2.01f;

            // ����λ��
            previousPos = currentPos;
            previousNode = currentNode;
        }

        //�趨���ٶ�
        Head.GetComponent<Rigidbody2D>().velocity = CurrentVelo;
    }
    public bool TransportToStart()
    {
        //���������һ������
        System.Random random = new();
        KeyValuePair<Vector2, Vector2> startPair;
        for (int i = 0; i < 5; i++)
        {
            startPair = SnakeManager.SpawnField.ElementAt(random.Next(SnakeManager.SpawnField.Count));
            if (Reset(startPair.Key, startPair.Value))
                return true;
        }

        ////������ÿһ��������,��������
        //foreach (var pair in SnakeManager.SpawnField)
        //{
        //    if (Reset(pair.Key, pair.Value))
        //        return true;
        //}

        //��Ȼ����ʧ��
        return false;
    }
    public bool Reset(Vector2 startPos, Vector2 tendDirection)
    {
        Vector2[] poss = new Vector2[NumberOfNodes];

        //�����ײ���Ƿ��ص�,�ص�����false
        for (int i = 0; i < Positions.Length; i++)
        {
            poss[i] = startPos + i * (-tendDirection.normalized * NodeDistance); //�ı��ߴ�ֱ�ڽ����
            //poss[i] = Positions[i] - (Positions[0] - startPair.Key); //ԭ���ᶯ�ߣ��������ͻȻ��������Ļ�м�

            if (Physics2D.OverlapCircleAll(poss[i], NodeDistance * 1.5f).Length != 0)
                return false;
        }

        //�趨�ߵ�λ�ã����ٶȺ�����
        for (int i = 0; i < Positions.Length; i++)
        {
            SnakeNodes[i].transform.position = poss[i];
        }
        TendDirection = tendDirection;
        Head.GetComponent<Rigidbody2D>().velocity = tendDirection.normalized * InitSpeed;

        //��Ⱦԭ������RootObject
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
        //�洢֮ǰ���ٶ�
        PreviousVelo = CurrentVelo;

        //�洢λ��
        for (int i = 0; i < Positions.Length; i++)
        {
            Positions[i] = SnakeNodes[i].transform.position;
        }

        //������ͼ
        for (int i = 0; i < SnakeNodes.Length; i++)
        {
            if (i != 0 && i != SnakeNodes.Length - 1)
                Spline.SetPoint(i, Positions[i], Positions[i - 1], Positions[i + 1]);
            else
                Spline.SetPoint(i, Positions[i]);
        }   

        //��������
        if (NormalChangeTime <= 0)
        {
            IsRight = -IsRight;

            //���ɻ������Ĵ�С
            BasicNormal = Random.Range(BasicNormalExpectation - BasicNormalRange, BasicNormalExpectation + BasicNormalRange);

            //����ʱ��
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

            //���ɻ������Ĵ�С
            BasicTangent = Random.Range(BasicTangentExpectation - BasicTangentRange, BasicTangentExpectation + BasicTangentRange);

            //����ʱ��
            TangentChangeTime = Random.Range(TangentTimeExpectation - TangentTimeRange, TangentTimeExpectation + TangentTimeRange);

            Debug.Log($"T change:{BasicTangent * IsFront}\nspeed{CurrentVelo.magnitude}\n" );
        }
        else
        {
            TangentChangeTime--;
        }

        //Ӧ�������뷨����
        CurrentVelo += (BasicTangent + Random.Range(-MicroTangentRange, MicroTangentRange)) * IsFront * CurrentVelo.normalized;

        CurrentVelo = CurrentVelo.Rotate(
            (BasicNormal + Random.Range(-MicroNormalRange, MicroNormalRange)) * IsRight * CurrentVelo.magnitude//�ٶȳ���
            );

        //����ƫ��Ƕ�
        if (CurrentVelo.IncludedAngle(TendDirection ) > MaxDeviationAngle)
        {
            CurrentVelo = CurrentVelo.magnitude * TendDirection.normalized.Rotate(-MaxDeviationAngle);
        }
        else if (CurrentVelo.IncludedAngle(TendDirection) < -MaxDeviationAngle)
        {
            CurrentVelo = CurrentVelo.magnitude *TendDirection.normalized.Rotate(MaxDeviationAngle);
        }
     
        //���������С�ٶ�
        if (CurrentVelo.magnitude > MaxSpeed)
        {
            CurrentVelo = CurrentVelo.normalized * MaxSpeed;
        }
        if (CurrentVelo.magnitude < MinSpeed)
        {
            CurrentVelo = CurrentVelo.normalized * MinSpeed;
        }

        //ʵ�������ٶ�
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