using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Verse;

namespace Merthsoft {
    public static class Helpers {
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
    }
}
