using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HPATile
{
    public Vector3 pos;
    public int x;
    public int y;
    public int penalty;
    public bool imPassable;
    public HPATile(Vector3 pos_, int x_,int y_,int penalty_,bool imPassable_)
    {
        pos = pos_;
        x = x_;
        y = y_;
        penalty = penalty_;
        imPassable = imPassable_;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;
        HPATile other = obj as HPATile;
        return x == other.x && y == other.y;
    }
    public override int GetHashCode()
    {
        return x ^ y;
    }

    public static bool operator !=(HPATile a,HPATile b)
    {
        if (ReferenceEquals(a, null))
            return !ReferenceEquals(b, null);
        else
            return !a.Equals(b);
    }
    public static bool operator ==(HPATile a, HPATile b)
    {
        if (ReferenceEquals(a, null))
            return ReferenceEquals(b, null);
        else
            return a.Equals(b);
    }

    public void Print()
    {
        Debug.Log("X : " + x + ", Y : " + y);
    }
}
