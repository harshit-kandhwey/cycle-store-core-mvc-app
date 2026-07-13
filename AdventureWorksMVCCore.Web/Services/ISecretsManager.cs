using System;
using System.Threading.Tasks;

namespace AdventureWorksMVCCore.Web.Services
{
    /// <summary>
    /// Interface for AWS Secrets Manager integration.
    /// </summary>
    public interface ISecretsManager
    {
        /// <summary>
        /// Retrieves a secret value from AWS Secrets Manager.
        /// </summary>
        Task<string> GetSecretAsync(string secretName);

        /// <summary>
        /// Retrieves a secret value synchronously (cached after first retrieval).
        /// </summary>
        string GetSecret(string secretName);
    }
}
