using System.ComponentModel;

namespace CafeLib.BsvSharp.Network
{
    public enum NetworkVersion : byte
    {
        [Description("main")]
        Main = 0x80,

        [Description("test")]
        Test = 0xef,
    }
}