﻿using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace crowlr.core.Services
{
    /// <summary>
    /// A type that tracks background operations and notifies ASP.NET that they are still in progress.
    /// </summary>
    public static class BackgroundTaskService
    {
        /// <summary>
        /// The background task manager for this app domain.
        /// </summary>
        private static readonly RegisteredTasks Instance = new RegisteredTasks();

        /// <summary>
        /// Gets a cancellation token that is set when ASP.NET is shutting down the app domain.
        /// </summary>
        public static CancellationToken Shutdown { get { return Instance.Shutdown; } }

        /// <summary>
        /// Executes an asynchronous background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        public static void Run(Func<Task> operation)
        {
            Instance.Run(operation);
        }

        /// <summary>
        /// Executes a background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        public static void Run(Action operation)
        {
            Instance.Run(operation);
        }
    }

    /// <summary>
    /// A type that tracks background operations and notifies ASP.NET that they are still in progress.
    /// </summary>
    public sealed class RegisteredTasks : IRegisteredObject
    {
        /// <summary>
        /// A cancellation token that is set when ASP.NET is shutting down the app domain.
        /// </summary>
        private readonly CancellationTokenSource _shutdown;

        /// <summary>
        /// A countdown event that is incremented each time a task is registered and decremented each time it completes. When it reaches zero, we are ready to shut down the app domain. 
        /// </summary>
        private readonly AsyncCountdownEvent _count;

        /// <summary>
        /// A task that completes after <see cref="_count"/> reaches zero and the object has been unregistered.
        /// </summary>
        private readonly Task _done;

        /// <summary>
        /// Creates an instance that is registered with the ASP.NET runtime.
        /// </summary>
        public RegisteredTasks()
        {
            // Start the count at 1 and decrement it when ASP.NET notifies us we're shutting down.
            _shutdown = new CancellationTokenSource();
            _count = new AsyncCountdownEvent(1);
            _shutdown.Token.Register(() => _count.Signal(), useSynchronizationContext: false);

            // Register the object.
            HostingEnvironment.RegisterObject(this);

            // When the count reaches zero (all tasks have completed and ASP.NET has notified us we are shutting down),
            //  then unregister this object, and then the _done task is completed.
            _done = _count.WaitAsync().ContinueWith(
                _ => HostingEnvironment.UnregisterObject(this),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Gets a cancellation token that is set when ASP.NET is shutting down the app domain.
        /// </summary>
        public CancellationToken Shutdown { get { return _shutdown.Token; } }

        void IRegisteredObject.Stop(bool immediate)
        {
            _shutdown.Cancel();

            if (immediate)
                _done.Wait();
        }

        /// <summary>
        /// Registers a task with the ASP.NET runtime. The task is unregistered when it completes.
        /// </summary>
        /// <param name="task">The task to register.</param>
        private void Register(Task task)
        {
            _count.AddCount();

            task.ContinueWith(
                _ => _count.Signal(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);
        }

        /// <summary>
        /// Executes an asynchronous background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        public void Run(Func<Task> operation)
        {
            Register(Task.Run(operation));
        }

        /// <summary>
        /// Executes a background operation, registering it with ASP.NET.
        /// </summary>
        /// <param name="operation">The background operation.</param>
        public void Run(Action operation)
        {
            Register(Task.Run(operation));
        }
    }

    /// <summary>
    /// An async-compatible countdown event.
    /// </summary>
    [DebuggerDisplay("CurrentCount = {_count}")]
    [DebuggerTypeProxy(typeof(DebugView))]
    public sealed class AsyncCountdownEvent
    {
        /// <summary>
        /// The TCS used to signal this event.
        /// </summary>
        private readonly TaskCompletionSource<object> _tcs;

        /// <summary>
        /// The remaining count on this event.
        /// </summary>
        private int _count;

        /// <summary>
        /// Creates an async-compatible countdown event.
        /// </summary>
        /// <param name="count">The number of signals this event will need before it becomes set. Must be greater than zero.</param>
        public AsyncCountdownEvent(int count)
        {
            _tcs = new TaskCompletionSource<object>();
            _count = count;
        }

        /// <summary>
        /// Asynchronously waits for this event to be set.
        /// </summary>
        public Task WaitAsync()
        {
            return _tcs.Task;
        }

        /// <summary>
        /// Attempts to modify the current count by the specified amount. This method returns <c>false</c> if the new current count value would be invalid, or if the count has already reached zero.
        /// </summary>
        /// <param name="signalCount">The amount to change the current count. This must be +1 or -1.</param>
        private void ModifyCount(int signalCount)
        {
            if (Interlocked.Add(ref _count, signalCount) == 0)
                _tcs.TrySetResult(null);
        }

        /// <summary>
        /// Attempts to add one to the current count. This method throws <see cref="InvalidOperationException"/> if the count is already at zero or if the new count would be greater than <see cref="Int32.MaxValue"/>.
        /// </summary>
        public void AddCount()
        {
            ModifyCount(1);
        }

        /// <summary>
        /// Attempts to subtract one from the current count. This method throws <see cref="InvalidOperationException"/> if the count is already at zero or if the new count would be less than zero.
        /// </summary>
        public void Signal()
        {
            ModifyCount(-1);
        }
        
        [DebuggerNonUserCode]
        private sealed class DebugView
        {
            private readonly AsyncCountdownEvent _ce;

            public DebugView(AsyncCountdownEvent ce)
            {
                _ce = ce;
            }

            public int CurrentCount { get { return _ce._count; } }

            public Task Task { get { return _ce._tcs.Task; } }
        }
    }
}
