#region Copyright
// Distributed under the Open BSV software license, see the accompanying file LICENSE.
#endregion

namespace CafeLib.BsvSharp.Persistence
{
    public interface IDataSerializer
    {
        /// <summary>
        /// Serialize object to data writer
        /// </summary>
        /// <param name="writer">data writer</param>
        /// <returns>data writer</returns>
        IDataWriter WriteTo(IDataWriter writer);
    }
}
