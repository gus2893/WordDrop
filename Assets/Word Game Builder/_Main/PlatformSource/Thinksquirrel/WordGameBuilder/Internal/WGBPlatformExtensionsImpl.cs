// WGBPlatformExtensionsImpl.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Thinksquirrel.WordGameBuilder.Internal
{
    //! \cond PRIVATE
    public class WGBPlatformExtensionsImpl : IWGBPlatformExtensions
    {
        public PropertyInfo ExtGetProperty(Type type, string propertyName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
#else
            return type.GetProperty(propertyName);
#endif
        }
        public EventInfo ExtGetEvent(Type type, string eventName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredEvent(eventName);
#else
            return type.GetEvent(eventName);
#endif
        }
        public MethodInfo ExtGetMethod(Type type, string methodName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
#else
            return type.GetMethod(methodName, Type.EmptyTypes);
#endif
        }
        public IEnumerable<MethodInfo> ExtGetMethods(Type type, string methodName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredMethods(methodName);
#else
            return type.GetMethods().Where(m => m.Name == methodName);
#endif
        }
        public FieldInfo ExtGetField(Type type, string fieldName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredField(fieldName);
#else
            return type.GetField(fieldName);
#endif
        }
        public Type ExtGetNestedType(Type type, string typeName)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return type.GetTypeInfo().GetDeclaredNestedType(typeName).AsType();
#else
            return type.GetNestedType(typeName);
#endif
        }
        public Delegate ExtCreateDelegate(MethodInfo methodInfo, Type delegateType, object target)
        {
#if UNITY_WINRT && !UNITY_EDITOR
            return methodInfo.CreateDelegate(delegateType, target);
#else
            return Delegate.CreateDelegate(delegateType, target, methodInfo);
#endif
        }
        //! \endcond
    }
}
