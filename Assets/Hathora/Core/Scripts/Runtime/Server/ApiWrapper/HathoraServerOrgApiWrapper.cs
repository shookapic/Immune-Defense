using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using Debug = UnityEngine.Debug;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    /// <summary>
    /// Operations that allow you manage your org tokens.
    /// Apps Concept | https://hathora.dev/docs/concepts/hathora-entities#application
    /// API Docs | https://hathora.dev/api#tag/AppV1
    /// </summary>
    public class HathoraServerOrgApiWrapper
    {

        // protected IOrganizationsV1 OrgApi { get; }
        // protected ITokensV1 TokenApi { get; }

        public HathoraServerOrgApiWrapper()
        {
            Debug.Log($"[{nameof(HathoraServerOrgApiWrapper)}.Constructor] " +
                "Initializing Server API...");
            
            // this.OrgApi = _hathoraSdk.OrganizationsV1;
            // this.TokenApi = _hathoraSdk.TokensV1;
        }
        
        
        #region Server App Async Hathora SDK Calls
        /// <summary>
        /// Wrapper for `CreateAppAsync` to upload and app a cloud app to Hathora.
        /// </summary>
        /// <param name="_cancelToken">TODO: This may be implemented in the future</param>
        /// <returns>Returns App on success</returns>
        public async Task<string> CreateOrgTokenAsync(
            string bearerToken,
            CancellationToken _cancelToken = default)
        {
            var sdk = new HathoraCloudSDK(
                security: new Security()
                {
                    HathoraDevToken = bearerToken,
                });
            IOrganizationsV1 OrgApi = sdk.OrganizationsV1;
            ITokensV1 TokenApi = sdk.TokensV1;
            
            string logPrefix = $"[{nameof(HathoraServerAppApiWrapper)}.{nameof(CreateOrgTokenAsync)}]";
            Debug.Log($"{logPrefix} making GetAppsAsync request");

            // Get response async => 
            GetOrgsResponse getOrgsResponse = null;
            
            try
            {
                getOrgsResponse = await OrgApi.GetOrgsAsync(); 
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} {nameof(OrgApi.GetOrgsAsync)} => Error: {e.Message}");
                return null; // fail
            }

            // Get inner response to return -> Log/Validate
            // TODO: this assumes one org per user
            List<Scope> userScopes =
                getOrgsResponse.OrgsPage != null && getOrgsResponse.OrgsPage.Orgs[0] != null ?
                    getOrgsResponse.OrgsPage.Orgs[0].Scopes : new List<Scope>();
            Debug.Log($"{logPrefix} num: '{userScopes?.Count ?? 0}'");
            
            getOrgsResponse.RawResponse?.Dispose(); // Prevent mem leaks
            
            // Create org tokenw ith same user scopes
            // Get response async => 
            CreateOrgTokenResponse createOrgTokenResponse = null;
            
            string currentDateTime = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
            CreateOrgTokenRequest createTokenRequest = new CreateOrgTokenRequest() {
                CreateOrgToken = new CreateOrgToken() {
                    Name = "unity-plugin-token_" + currentDateTime,
                    Scopes = Scopes.CreateArrayOfScope(userScopes)
                },
                // TODO: this assumes one org per user
                OrgId = getOrgsResponse.OrgsPage.Orgs[0].OrgId,
            };
            
            try
            {
                createOrgTokenResponse = await TokenApi.CreateOrgTokenAsync(createTokenRequest); 
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} {nameof(TokenApi.CreateOrgTokenAsync)} => Error: {e.Message}");
                return null; // fail
            }
            
            string createdOrgToken =
                createOrgTokenResponse.CreatedOrgToken != null && createOrgTokenResponse.CreatedOrgToken.PlainTextToken.Length > 0 ?
                    createOrgTokenResponse.CreatedOrgToken.PlainTextToken : "";
            Debug.Log($"{logPrefix} num: 'orgToken created, named: " + "unity-plugin-token_" + currentDateTime);
            
            createOrgTokenResponse.RawResponse?.Dispose(); // Prevent mem leaks
            
            return createdOrgToken;
        }
        #endregion // Server Org Async Hathora SDK Calls
    }
}
