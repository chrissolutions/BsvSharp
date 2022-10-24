using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Api.Paymail.Models;
using CafeLib.BsvSharp.Extensions;
using CafeLib.BsvSharp.Keys;
using CafeLib.BsvSharp.Scripting;
using CafeLib.BsvSharp.Units;
using CafeLib.Core.Support;
using CafeLib.Web.Request;
using DnsClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EnumExtensions = CafeLib.Core.Extensions.EnumExtensions;

namespace CafeLib.BsvSharp.Api.Paymail
{
    public class PaymailClient : BasicApiRequest, IPaymail
    {
        private const string HandleRegexPattern = @"^[a-zA-Z0-9.!#$%&'*+\/=?^_`{|}~-]+@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$";
        private readonly IDictionary<string, CapabilitiesResponse> _cache;
        private static readonly Lazy<Regex> HandleRegex = new(() => new Regex(HandleRegexPattern), true);

        /// <summary>
        /// Paymail api default constructor.
        /// </summary>
        public PaymailClient()
        {
             _cache = new ConcurrentDictionary<string, CapabilitiesResponse>();
             Headers.Add("User-Agent", "KzPaymailClient");
             Headers.Add("Accept", WebContentType.Json);
        }

        /// <summary>
        /// Cache a domain.
        /// </summary>
        /// <param name="domain">domain</param>
        /// <returns>true if domain is cached</returns>
        public async Task<bool> CacheDomain(string domain)
        {
            await GetApiDescriptionFor(domain);
            return _cache.ContainsKey(domain);
        }

        /// <summary>
        /// Determine whether domain has capability.
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public async Task<bool> DomainHasCapability(string domain, Capability capability)
        {
            var id = ToBrfcId(capability);
            var ba = await GetApiDescriptionFor(domain);
            if (ba == null || !ba.Capabilities.ContainsKey(id))
                return false;
            var v = ba.Capabilities[id].Value;
            return v != null && !v.Equals(false);
        }

        /// <summary>
        /// Ensure capability
        /// </summary>
        /// <param name="domain"></param>
        /// <param name="capability"></param>
        /// <returns></returns>
        public async Task EnsureCapability(string domain, Capability capability)
        {
            if (!await DomainHasCapability(domain, capability))
                throw new InvalidOperationException($"Unknown capability \"{capability}\" for \"{domain}\"");
        }

        /// <summary>
        /// Get public key.
        /// </summary>
        /// <param name="paymailAddress"></param>
        /// <returns></returns>
        public async Task<GetPublicKeyResponse> GetPublicKey(string paymailAddress)
        {
            try
            {
                var url = await GetIdentityUrl(paymailAddress);
                var json = await GetAsync(url);
                var response = JsonConvert.DeserializeObject<GetPublicKeyResponse>(json);
                var pubkey = response != null ? new PublicKey(response.PubKey) : null;
                return new GetPublicKeyResponse(response, () => pubkey != null && pubkey.IsCompressed && new[] { 2, 3 }.ToArray().Contains(pubkey.Data[0]));
            }
            catch (Exception ex)
            {
                return new GetPublicKeyResponse(ex);
            }
        }

        /// <summary>
        /// Verify public key.
        /// </summary>
        /// <param name="receiverHandle"></param>
        /// <param name="pubKey"></param>
        /// <returns></returns>
        public async Task<VerifyPublicKeyResponse> VerifyPubKey(string receiverHandle, PublicKey pubKey)
        {
            try
            {
                var url = await GetVerifyUrl(receiverHandle, pubKey.ToHex());
                var json = await GetAsync(url);
                var response = JsonConvert.DeserializeObject<VerifyPublicKeyResponse>(json);
                return response != null
                    ? new VerifyPublicKeyResponse(response, () => response.PublicKey == pubKey.ToHex() && response.Match)
                    : new VerifyPublicKeyResponse(false);
            }
            catch (Exception ex)
            {
                return new VerifyPublicKeyResponse(ex);
            }
        }

        /// <summary>
        /// Implements brfc 759684b1a19a, paymentDestination: bsvalias Payment Addressing (Basic Address Resolution)
        /// </summary>
        /// <param name="key">Private key with which to sign this request. If null, signature will be blank. Else, must match public key returned by GetPubKey(senderHandle).</param>
        /// <param name="receiverHandle"></param>
        /// <param name="senderHandle"></param>
        /// <param name="senderName"></param>
        /// <param name="amount"></param>
        /// <param name="purpose"></param>
        /// <returns></returns>
        public async Task<Script> GetOutputScript(PrivateKey key, string receiverHandle, string senderHandle, string senderName = null, Amount? amount = null, string purpose = "")
        {
            amount ??= Amount.Zero;
            var dt = DateTime.UtcNow.ToString("o");
            var message = $"{senderHandle}{amount.Value.Satoshis}{dt}{purpose}";
            var signature = key?.SignMessage(message).ToString() ?? "";

            // var ok = key.GetPubKey().VerifyMessage(message, signature);

            var request = new GetOutputScriptRequest
            {
                SenderHandle = senderHandle,
                Amount = amount.Value.Satoshis,
                Timestamp = dt,
                Purpose = purpose ?? "",
                SenderName = senderName ?? "",
                Signature = signature
            };

            var url = await GetAddressUrl(receiverHandle);
            var json = JObject.FromObject(request);

            var response = await PostAsync(url, json);
            // e.g. {"output":"76a914bdfbe8a16162ba467746e382a081a1857831811088ac"} 
            var outputScript = JsonConvert.DeserializeObject<GetOutputScriptResponse>(response);
            return outputScript != null ? Script.FromHex(outputScript.Output) : Script.None;
        }

