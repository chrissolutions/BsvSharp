using System.ComponentModel;

namespace CafeLib.BsvSharp.Api.Paymail
{
    public enum Capability
    {
        [Description("pki")]
        Pki,

        [Description("paymentDestination")]
        PaymentDestination,

        [Description("6745385c3fc0")]
        SenderValidation,

        [Description("a9f510c16bde")]
        VerifyPublicKeyOwner,

        [Description("c318d09ed40")]
        ReceiverApprovals,

        [Description("7bd25e5a1fc6")]
        PayToProtocolPrefix
    }
}
