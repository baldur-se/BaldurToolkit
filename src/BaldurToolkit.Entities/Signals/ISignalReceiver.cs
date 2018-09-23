using System;

namespace BaldurToolkit.Entities.Signals
{
    public interface ISignalReceiver<in TSignal>
    {
        /// <summary>
        /// Adds a signal to the signal queue.
        /// </summary>
        /// <param name="signal">The signal.</param>
        void AddSignal(TSignal signal);

        /// <summary>
        /// Adds a delayed signal to the signal queue.
        /// </summary>
        /// <remarks>
        /// The signal should be handled after the specified amount of time
        /// since the receiver receives the signal (not since the sender sent it!).
        /// </remarks>
        /// <param name="signal">The signal.</param>
        /// <param name="delay">The delay of the message handling (since the message has been received)</param>
        void AddSignal(TSignal signal, TimeSpan delay);
    }
}
