using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;

namespace AdventureWorksMVCCore.Web.Services
{
    /// <summary>
    /// AWS Secrets Manager implementation for retrieving secrets at runtime.
    /// Secrets are cached in memory after first retrieval to minimize API calls.
    /// </summary>
    public class AwsSecretsManager : ISecretsManager
    {
        private readonly IAmazonSecretsManager _client;
        private readonly ConcurrentDictionary<string, string> _cache;

        public AwsSecretsManager(IAmazonSecretsManager client)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _cache = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            if (string.IsNullOrEmpty(secretName))
                throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

            // Check cache first
            if (_cache.TryGetValue(secretName, out var cachedValue))
                return cachedValue;

            try
            {
                var request = new GetSecretValueRequest
                {
                    SecretId = secretName
                };

                var response = await _client.GetSecretValueAsync(request);
                var secretValue = response.SecretString;

                // Cache the secret
                _cache.TryAdd(secretName, secretValue);

                return secretValue;
            }
            catch (ResourceNotFoundException)
            {
                throw new InvalidOperationException($"Secret '{secretName}' not found in AWS Secrets Manager");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from AWS Secrets Manager", ex);
            }
        }

        public string GetSecret(string secretName)
        {
            // For synchronous access, use cached value or throw
            if (_cache.TryGetValue(secretName, out var cachedValue))
                return cachedValue;

            // If not cached, retrieve synchronously (blocking)
            return GetSecretAsync(secretName).GetAwaiter().GetResult();
        }
    }
}
