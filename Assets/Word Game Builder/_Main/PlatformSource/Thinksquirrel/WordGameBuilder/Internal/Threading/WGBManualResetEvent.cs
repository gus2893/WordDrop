// WGBManualResetEvent.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
using System;
using System.Threading;
using UnityEngine;

namespace Thinksquirrel.WordGameBuilder.Internal.Threading
{       
    class WGBManualResetEvent : IEventWaitHandle
    {
        #if !UNITY_WEBGL && !UNITY_WINRT
        readonly ManualResetEvent m_Event = new ManualResetEvent(false);
        #else
        bool m_Flag;
        #endif

        public bool WaitOne()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            return m_Event.WaitOne();
            #else
            return m_Flag;
            #endif
        }
        public bool WaitOne(TimeSpan timeout)
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            return m_Event.WaitOne(timeout);
            #else
            return m_Flag;
            #endif
        }
        public bool WaitOne(int millisecondsTimeout)
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            return m_Event.WaitOne(millisecondsTimeout);
            #else
            return m_Flag;
            #endif
        }
        public bool Set()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            return m_Event.Set();
            #else
            m_Flag = true;
            return true;
            #endif
        }
        public bool Reset()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            return m_Event.Reset();
            #else
            m_Flag = false;
            return true;
            #endif
        }
        public void Close()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            m_Event.Close();
            #endif
        }
    }
}
