﻿using System;
using System.Threading.Tasks;
using CafeLib.BsvSharp.Mapi.Models;
using CafeLib.BsvSharp.Mapi.Responses;
using CafeLib.BsvSharp.Network;
using CafeLib.Core.Extensions;
using CafeLib.Core.Support;
using CafeLib.Web.Request;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CafeLib.BsvSharp.Mapi
{
    public abstract class MerchantClient : BasicApiRequest, IMerchantClient
    {
        public string Network { get; }
        public string Url { get; }
        public string Name { get; }
        public string ApiKey { get; private set; }

        #region Constructors

        protected MerchantClient(string clientName, string merchantUrl, NetworkType networkType = NetworkType.Main)
            : this(clientName, merchantUrl, null, networkType)
        {
        }

        protected MerchantClient(string clientName, string merchantUrl, string apiEnv, NetworkType networkType = NetworkType.Main)
        {
            Name = clientName;
            Url = merchantUrl;
            Network = networkType.GetDescriptor();

            Headers.Add("Content-Type", WebContentType.Json);
            Headers.Add("User-Agent", GetType().Namespace);

            AddAuthorizationKey(Environment.GetEnvironmentVariable(apiEnv ?? ""));
        }

        #endregion

        #region Methods

        protected void AddAuthorizationKey(string apiKey)
        {
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                ApiKey = apiKey;
                Headers.Add("Authorization", $"Bearer {ApiKey}");
            }
        }

        #endregion

        #region Mapi

        /// <summary>
        /// Get fee quote.
        /// </summary>
        /// <returns>request response</returns>
        public virtual async Task<ApiResponse<FeeQuoteResponse>> GetFeeQuote()
        {
            var url = $"{Url}/mapi/feeQuote";
            return await GetRequest<ApiResponse<FeeQuoteResponse>, FeeQuoteResponse, FeeQuote>(url);
        }

        /// <summary>
        /// Get transaction status
        /// </summary>
        /// <param name="txHash">transaction hash</param>
        /// <returns>request response</returns>
        public virtual async Task<ApiResponse<TransactionStatusResponse>> QueryTransactionStatus(string txHash)
        {
            var url = $"{Url}/mapi/tx/{txHash}";
            return await GetRequest<ApiResponse<TransactionStatusResponse>, TransactionStatusResponse, TransactionStatus>(url);
        }

        /// <summary>
        /// Submit transaction
        /// </summary>
        /// <param name="txRaw">raw transaction as hex string</param>
        /// <returns>request response</returns>
        public virtual async Task<ApiResponse<TransactionSubmitResponse>> SubmitTransaction(string txRaw)
        {
            var url = $"{Url}/mapi/tx";
            var jsonBody = JToken.FromObject(new { rawtx = txRaw });
            return await PostRequest<ApiResponse<TransactionSubmitResponse>, TransactionSubmitResponse, TransactionSubmit>(url, jsonBody);
        }

        #endregion

        #region Helpers

        private async Task<TApiResponse> GetRequest<TApiResponse, TMerchantResponse, TCargo>(string url)
            where TApiResponse : ApiResponse<TMerchantResponse> where TMerchantResponse : MerchantResponse<TCargo> where TCargo : Cargo
        {
            try
            {
                var json = await GetAsync(url);
                var response = JsonConvert.DeserializeObject<TMerchantResponse>(json);
                if (response == null) throw new MerchantClientException<TMerchantResponse>("null response");

                response.ProviderName = Name;
                response.Payload = JsonConvert.DeserializeObject<TCargo>(response.JsonPayload);
                if (response.Payload == null) throw new MerchantClientException<TMerchantResponse>(response, "missing payload");
                response.ProviderId = response.Payload.MinerId;
                return Creator.CreateInstance<TApiResponse>(response);
            }
            catch (Exception ex)
            {
                return Creator.CreateInstance<TApiResponse>(ex);
            }
        }

        private async Task<TApiResponse> PostRequest<TApiResponse, TMerchantResponse, TCargo>(string url, JToken body)
            where TApiResponse : ApiResponse<TMerchantResponse> where TMerchantResponse : MerchantResponse<TCargo> where TCargo : Cargo
        {
            try
            {
                var json = await PostAsync(url, body);
                var response = JsonConvert.DeserializeObject<TMerchantResponse>(json);
                if (response == null) throw new MerchantClientException<TMerchantResponse>("null response");

                response.ProviderName = Name;
                response.Payload = JsonConvert.DeserializeObject<TCargo>(response.JsonPayload);
                if (response.Payload == null) throw new MerchantClientException<TMerchantResponse>(response, "missing payload");
                response.ProviderId = response.Payload.MinerId;
                return Creator.CreateInstance<TApiResponse>(response);
            }
            catch (Exception ex)
            {
                return Creator.CreateInstance<TApiResponse>(ex);
            }
        }

        #endregion
    }
}
