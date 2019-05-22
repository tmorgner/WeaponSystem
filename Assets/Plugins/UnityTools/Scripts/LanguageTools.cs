using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq.Expressions;

namespace RabbitStewdio.Unity.UnityTools
{
    public static class LanguageTools
    {
        static readonly ConcurrentDictionary<Type, object> cache;

        static LanguageTools()
        {
            cache = new ConcurrentDictionary<Type, object>();
        }

        public static bool IsFlagSet<T>(this T value, T flag) where T : struct, IComparable, IFormattable, IConvertible
        {
            Func<T, T, bool> fn;
            if (!cache.TryGetValue(typeof(T), out var maybeFn))
            {
                fn = GenerateHasFlag<T>();
                cache[typeof(T)] = fn;
            }
            else if (maybeFn is Func<T, T, bool> fn2)
            {
                fn = fn2;
            }
            else
            {
                var valueLong = value.ToInt64(NumberFormatInfo.CurrentInfo);
                var flagLong = flag.ToInt64(NumberFormatInfo.CurrentInfo);
                return (valueLong & flagLong) == flagLong;
            }

            return fn.Invoke(value, flag);
        }


#if !ENABLE_IL2CPP        
        static Func<T, T, bool> GenerateHasFlag<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            var value = Expression.Parameter(typeof(T));
            var flag = Expression.Parameter(typeof(T));

            // Convert from Enum to underlying type (byte, int, long, ...)
            // to allow bitwise functions to work
            UnaryExpression valueConverted = Expression.Convert(value, Enum.GetUnderlyingType(typeof(T)));
            UnaryExpression flagConverted = Expression.Convert(flag, Enum.GetUnderlyingType(typeof(T)));

            // (Value & Flag)
            BinaryExpression bitwiseAnd =
                Expression.MakeBinary(
                    ExpressionType.And,
                    valueConverted,
                    flagConverted);

            // (Value & Flag) == Flag
            BinaryExpression hasFlagExpression =
                Expression.MakeBinary(ExpressionType.Equal, bitwiseAnd, flagConverted);

            return Expression.Lambda<Func<T, T, bool>>(hasFlagExpression, value, flag)
                             .Compile();
        }
#else
        static Func<T, T, bool> GenerateHasFlag<T>() where T : struct, IComparable, IFormattable, IConvertible
        {
            return (value, flag) =>
            {
                var valueLong = value.ToInt64(NumberFormatInfo.CurrentInfo);
                var flagLong = flag.ToInt64(NumberFormatInfo.CurrentInfo);
                return (valueLong & flagLong) == flagLong;
            };
        }
#endif        
        
    }
}