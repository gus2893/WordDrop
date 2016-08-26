// WGBThreadPool.cs
// Copyright (c) 2011-2016 Thinksquirrel Inc.
using UnityEngine;

#if !UNITY_WEBGL && !UNITY_WINRT
using System;
using System.Threading;
using ThreadPriority = System.Threading.ThreadPriority;
#endif

namespace Thinksquirrel.WordGameBuilder.Internal.Threading
{       
    [ExecuteInEditMode]
    [AddComponentMenu("")]
    class WGBThreadPool : MonoBehaviour
#if !UNITY_WEBGL && !UNITY_WINRT
        , IThreadPool
#endif
    {
        #if !UNITY_WEBGL && !UNITY_WINRT
        readonly LockFreeQueue m_ActionQueue = new LockFreeQueue();
        readonly LockFreeQueue m_MainThreadQueue = new LockFreeQueue();
        readonly ManualResetEvent m_QueueIsNotEmptyEvent = new ManualResetEvent(false);
        [ThreadStatic] static int s_ThreadWaitTime;
        Thread[] m_WorkerThreads;
        bool m_ShouldStopThreads;
        int m_ThreadCount;
        bool m_Initialized;
        Task m_TaskObject = new Task();
        
        class Task
        {
            readonly ManualResetEvent m_IsFinished = new ManualResetEvent(true);
            volatile int m_TasksCompleted;
            int m_TaskCount;
            ParallelAction m_Action;
            
            internal int IncrementTask()
            {
                #pragma warning disable 420
                return Interlocked.Increment(ref m_TasksCompleted);
                #pragma warning restore 420
            }
            internal void ResetTask(int iterations, int chunkSize, ParallelAction action)
            {
                Wait();
                m_TasksCompleted = 0;
                m_Action = action;
                var taskCount = 0;
                for (var i = 0; i < iterations; i += chunkSize)
                {
                    taskCount++;
                }
                m_TaskCount = taskCount;
                m_IsFinished.Reset();
            }
            internal void SetFinished()
            {
                m_IsFinished.Set();
            }
            internal int GetTaskCount()
            {
                return m_TaskCount;
            }
            internal void Invoke(int index)
            {
                m_Action(index);
            }
            internal bool Wait()
            {
                return m_IsFinished.WaitOne();
            }
        }       
        void Initialize()
        {
            m_ThreadCount = Mathf.Max(1, SystemInfo.processorCount - 1); // TODO: Desired threads

            m_ShouldStopThreads = false;
            m_WorkerThreads = new Thread[m_ThreadCount];

            const ThreadPriority priority = ThreadPriority.Normal; // TODO: Thread priority

            for (var i = 0; i < m_ThreadCount; ++i)
            {
                var thread = new Thread(ExecuteThread) {Priority = priority, IsBackground = true};
                m_WorkerThreads[i] = thread;
                thread.Start();
            }
            m_Initialized = true;
        }
        public void SetPriority()
        {
            const ThreadPriority priority = ThreadPriority.Normal; // TODO: Thread priority

            for (var i = 0; i < m_ThreadCount; ++i)
            {
                var thread = m_WorkerThreads[i];
                if (thread != null && (thread.IsAlive || thread.ThreadState == ThreadState.Unstarted))
                    thread.Priority = priority;
            }
        }
        public int GetThreadCount()
        {
            return m_ThreadCount;
        }
        public void Dispose()
        {
            if (!m_Initialized)
                return;

            m_ShouldStopThreads = true;

            if (m_WorkerThreads != null)
            {
                for (var i = 0; i < m_ThreadCount; ++i)
                {
                    var thread = m_WorkerThreads[i];

                    if (thread.ThreadState == ThreadState.Unstarted) continue;

                    thread.Join();
                }
            }
            m_WorkerThreads = null;           
            m_ShouldStopThreads = false;
            m_Initialized = false;
        }      
        public void For(int iterations, ParallelAction action)
        {
            DoFor(iterations, action, m_TaskObject);
        }
        public void RunInBackground(Action action)
        {
            if (!m_Initialized)
                Initialize();

            m_ActionQueue.Enqueue(action);                        
            m_QueueIsNotEmptyEvent.Set();            
        }
        public void RunOnMainThread(Action action)
        {
            if (!m_Initialized)
                Initialize();

            m_MainThreadQueue.Enqueue(action);
        }
        void DoFor(int iterations, ParallelAction action, Task task)
        {
            if (!m_Initialized)
                Initialize();

            // No multithreaded support
            if (!m_Initialized)
            {
                for (var i = 0; i < iterations; ++i)
                    action(i);

                return;
            }

            if (iterations == 0)
                return;

            var chunkSize = Mathf.Clamp(iterations/m_ThreadCount, 1, iterations);
            task.ResetTask(iterations, chunkSize, action);
            
            var id = 0;

            for (var i = 0; i < iterations; i += chunkSize)
            {
                var startIndex = id*chunkSize;
                var endIndex = Math.Min(startIndex + chunkSize, iterations);

                m_ActionQueue.Enqueue(() => DoTaskAction(task, startIndex, endIndex));
                m_QueueIsNotEmptyEvent.Set();

                ++id;
            }

            task.Wait();
        }
        static void DoTaskAction(Task task, int startIndex, int endIndex)
        {
            for (var index = startIndex; index < endIndex; ++index)
                task.Invoke(index);
            
            if (task.IncrementTask() >= task.GetTaskCount())
                task.SetFinished();
        }
        void ExecuteThread()
        {
            s_ThreadWaitTime = 1;
            while (!m_ShouldStopThreads && m_ActionQueue != null)
            {
                // Wait
                if (!m_QueueIsNotEmptyEvent.WaitOne(s_ThreadWaitTime) && m_ActionQueue.isEmpty)
                {
                    if (s_ThreadWaitTime < 8)
                        s_ThreadWaitTime *= 2;
                    continue;
                }
                
                // Try to dequeue
                var action = m_ActionQueue.Dequeue();

                // We missed, queue is empty
                if (action == null)
                {
                    m_QueueIsNotEmptyEvent.Reset();
                    continue;
                }

                s_ThreadWaitTime = 1;

                try
                {
                    action();
                }
                catch (Exception e)
                {
                    if (!(e is ThreadAbortException))
                    {
                        Debug.LogError(string.Format("Unhandled {0} caught on background thread ({1}) - task aborted.\n{2}", e.GetType().Name, e.Message, e.StackTrace), this);
                    }
                }
            }
        }
        #endif
        
        #region Unity functions
        void OnEnable()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            Parallel.InitializeThreadPool(this, typeof(WGBManualResetEvent));
            Initialize();
            #else
            Parallel.InitializeThreadPool(null, typeof(WGBManualResetEvent));
            #endif
        }
        void OnDestroy()
        {
            #if !UNITY_WEBGL && !UNITY_WINRT
            Parallel.InitializeThreadPool(null, typeof(WGBManualResetEvent));
            Dispose();
            #endif
        }
        #if !UNITY_WEBGL && !UNITY_WINRT
        void Update()
        {
            while (!m_MainThreadQueue.isEmpty)
            {
                m_MainThreadQueue.Dequeue()();
            }
        }
        #endif
        #endregion
    }
}
