using System;

namespace GlogGenerator.IgdbApi
{
    public class IgdbApiClientToken
    {
        public string AccessToken { get; set; }

        public DateTimeOffset ExpiresAt { get; set; } = DateTimeOffset.MaxValue;

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(this.AccessToken) && (DateTimeOffset.UtcNow < this.ExpiresAt);
        }

        public static IgdbApiClientToken FromTwitchOAuthToken(DateTimeOffset requestedAt, TwitchOAuthToken twitchToken)
        {
            if (!twitchToken.TokenType.Equals("bearer", StringComparison.Ordinal))
            {
                throw new ArgumentException($"Unexpected token type `{twitchToken.TokenType}` (expected `bearer`)");
            }

            var apiClientToken = new IgdbApiClientToken();

            apiClientToken.AccessToken = twitchToken.AccessToken;
            apiClientToken.ExpiresAt = requestedAt + TimeSpan.FromSeconds(twitchToken.ExpiresInSeconds);

            return apiClientToken;
        }
    }
}
