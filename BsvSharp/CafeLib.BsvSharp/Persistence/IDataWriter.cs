using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Persistence
{
    public interface IDataWriter
    {
        IDataWriter Write(byte[] data);
        IDataWriter Write(byte data);
        IDataWriter Write(int data);
        IDataWriter Write(uint data);
        IDataWriter Write(long data);
        IDataWriter Write(ulong data);
        IDataWriter Write(string data);
        IDataWriter Write(UInt160 data);
        IDataWriter Write(UInt256 data);
        IDataWriter Write(UInt512 data);
    }
}
