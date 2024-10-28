using UnityEngine;
using UnityEngine.U2D; 

public static class SplineUtil
{
    public static void SetPoint(this Spline spline, int index, Vector2 pos)//���ÿ��Ƶ�
    {
        if (index < 0)
        {
            Debug.LogError("Control point index is non-negative.");
            return;
        }
        if (index >= spline.GetPointCount())
        {
            spline.AddPoint(index, pos);
        }
        else
        {
            spline.MovePoint(index, pos);
        }

        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
    }
    public static void SetPoint(this Spline spline, int index, Vector2 pos, Vector2 beforePos, Vector2 afterPos)//���ÿ��Ƶ�
    {
        spline.SetPoint(index, pos);
        spline.SetTangent(index, Snake.CurveTangentLength * Vector2.right);
        var angleVector = -Vector2.Perpendicular(Vector2.right.Rotate((beforePos - pos).Bisector(afterPos - pos)));
        if (beforePos.x > afterPos.x)
            angleVector = - angleVector;
        spline.SetTangentAngle(index, angleVector.Angle());
        //spline.SetTangentAngle(index, ((beforePos - pos).Bisector(afterPos - pos) + Mathf.PI) % (2*Mathf.PI));
    }
    public static void MovePoint(this Spline spline, int index, Vector3 newPosition)// �ƶ����Ƶ�
    {
        if (index < 0 || index >= spline.GetPointCount())
        {
            Debug.LogError("Control point index is out of range.");
            return;
        }

        spline.SetPosition(index, newPosition);
    }
    public static void AddPoint(this Spline spline, int index, Vector3 position)// ��ӿ��Ƶ�
    {
        spline.InsertPointAt(index, position);
    }
    public static void RemovePoint(this Spline spline, int index)// ɾ�����Ƶ�
    {
        if (index >= 0 && index < spline.GetPointCount())
        {
            spline.RemovePointAt(index);
        }
    }
    public static void Clear(this Spline spline)//��տ��Ƶ�
    {
        spline.Clear();
    }
    public static void SetTangentMode(this Spline spline, int index, ShapeTangentMode mode)//���ÿ��Ƶ�ģʽ
    {
        spline.SetTangentMode(index, mode);
    }
    public static void SetTangent(this Spline spline, int index, Vector2 tangent)//���ÿ��Ƶ�����
    {
        spline.SetRightTangent(index, tangent);
        spline.SetLeftTangent(index, -tangent);
    }
    public static void SetTangentAngle(this Spline spline, int index, float rad)//���ÿ��Ƶ����߷���
    {
        var tangent = spline.GetLeftTangent(index);
        tangent = Vector2.right.Rotate(rad) * tangent.magnitude;
        spline.SetTangent(index, tangent);
    }
    public static void SetTangentLength(this Spline spline, int index, float length)//���ÿ��Ƶ����߳��ȣ����ʣ�
    {
        var tangent = spline.GetLeftTangent(index);
        tangent.Normalize();
        tangent *= length;
        spline.SetTangent(index, tangent);
    }
}

public static class AngleUtil
{
    public static float Angle(this Vector2 vector)//��÷���
    {
        if (Mathf.Atan2(vector.y, vector.x) < 0)
            return Mathf.Atan2(vector.y, vector.x) + 2 * Mathf.PI;
        return Mathf.Atan2(vector.y, vector.x);
    }
    public static float IncludedAngle(this Vector2 v1, Vector2 v2)//��üн�(������)(v1��ʱ��ת���ٵõ�v2)
    {
        if (Angle(v2) - Angle(v1) > Mathf.PI)
            return (Angle(v2) - Angle(v1)) - 2 * Mathf.PI;
        return Angle(v2) - Angle(v1);
    }
    public static float AbsIncludedAngle(this Vector2 v1, Vector2 v2)//��üн�
    {
        return Mathf.Abs(IncludedAngle(v1, v2));
    }
    public static Vector2 Rotate(this Vector2 vector, float rad)//��ʱ����ת
    {
        Quaternion rotation = Quaternion.Euler(0, 0, rad * Mathf.Rad2Deg);
        return rotation * vector;
    }
    public static float Bisector(this Vector2 v1, Vector2 v2)//��ý�ƽ���ߵķ���
    {
        if ((Angle(v1) + Angle(v2)) / 2 <= Mathf.PI)
            return (Angle(v1) + Angle(v2)) / 2;
        return (Angle(v1) + Angle(v2)) / 2 - Mathf.PI;
    }
}

public class CameraBorder
{
    public static float xMin
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).x;
        }
    }
    public static float xMax
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).x;
        }
    }
    public static float yMin
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(0, 0, 0)).y;
        }
    }
    public static float yMax
    {
        get
        {
            return Camera.main.ViewportToWorldPoint(new Vector3(1, 1, 0)).y;
        }
    }
    public static Vector2 UpperLeft
    {
        get
        {
            return new Vector2(xMin, yMax);
        }
    }
    public static Vector2 UpperRight
    {
        get
        {
            return new Vector2(xMax, yMax);
        }
    }
    public static Vector2 LowerLeft
    {
        get
        {
            return new Vector2(xMin, yMin);
        }
    }
    public static Vector2 LowerRight
    {
        get
        {
            return new Vector2(xMax, yMin);
        }
    }
}


