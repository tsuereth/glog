using System;
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

        private static readonly Dictionary<Type, string> GetItemsEndpointPaths = new Dictionary<Type, string>()
        {
            { typeof(IgdbCollection), "collections" },
            { typeof(IgdbCompany), "companies" },
            { typeof(IgdbFranchise), "franchises" },
            { typeof(IgdbGame), "games" },
            { typeof(IgdbGameMode), "game_modes" },
            { typeof(IgdbGameStatus), "game_statuses" },
            { typeof(IgdbGameType), "game_types" },
            { typeof(IgdbGenre), "genres" },
            { typeof(IgdbInvolvedCompany), "involved_companies" },
            { typeof(IgdbKeyword), "keywords" },
            { typeof(IgdbPlatform), "platforms" },
            { typeof(IgdbPlayerPerspective), "player_perspectives" },
            { typeof(IgdbTheme), "themes" },
        };

        private readonly ILogger logger;
        private readonly string clientId;
        private readonly string clientSecret;

        private static DateTimeOffset lastRequestTime = DateTimeOffset.MinValue;

        private HttpClient httpClient;
        private bool disposed;

        private IgdbApiClientToken token;

        public IgdbApiClient(
            ILogger logger,
            string clientId,
            string clientSecret)
        {
            if (string.IsNullOrEmpty(clientId))
            {
                throw new ArgumentException("Missing or empty IGDB Client ID");
            }
            if (string.IsNullOrEmpty(clientSecret))
            {
                throw new ArgumentException("Missing or empty IGDB Client Secret");
            }

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

                    var timeSinceLastRequest = DateTimeOffset.UtcNow - lastRequestTime;
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
                        lastRequestTime = DateTimeOffset.UtcNow;
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

        public async Task<List<T>> GetEntitiesAsync<T>(List<int> ids)
            where T : IgdbEntity
        {
            if (!GetItemsEndpointPaths.TryGetValue(typeof(T), out var itemsEndpointPath))
            {
                throw new NotImplementedException();
            }

            return await this.GetItemsAsync<T>(itemsEndpointPath, ids);
        }

        public async Task<IgdbApiBatchDataResponse> GetAllDataForGameIdsAsync(List<int> gameIds)
        {
            var allData = new IgdbApiBatchDataResponse();

            // Fetch the games first, to get current IDs for metadata references.
            var requestedGames = await this.GetEntitiesAsync<IgdbGame>(gameIds);
            allData.Games = requestedGames;

            // Extract associated game IDs (remasters, expansions, etc) and fetch those games as well.
            var associatedGameIds = requestedGames.SelectMany(g => g.GetAllAssociatedGameIds()).Distinct();
            var newAssociatedGameIds = associatedGameIds.Except(gameIds).ToList();
            var associatedGames = await this.GetEntitiesAsync<IgdbGame>(newAssociatedGameIds);
            allData.Games.AddRange(associatedGames);

            // Update involvedCompanies to get most-current company IDs.
            var involvedCompanyIds = allData.Games.SelectMany(g => g.InvolvedCompanyIds).Distinct().ToList();
            allData.InvolvedCompanies = await this.GetEntitiesAsync<IgdbInvolvedCompany>(involvedCompanyIds);

            var companyIds = allData.InvolvedCompanies.Select(i => i.CompanyId).Distinct().ToList();
            allData.Companies = await this.GetEntitiesAsync<IgdbCompany>(companyIds);

            // Now, update the rest of the ID-driven metadata.

            var collectionIds = allData.Games.SelectMany(g => g.CollectionIds).Distinct().ToList();
            allData.Collections = await this.GetEntitiesAsync<IgdbCollection>(collectionIds.Distinct().ToList());

            var franchiseIds = allData.Games.Where(g => g.MainFranchiseId != IgdbFranchise.IdNotFound).Select(g => g.MainFranchiseId).Distinct().ToList();
            franchiseIds.AddRange(allData.Games.SelectMany(g => g.FranchiseIds).Distinct());
            allData.Franchises = await this.GetEntitiesAsync<IgdbFranchise>(franchiseIds.Distinct().ToList());

            var gameModeIds = allData.Games.SelectMany(g => g.GameModeIds).Distinct().ToList();
            allData.GameModes = await this.GetEntitiesAsync<IgdbGameMode>(gameModeIds);

            var gameStatusIds = allData.Games.Where(g => g.GameStatusId != IgdbGameStatus.IdNotFound).Select(g => g.GameStatusId).Distinct().ToList();
            allData.GameStatuses = await this.GetEntitiesAsync<IgdbGameStatus>(gameStatusIds);

            var gameTypeIds = allData.Games.Where(g => g.GameTypeId != IgdbGameType.IdNotFound).Select(g => g.GameTypeId).Distinct().ToList();
            allData.GameTypes = await this.GetEntitiesAsync<IgdbGameType>(gameTypeIds);

            var genreIds = allData.Games.SelectMany(g => g.GenreIds).Distinct().ToList();
            allData.Genres = await this.GetEntitiesAsync<IgdbGenre>(genreIds);

            // NOTE: As of writing, "keywords" are way too abundant and vague to be useful; ignore 'em.
#if false
            var keywordIds = allData.Games.SelectMany(g => g.KeywordIds).Distinct().ToList();
            allData.Keywords = await this.GetEntitiesAsync<IgdbKeyword>(keywordIds);
#else
            allData.Keywords = new List<IgdbKeyword>();
#endif

            var platformIds = allData.Games.SelectMany(g => g.PlatformIds).Distinct().ToList();
            allData.Platforms = await this.GetEntitiesAsync<IgdbPlatform>(platformIds);

            var playerPerspectiveIds = allData.Games.SelectMany(g => g.PlayerPerspectiveIds).Distinct().ToList();
            allData.PlayerPerspectives = await this.GetEntitiesAsync<IgdbPlayerPerspective>(playerPerspectiveIds);

            var themeIds = allData.Games.SelectMany(g => g.ThemeIds).Distinct().ToList();
            allData.Themes = await this.GetEntitiesAsync<IgdbTheme>(themeIds);

            return allData;
        }
    }
}
