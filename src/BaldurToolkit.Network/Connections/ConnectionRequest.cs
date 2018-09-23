using System;

namespace BaldurToolkit.Network.Connections
{
    public class ConnectionRequest<TConnection> : IDisposable
        where TConnection : class, IConnection
    {
        private readonly object syncRoot = new object();
        private bool isCompleted = false;

        ~ConnectionRequest()
        {
            this.Dispose(false);
        }

        /// <summary>
        /// Occurs when a new connection is being created.
        /// </summary>
        public event EventHandler<ConnectionCreatedEventArgs> ConnectionCreated;

        /// <summary>
        /// Occurs when connection creation fails.
        /// </summary>
        public event EventHandler<ConnectionErrorEventArgs> ConnectionError;

        /// <summary>
        /// Complete the request and return the requested connection to the requesting side.
        /// </summary>
        /// <param name="connection">The connection.</param>
        public void Complete(TConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            lock (this.syncRoot)
            {
                if (this.isCompleted)
                {
                    throw new InvalidOperationException("The connection request is already completed.");
                }

                this.isCompleted = true;
            }

            this.ConnectionCreated?.Invoke(this, new ConnectionCreatedEventArgs(connection));
        }

        /// <summary>
        /// Complete the request by sending the error to the requesting side.
        /// </summary>
        /// <param name="connectionError">The error.</param>
        public void Fail(Exception connectionError)
        {
            if (connectionError == null)
            {
                throw new ArgumentNullException(nameof(connectionError));
            }

            lock (this.syncRoot)
            {
                if (this.isCompleted)
                {
                    throw new InvalidOperationException("Can not fail the connection request: the connection request is already completed.", connectionError);
                }

                this.isCompleted = true;
            }

            this.ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(connectionError));
        }

        void IDisposable.Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            lock (this.syncRoot)
            {
                if (this.isCompleted)
                {
                    return;
                }
            }

            this.ConnectionError?.Invoke(this, new ConnectionErrorEventArgs(new Exception("The connection request was not completed correctly.")));
        }
    }
}
