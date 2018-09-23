using System;

namespace BaldurToolkit.Network.Controllers
{
    /// <summary>
    /// Default packet handling attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class HandleOpcodePacketAttribute : Attribute
    {
        public HandleOpcodePacketAttribute(int opcode)
        {
            this.Opcode = opcode;
        }

        public int Opcode { get; }
    }
}
