using System.ComponentModel;

namespace CafeLib.BsvSharp.Network
{
    public enum NetworkType
    {
        [Description("main")]
        Main,

        [Description("test")]
        Test,

        [Description("regtest")]
        Regression,

        [Description("stn")]
        Scaling
    }
}