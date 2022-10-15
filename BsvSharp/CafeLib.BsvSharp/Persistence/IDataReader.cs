#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

using CafeLib.Core.Buffers;
using CafeLib.Core.Numerics;

namespace CafeLib.BsvSharp.Persistence
{
    public interface IDataReader
    {
        IDataReader Read(byte[] data);
        IDataReader Read(out byte data);
        IDataReader Read(out int data);
        IDataReader Read(out uint data);
        IDataReader Read(out long data);
        IDataReader Read(out ulong data);
        IDataReader Read(out UInt160 data);
        IDataReader Read(out UInt256 data);
        IDataWriter Read(out UInt512 data);
    }
}
