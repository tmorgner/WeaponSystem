using System;
using System.Collections.Generic;

namespace RabbitStewdio.Unity.UnityTools
{
  public struct StructTuple<TA, TB, TC> : IEquatable<StructTuple<TA, TB, TC>>
  {
    public readonly TA Item1;
    public readonly TB Item2;
    public readonly TC Item3;

    public StructTuple(TA item1, TB item2, TC item3)
    {
      Item1 = item1;
      Item2 = item2;
      Item3 = item3;
    }

    public bool Equals(StructTuple<TA, TB, TC> other)
    {
      return EqualityComparer<TA>.Default.Equals(Item1, other.Item1) 
             && EqualityComparer<TB>.Default.Equals(Item2, other.Item2)
             && EqualityComparer<TC>.Default.Equals(Item3, other.Item3);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      return obj is StructTuple<TA, TB, TC> && Equals((StructTuple<TA, TB, TC>) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        var hashCode = EqualityComparer<TA>.Default.GetHashCode(Item1);
        hashCode = (hashCode * 397) ^ EqualityComparer<TB>.Default.GetHashCode(Item2);
        hashCode = (hashCode * 397) ^ EqualityComparer<TC>.Default.GetHashCode(Item3);
        return hashCode;
      }
    }

    public static bool operator ==(StructTuple<TA, TB, TC> left, StructTuple<TA, TB, TC> right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(StructTuple<TA, TB, TC> left, StructTuple<TA, TB, TC> right)
    {
      return !left.Equals(right);
    }

    public override string ToString()
    {
      return string.Format("({0}, {1}, {2})", Item1, Item2, Item3);
    }
  }

  public struct StructTuple<TA, TB> : IEquatable<StructTuple<TA, TB>>
  {
    public readonly TA Item1;
    public readonly TB Item2;

    public StructTuple(TA item1, TB item2)
    {
      Item1 = item1;
      Item2 = item2;
    }

    public bool Equals(StructTuple<TA, TB> other)
    {
      return EqualityComparer<TA>.Default.Equals(Item1, other.Item1) && EqualityComparer<TB>.Default.Equals(Item2, other.Item2);
    }

    public override bool Equals(object obj)
    {
      if (ReferenceEquals(null, obj))
        return false;
      return obj is StructTuple<TA, TB> && Equals((StructTuple<TA, TB>) obj);
    }

    public override int GetHashCode()
    {
      unchecked
      {
        return (EqualityComparer<TA>.Default.GetHashCode(Item1) * 397) ^ EqualityComparer<TB>.Default.GetHashCode(Item2);
      }
    }

    public static bool operator ==(StructTuple<TA, TB> left, StructTuple<TA, TB> right)
    {
      return left.Equals(right);
    }

    public static bool operator !=(StructTuple<TA, TB> left, StructTuple<TA, TB> right)
    {
      return !left.Equals(right);
    }

    public override string ToString()
    {
      return string.Format("({0}, {1})", Item1, Item2);
    }
  }

  public static class StructTuple
  {
    public static StructTuple<TA, TB> Create<TA, TB>(TA a, TB b)
    {
      return new StructTuple<TA, TB>(a,b);
    }

    public static StructTuple<TA, TB, TC> Create<TA, TB, TC>(TA a, TB b, TC c)
    {
      return new StructTuple<TA, TB, TC>(a,b,c);
    }
  }
}
