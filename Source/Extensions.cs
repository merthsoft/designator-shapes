using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Verse;

namespace Merthsoft {
    public static class Extensions {
        static BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;

        public static object GetInstanceField(this object instance, string fieldName) {
            var type = instance.GetType();
            var field = type.GetField(fieldName, fieldFlags);
            return field.GetValue(instance); 
        }

        public static T GetInstanceField<T>(this object instance, string fieldName) where T : class {
            var type = instance.GetType();
            var field = type.GetField(fieldName, fieldFlags);
            return field.GetValue(instance) as T;
        }

        public static void SetInstanceField<T>(this object instance, string fieldName, T value) {
            var type = instance.GetType();
            var field = type.GetField(fieldName, fieldFlags);
            field.SetValue(instance, value);
        }


        public static void InvokeStaticMethod(this Type type, string methodName, params object[] methodParams) {
            MethodInfo dynMethod = type.GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
            dynMethod.Invoke(null, methodParams);
        }

        public static void InvokeMethod(this object obj, string methodName, params object[] methodParams) {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, fieldFlags);
            dynMethod.Invoke(obj, methodParams);
        }
        
        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action) {
            foreach (T t in list) {
                action(t);
            }
        }

        public static List<TResult> SelectList<T, TResult>(this IEnumerable<T> i, Func<T, TResult> f) => i.Select(f).ToList();

        public static IntVec3 Halve(this IntVec3 vec) => new IntVec3(vec.x / 2, vec.y / 2, vec.z / 2);

        public static IEnumerable<int> Range(this int count, int startValue = 0, int? by = null) {
            if (!by.HasValue) {
                by = count.Sign();
                count = count.Magnitude();
            }

            for (int i = 0; i < count; i++) {
                yield return startValue;
                startValue += by.Value;
            }
        }

        public static int Magnitude(this int i)
            => Math.Abs(i);

        public static int Sign(this int i)
            => Math.Sign(i);

        public static float Or(this float? v, float or)
            => v.GetValueOrDefault(or);

        public static float Add(this float lhs, float? rhs)
            => lhs + rhs.GetValueOrDefault();

        public static Rect Clone(this Rect r,
            float? x = null,
            float? y = null,
            float? width = null,
            float? height = null,
            float? addX = null,
            float? addY = null,
            float? addWidth = null,
            float? addHeight = null)
            => new Rect(x.Or(r.x.Add(addX)),
                        y.Or(r.y.Add(addY)),
                        width.Or(r.width.Add(addWidth)),
                        height.Or(r.height.Add(addHeight)));
    }
}
