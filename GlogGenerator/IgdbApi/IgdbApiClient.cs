﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace GlogGenerator.IgdbApi
{
    public class IgdbApiClient : IDisposable
    {
        public const string BaseUrl = "https://api.igdb.com/v4/";

        private const int RequestItemsMax = 500;

        // "There is a rate limit of 4 requests per second."
        // https://api-docs.igdb.com/#rate-limits
        private static readonly TimeSpan RequestDelayTimeMin = TimeSpan.FromSeconds(0.25);

        private readonly ILogger logger;
        private readonly string clientId;
        private readonly string clientSecret;

        private HttpClient httpClient;
        private bool disposed;

        private IgdbApiClientToken token;

        private DateTimeOffset lastRequestTime = DateTimeOffset.MinValue;

        public IgdbApiClient(
            ILogger logger,
            string clientId,
            string clientSecret)
        {
            this.logger = logger;
            this.clientId = clientId;
            this.clientSecret = clientSecret;

            this.httpClient = new HttpClient();
            this.disposed = false;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!this.disposed)
                {
                    this.httpClient.Dispose();

                    this.disposed = true;
                }
            }
        }

        private async Task<string> GetBearerTokenAsync()
        {
            if ((this.token == null) || !this.token.IsValid())
            {
                // https://api-docs.igdb.com/#authentication
                var oauthRequestPath = "https://id.twitch.tv/oauth2/token";
                var oauthRequestParams = new Dictionary<string, string>()
                {
                    { "client_id", this.clientId },
                    { "client_secret", this.clientSecret },
                    { "grant_type", "client_credentials" },
                };

                var oauthRequestUri = QueryHelpers.AddQueryString(oauthRequestPath, oauthRequestParams);
                using (var request = new HttpRequestMessage(HttpMethod.Post, oauthRequestUri))
                {
                    var requestTime = DateTimeOffset.UtcNow;

                    var response = await this.httpClient.SendAsync(request);

                    response.EnsureSuccessStatusCode();

                    var responseText = await response.Content.ReadAsStringAsync();
                    var responseObject = JsonConvert.DeserializeObject<TwitchOAuthToken>(responseText);

                    this.token = IgdbApiClientToken.FromTwitchOAuthToken(requestTime, responseObject);
                }
            }

            return this.token.AccessToken;
        }

        private static List<string> GetFieldNames<T>()
            where T : class
        {
            var fields = new List<string>();

            var typeProperties = typeof(T).GetProperties();
            foreach (var typeProperty in typeProperties)
            {
                var jsonPropertyAttr = typeProperty.GetCustomAttribute<JsonPropertyAttribute>();
                if (jsonPropertyAttr == null)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(jsonPropertyAttr.PropertyName))
                {
                    throw new ArgumentException($"{typeof(T).FullName}.{typeProperty.Name} has an empty JsonProperty name");
                }

                // Skip properties which are Glog overrides.
                var glogOverrideAttr = typeProperty.GetCustomAttribute<IgdbEntityGlogOverrideValueAttribute>();
                if (glogOverrideAttr != null)
                {
                    continue;
                }

                fields.Add(jsonPropertyAttr.PropertyName);
            }

            return fields;
        }

        private async Task<List<T>> GetItemsAsync<T>(string itemsEndpointPath, List<int> itemIds)
            where T : IgdbEntity
        {
            var requestUri = BaseUrl + itemsEndpointPath;
            var requestFields = GetFieldNames<T>();

            var items = new List<T>(itemIds.Count);
            var itemIdsStart = 0;
            while (itemIdsStart < itemIds.Count)
            {
                var requestIdsCount = Math.Min(itemIds.Count - itemIdsStart, RequestItemsMax);
                var requestIds = itemIds.GetRange(itemIdsStart, requestIdsCount);

                using (var request = new HttpRequestMessage(HttpMethod.Post, requestUri))
                {
                    var bearerToken = await this.GetBearerTokenAsync();

                    var queryBuilder = new StringBuilder();
                    queryBuilder.Append("fields ");
                    queryBuilder.Append(string.Join(',', requestFields.ToArray()));
                    queryBuilder.Append(";where id = (");
                    queryBuilder.Append(string.Join(',', requestIds.ToArray()));
                    queryBuilder.Append(");limit ");
                    queryBuilder.Append(requestIdsCount.ToString(CultureInfo.InvariantCulture));
                    queryBuilder.Append(';');

                    request.Headers.Add("Accept", "application/json");
                    request.Headers.Add("Authorization", $"Bearer {bearerToken}");
                    request.Headers.Add("Client-ID", this.clientId);
                    request.Content = new StringContent(queryBuilder.ToString());

                    var timeSinceLastRequest = DateTimeOffset.UtcNow - this.lastRequestTime;
                    if (timeSinceLastRequest < RequestDelayTimeMin)
                    {
                        await Task.Delay(RequestDelayTimeMin - timeSinceLastRequest);
                    }

                    HttpResponseMessage response;
                    try
                    {
                        response = await this.httpClient.SendAsync(request);
                    }
                    finally
                    {
                        this.lastRequestTime = DateTimeOffset.UtcNow;
                    }

                    var responseText = await response.Content.ReadAsStringAsync();

                    try
                    {
                        response.EnsureSuccessStatusCode();
                    }
                    catch (Exception ex) when (!string.IsNullOrEmpty(responseText))
                    {
                        throw new AggregateException(responseText, ex);
                    }

                    var responseObject = JsonConvert.DeserializeObject<List<T>>(responseText);

                    items.AddRange(responseObject);
                }

                itemIdsStart += requestIdsCount;
            }

            var itemIdsFound = items.Select(e => e.GetEntityId()).ToList();
            var itemIdsNotFound = itemIds.Where(i => !itemIdsFound.Contains(i)).ToList();
            foreach (var itemIdNotFound in itemIdsNotFound)
            {
                this.logger.LogWarning("{EntityType} id {ItemIdNotFound} wasn't found",
                    typeof(T).Name,
                    itemIdNotFound);
            }

            return items;
        }

        public async Task<List<IgdbCollection>> GetCollectionsAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbCollection>("collections", ids);
        }

        public async Task<List<IgdbCompany>> GetCompaniesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbCompany>("companies", ids);
        }

        public async Task<List<IgdbFranchise>> GetFranchisesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbFranchise>("franchises", ids);
        }

        public async Task<List<IgdbGame>> GetGamesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbGame>("games", ids);
        }

        public async Task<List<IgdbGameMode>> GetGameModesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbGameMode>("game_modes", ids);
        }

        public async Task<List<IgdbGenre>> GetGenresAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbGenre>("genres", ids);
        }

        public async Task<List<IgdbInvolvedCompany>> GetInvolvedCompaniesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbInvolvedCompany>("involved_companies", ids);
        }

        public async Task<List<IgdbKeyword>> GetKeywordsAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbKeyword>("keywords", ids);
        }

        public async Task<List<IgdbPlatform>> GetPlatformsAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbPlatform>("platforms", ids);
        }

        public async Task<List<IgdbPlayerPerspective>> GetPlayerPerspectivesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbPlayerPerspective>("player_perspectives", ids);
        }

        public async Task<List<IgdbReleaseDate>> GetReleaseDatesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbReleaseDate>("release_dates", ids);
        }

        public async Task<List<IgdbTheme>> GetThemesAsync(List<int> ids)
        {
            return await this.GetItemsAsync<IgdbTheme>("themes", ids);
        }
    }
}
