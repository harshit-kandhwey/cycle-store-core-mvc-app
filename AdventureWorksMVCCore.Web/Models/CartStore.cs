using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using AdventureWorksMVCCore.Web.Services;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// A simple guest cart persisted in the session as productId -> quantity.
    /// No database or account is required (the CYCLE_STORE schema has no order tables).
    /// Session key is retrieved from AWS Secrets Manager for secure configuration management.
    /// </summary>
    public class CartStore
    {
        private readonly ISecretsManager _secretsManager;
        private readonly string _sessionKey;

        public CartStore(ISecretsManager secretsManager)
        {
            _secretsManager = secretsManager;
            // Retrieve session key from AWS Secrets Manager
            // The secret should be stored as a simple string value in AWS Secrets Manager
            // with the name "AdventureWorks/CartSessionKey"
            _sessionKey = _secretsManager.GetSecret("AdventureWorks/CartSessionKey");
        }

        public Dictionary<int, int> Get(ISession session)
        {
            var json = session.GetString(_sessionKey);
            if (string.IsNullOrEmpty(json)) return new Dictionary<int, int>();
            try
            {
                return JsonSerializer.Deserialize<Dictionary<int, int>>(json) ?? new Dictionary<int, int>();
            }
            catch (JsonException)
            {
                // Corrupted session payload -> treat as an empty cart rather than 500.
                return new Dictionary<int, int>();
            }
        }

        public void Save(ISession session, Dictionary<int, int> cart)
            => session.SetString(_sessionKey, JsonSerializer.Serialize(cart));

        public int Count(ISession session)
            => Get(session).Values.Sum();

        public void Add(ISession session, int productId, int qty)
        {
            if (qty < 1) qty = 1;
            var cart = Get(session);
            cart.TryGetValue(productId, out var cur);
            cart[productId] = cur + qty;
            Save(session, cart);
        }

        public void SetQty(ISession session, int productId, int qty)
        {
            var cart = Get(session);
            if (qty <= 0) cart.Remove(productId);
            else cart[productId] = qty;
            Save(session, cart);
        }

        public void Remove(ISession session, int productId)
        {
            var cart = Get(session);
            if (cart.Remove(productId)) Save(session, cart);
        }

        public void Clear(ISession session) => session.Remove(_sessionKey);
    }
}
