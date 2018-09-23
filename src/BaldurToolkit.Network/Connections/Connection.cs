using System;
using System.Net;
using BaldurToolkit.Network.Controllers;
using BaldurToolkit.Network.Protocol;

namespace BaldurToolkit.Network.Connections
{
    public abstract class Connection : IConnection
    {
        private bool isClosed;

        protected Connection(IPacketRouter router, IProtocol protocol)
        {
            this.Router = router;
            this.Protocol = protocol;
        }

        public event EventHandler<ConnectionClosedEventArgs> Closed;

        public bool IsOpen => !this.isClosed;

        public EndPoint RemoteEndPoint { get; protected set; }

        public IPacketRouter Router { get; }

        public IProtocol Protocol { get; }

        protected object SyncRoot { get; } = new object();

        public abstract void Send(IPacket packet);

        public virtual void Close()
        {
            if (!this.TryClose())
            {
                return;
            }

            this.InvokeClosedEvent(null);
        }

        public override string ToString()
        {
            if (this.RemoteEndPoint != null)
            {
                return $"{this.GetType().Name}({this.RemoteEndPoint})";
            }

            return base.ToString();
        }

        protected bool TryClose()
        {
            lock (this.SyncRoot)
            {
                if (this.isClosed)
                {
                    return false;
                }

                this.isClosed = true;
            }

            return true;
        }

        protected void InvokeClosedEvent(Exception exception)
        {
            this.Closed?.Invoke(this, exception == null ? ConnectionClosedEventArgs.Empty : new ConnectionClosedEventArgs(exception));
        }
    }
}
