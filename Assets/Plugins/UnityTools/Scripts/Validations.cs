using System;

namespace RabbitStewdio.Unity.UnityTools
{
  public static class Validations
  {
    public static void AssertNotNull(object o, string parameterName = null)
    {
      if (o == null)
      {
        throw new ArgumentNullException(parameterName);
      }
    }

    public static void AssertGreaterThan<T>(T o, T min, string paramName) where T : struct, IComparable<T>
    {
      if (min.CompareTo(o) < 0)
      {
        throw new ArgumentOutOfRangeException(paramName, o, string.Format("must be larger than {0}", min));
      }
    } 

    public static void AssertRange(int o, int min, int max, string paramName)
    {
      if (o < min)
      {
        throw new ArgumentOutOfRangeException(paramName, o, string.Format("must be larger than {0}", min));
      }
      if (o >= max)
      {
        throw new ArgumentOutOfRangeException(paramName, o, string.Format("must be smaller than {0}", max));
      }
    }

    public static void AssertRange<T>(T o, T min, T max, string paramName) where T : struct, IComparable<T>
    {
      if (min.CompareTo(o) < 0)
      {
        throw new ArgumentOutOfRangeException(paramName, o, string.Format("must be larger than {0}", min));
      }
      if (max.CompareTo(o) >= 0)
      {
        throw new ArgumentOutOfRangeException(paramName, o, string.Format("must be smaller than {0}", max));
      }
    }
  }
}
