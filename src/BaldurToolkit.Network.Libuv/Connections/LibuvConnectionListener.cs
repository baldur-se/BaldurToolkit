using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaldurToolkit.Network.Connections;
using NLibuv;

namespace BaldurToolkit.Network.Libuv.Connections
{
    public abstract class LibuvConnectionListener<TConnection> : ConnectionListener<TConnection>, IDisposable
        where TConnection : LibuvConnection
    {
        private readonly Func<UvTcp, LibuvWorker, TConnection> connectionFactory;

        private int maxConnections = -1;

        protected LibuvConnectionListener(LibuvWorker worker, Func<UvTcp, LibuvWorker, TConnection> connectionFactory)
            : this(worker)
        {
            this.connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        protected LibuvConnectionListener(LibuvWorker worker)
        {
            this.Worker = worker;
        }

        ~LibuvConnectionListener()
        {
            this.Dispose(false);
        }

        public LibuvWorker Worker { get; }

        public override bool IsListening
        {
            get
            {
                lock (this.SyncRoot)
                {
                    return this.State != null;
                }
            }
        }

        /// <summary>
        /// Gets or sets maximum number of working connections.
        /// Set to -1 to disable limit.
        /// Default value is -1.
        /// </summary>
        public int MaxConnections
        {
            get => this.maxConnections;
            set
            {
                if (value < -1 || value == 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                this.maxConnections = value;
            }
        }

        protected object SyncRoot { get; } = new object();

        protected ListenState State { get; set; }

        public override Task StartListening()
        {
            var tcs = new TaskCompletionSource<object>();
            lock (this.SyncRoot)
            {
                if (this.State != null)
                {
                    throw new Exception("Already listening a socket. Please stop listening first.");
                }

                this.State = new ListenState();
                this.State.StartTcs = tcs;

                this.OnListeningStarted(this.State);
            }

            return tcs.Task;
        }

        public override Task StopListening()
        {
            var tcs = new TaskCompletionSource<object>();
            lock (this.SyncRoot)
            {
                this.CloseBaseSocket(tcs);
            }

            return tcs.Task;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (this.SyncRoot)
            {
                this.CloseBaseSocket();
            }
        }

        protected abstract void OnListeningStarted(ListenState state);

        protected virtual void OnNewConnectionAccepted(UvTcp socket)
        {
            if (this.MaxConnections != -1 && this.ConnectionCount >= this.MaxConnections)
            {
                socket.Close();
                return;
            }

            var connection = this.CreateConnection(socket);

            this.OnConnectionCreated(connection);

            connection.BeginReceive();
        }

        protected virtual TConnection CreateConnection(UvTcp socket)
        {
            return this.connectionFactory(socket, this.Worker);
        }

        protected virtual void CloseBaseSocket(TaskCompletionSource<object> tcs = null)
        {
            var listenState = this.State;
            this.State = null;

            if (listenState != null)
            {
                if (tcs != null)
                {
                    listenState.StopTcs.Add(tcs);
                }

                // Execute on the loop thread
                this.Worker.EnqueueFinalJob(CloseBaseSocketCallback, listenState);
            }
            else
            {
                tcs?.TrySetResult(null);
            }
        }

        private static void CloseBaseSocketCallback(object stateObject)
        {
            var listenState = stateObject as ListenState;
            listenState?.Close();
        }

        protected class ListenState
        {
            public List<UvHandle> OpenedHandles { get; } = new List<UvHandle>();

            public TaskCompletionSource<object> StartTcs { get; set; }

            public List<TaskCompletionSource<object>> StopTcs { get; set; } = new List<TaskCompletionSource<object>>();

            public virtual void Close()
            {
                try
                {
                    foreach (var handle in this.OpenedHandles)
                    {
                        handle.Close();
                    }

                    foreach (var tcs in this.StopTcs)
                    {
                        tcs.TrySetResult(null);
                    }
                }
                catch (Exception exception)
                {
                    foreach (var tcs in this.StopTcs)
                    {
                        tcs.TrySetResult(exception);
                    }
                }
            }
        }
    }
}
