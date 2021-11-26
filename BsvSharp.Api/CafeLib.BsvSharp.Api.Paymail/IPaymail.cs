using System.Threading.Tasks;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Units;

namespace CafeLib.BsvSharp.Api.Paymail
{
    public interface IPaymail
    {
        /// <summary>
        /// Determine whether domain has capability.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        Task<bool> DomainHasCapability(string domain, Capability capability);

        /// <summary>
        /// Ensure capability
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        Task EnsureCapability(string domain, Capability capability);

        /// <summary>
        /// Get public key.
        /// </summary>
        /// <param name="paymailAddress"></param>
        /// <returns></returns>
        Task<PublicKey> GetPublicKey(string paymailAddress);

        /// <summary>
        /// Verify public key.
        /// </summary>
        /// <param name="paymailAddress"></param>
        /// <param name="pubKey"></param>
        /// <returns></returns>
        Task<bool> VerifyPubKey(string paymailAddress, PublicKey pubKey);

        /// <summary>
        /// Implements brfc 759684b1a19a, paymentDestination: bsvalias Payment Addressing (Basic Address Resolution)
        /// </summary>
        /// <param name="key">Private key with which to sign this request. If null, signature will be blank. Else, must match public key returned by GetPubKey(senderHandle).</param>
        /// <param name="receiverAddress"></param>
        /// <param name="senderAddress"></param>
        /// <param name="senderName"></param>
        /// <param name="amount"></param>
        /// <param name="purpose"></param>
        /// <returns></returns>
        Task<Script> GetOutputScript(PrivateKey key, string receiverAddress, string senderAddress, string senderName = null, Amount? amount = null, string purpose = "");

        /// <summary>
        /// Verifies that the message was signed by the private key corresponding to the paymail public key.
        /// </summary>
        /// <param name="paymail">The paymail claiming to have signed the message.</param>
        /// <param name="message">A copy of the message which was originally signed.</param>
        /// <param name="signature">The signature received for validation.</param>
        /// <returns>true if both the public key and signature were confirmed as valid.</returns>
        Task<bool> IsValidSignature(string paymail, string message, string signature);
    }
}