        /// <summary>
        /// Verifies that the message was signed by the private key corresponding to the paymail public key.
        /// </summary>
        /// <param name="paymail">The paymail claiming to have signed the message.</param>
        /// <param name="message">A copy of the message which was originally signed.</param>
        /// <param name="signature">The signature received for validation.</param>
        /// <returns>true if both the public key and signature were confirmed as valid.</returns>
        public async Task<bool> IsValidSignature(string paymail, string message, string signature)
        {
            var response = await GetPublicKey(paymail);
            return response.IsSuccessful && new PublicKey(response.PubKey).VerifyMessage(message, signature);
        }

        /// <summary>
        /// Verifies that the message was signed by the private key corresponding to the paymail public key.
        /// </summary>
        /// <param name="message">A copy of the message which was originally signed.</param>
        /// <param name="signature">The signature received for validation.</param>
        /// <param name="paymail">The paymail claiming to have signed the message.</param>
        /// <param name="pubkey">If known, the public key corresponding to the private key used by the paymail to sign messages.</param>
        /// <returns>(ok, pubkey) where ok is true only if both the public key and signature were confirmed as valid.
        /// If ok is true, the returned public key is valid and can be saved for future validations.
        /// </returns>
        public async Task<(bool ok, PublicKey pubkey)> IsValidSignature(string message, string signature, string paymail, PublicKey pubkey)
        {
            if (!TryParse(paymail, out _, out var domain)) return (false, pubkey);

            if (pubkey != null)
            {
                // If a pubkey is provided and the domain is capable, verify that it is correct
                // If it is not correct, forget the input value and attempt to obtain the valid key.
                if (await DomainHasCapability(domain, Capability.VerifyPublicKeyOwner))
                {
                    if (!(await VerifyPubKey(paymail, pubkey)).IsSuccessful)
                        pubkey = null;
                }
            }

            // Attempt to determine the correct pubkey for the paymail.
            if (pubkey == null && await DomainHasCapability(domain, Capability.Pki))
            {
                var response = await GetPublicKey(paymail);
                pubkey = response.IsSuccessful ? new PublicKey(response.PubKey) : null;
            }

            return pubkey != null 
                ? (pubkey.VerifyMessage(message, signature), pubkey)
                : (false, null);
        }

        #region Helpers

        private async Task<string> GetIdentityUrl(string paymail) => await GetCapabilityUrl(Capability.Pki, paymail);
        private async Task<string> GetAddressUrl(string paymail) => await GetCapabilityUrl(Capability.PaymentDestination, paymail);
        private async Task<string> GetVerifyUrl(string paymail, string pubkey) => await GetCapabilityUrl(Capability.VerifyPublicKeyOwner, paymail, pubkey);

        /// <summary>
        /// BRFC identifiers are partially defined here: http://bsvalias.org
        /// </summary>
        private static string ToBrfcId(Capability capability)
        {
            return EnumExtensions.GetDescriptor(capability);
        }

        private async Task<CapabilitiesResponse> GetApiDescriptionFor(string domain, bool ignoreCache = false)
        {
            if (!ignoreCache && _cache.TryGetValue(domain, out var capabilities))
                return capabilities;

            var hostname = domain;
            await Retry.Run(async _ =>
            {
                var dns = new LookupClient();
                var response = await dns.QueryAsync($"_bsvalias._tcp.{domain}", QueryType.SRV);
                if (!response.HasError && response.Answers.Count == 1)
                {
                    var srv = response.Answers[0] as DnsClient.Protocol.SrvRecord;
                    hostname = $"{srv?.Target.Value[..^1]}:{srv?.Port}";
                }
            });

            try
            {
                var json = await GetAsync($"https://{hostname}/.well-known/bsvalias");
                capabilities = JsonConvert.DeserializeObject<CapabilitiesResponse>(json);
                _cache[domain] = capabilities;
                return capabilities;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private async Task<string> GetCapabilityUrl(Capability capability, string paymail, string pubkey = null)
        {
            if (!TryParse(paymail, out var alias, out var domain)) return null;

            await EnsureCapability(domain, capability);
            var ba = await GetApiDescriptionFor(domain);
            var url = ba.Capabilities[ToBrfcId(capability)].Value<string>();
            url = url?.Replace("{alias}", alias).Replace("{domain.tld}", domain);
            if (pubkey != null)
                url = url?.Replace("{pubkey}", pubkey);
            return url;
        }

        internal static bool TryParse(string paymail, out string alias, out string domain)
        {
            alias = null;
            domain = null;

            if (!HandleRegex.Value.IsMatch(paymail)) return false;

            var parts = paymail.Split('@');
            alias = parts[0];
            domain = parts[1];
            return true;
        }

        #endregion
    }
}
