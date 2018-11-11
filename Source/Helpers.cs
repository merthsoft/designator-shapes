using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Merthsoft {
    public static class Helpers {
        static BindingFlags fieldFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.GetField | BindingFlags.GetProperty;

        public static T GetBaseInstanceField<T>(this object instance, string fieldName) where T : class {
            var type = instance.GetType().BaseType;
            var field = type.GetField(fieldName, fieldFlags);
            return field.GetValue(instance) as T;
        }

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

        public static void SetBaseInstanceField<T>(this object instance, string fieldName, T value) {
            var type = instance.GetType().BaseType;
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

        public static T InvokeMethod<T>(this object obj, string methodName, params object[] methodParams) where T : class {
            MethodInfo dynMethod = obj.GetType().GetMethod(methodName, fieldFlags);
            return dynMethod.Invoke(obj, methodParams) as T;
        }

        public static void InvokeBaseMethod(this object obj, string methodName, params object[] methodParams) {
            MethodInfo dynMethod = obj.GetType().BaseType.GetMethod(methodName, fieldFlags);
            dynMethod.InvokeNotOverride(obj, methodParams);
        }

        public static T InvokeBaseMethod<T>(this object obj, string methodName, params object[] methodParams) where T : class {
            MethodInfo dynMethod = obj.GetType().BaseType.GetMethod(methodName, fieldFlags);
            return dynMethod.InvokeNotOverride(obj, methodParams) as T;
        }

        public static object InvokeNotOverride(this MethodInfo methodInfo, object targetObject, params object[] arguments) {
            var parameters = methodInfo.GetParameters();

            if (parameters.Length == 0) {
                if (arguments != null && arguments.Length != 0)
                    throw new Exception("Arguments cont doesn't match");
            } else {
                if (parameters.Length != arguments.Length)
                    throw new Exception("Arguments cont doesn't match");
            }

            Type returnType = null;
            if (methodInfo.ReturnType != typeof(void)) {
                returnType = methodInfo.ReturnType;
            }

            var type = targetObject.GetType();
            var dynamicMethod = new DynamicMethod("", returnType,
            new Type[] { type, typeof(Object) }, type);

            var iLGenerator = dynamicMethod.GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldarg_0); // this

            for (var i = 0; i < parameters.Length; i++) {
                var parameter = parameters[i];

                iLGenerator.Emit(OpCodes.Ldarg_1); // load array argument

                // get element at index
                iLGenerator.Emit(OpCodes.Ldc_I4_S, i); // specify index
                iLGenerator.Emit(OpCodes.Ldelem_Ref); // get element

                var parameterType = parameter.ParameterType;
                if (parameterType.IsPrimitive) {
                    iLGenerator.Emit(OpCodes.Unbox_Any, parameterType);
                } else if (parameterType == typeof(object)) {
                    // do nothing
                } else {
                    iLGenerator.Emit(OpCodes.Castclass, parameterType);
                }
            }

            iLGenerator.Emit(OpCodes.Call, methodInfo);
            iLGenerator.Emit(OpCodes.Ret);

            return dynamicMethod.Invoke(null, new object[] { targetObject, arguments });
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action) {
            foreach (T t in list) {
                action(t);
            }
        }

        public static List<TResult> SelectList<T, TResult>(this List<T> i, Func<T, TResult> f) => i.Select(f).ToList();
    }
}
