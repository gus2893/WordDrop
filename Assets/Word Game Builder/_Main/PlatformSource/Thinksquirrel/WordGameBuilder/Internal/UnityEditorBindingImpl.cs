// UnityEditorBindingImpl.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Thinksquirrel.WordGameBuilder.Internal
{
    [InitializeOnLoad]
    static class UnityEditorBindingInitialization
    {
        static UnityEditorBindingInitialization()
        {
            UnityEditorBinding.SetImplementation(new UnityEditorBindingImpl());
        }    
    }
    class UnityEditorBindingImpl : IUnityEditorBinding
    {
        Assembly m_EditorAssembly;
        
        public void LoadAssembly()
        {
            // Get the Unity Editor assembly
            m_EditorAssembly = Assembly.Load("UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        }
       
        public object RunStatic(string method, params object[] arguments)
        {
            if (m_EditorAssembly == null)
                LoadAssembly();
                
            if (m_EditorAssembly == null)
                return null;
            
            // Get the type
            var typeString = method.Substring(0, method.LastIndexOf('.'));
            var type = m_EditorAssembly.GetType(string.Format("UnityEditor.{0}", typeString));
            
            if (type == null)
                return null;
            
            // Get the Method
            var methodString = method.Substring(method.LastIndexOf('.') + 1);
            var methodInfo = type.GetMethod(methodString, BindingFlags.Static | BindingFlags.Public);
            
            return methodInfo == null ? null : methodInfo.Invoke(null, arguments);
        }
        public object RunInstance(object instance, string method, params object[] arguments)
        {
            if (m_EditorAssembly == null)
                LoadAssembly();
                
            if (m_EditorAssembly == null)
                return null;
            
            // Get the type
            var type = instance.GetType();
            
            // Get the Method
            var methodString = method.Substring(method.LastIndexOf('.') + 1);
            var methodInfo = type.GetMethod(methodString, BindingFlags.Instance | BindingFlags.Public);
            
            return methodInfo == null ? null : methodInfo.Invoke(instance, arguments);
        }
    }
}
#endif