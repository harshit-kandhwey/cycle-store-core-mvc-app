using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace AdventureWorksMVCCore.Web.Models
{
    /// <summary>
    /// A simple guest cart persisted in the session as productId -> quantity.
    /// No database or account is required (the CYCLE_STORE schema has no order tables).
    /// </summary>
    public static class CartStore
    {
        private const string Key = "cart";

        public static Dictionary<int, int> Get(ISession session)
        {
            var json = session.GetString(Key);
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

        public static void Save(ISession session, Dictionary<int, int> cart)
            => session.SetString(Key, JsonSerializer.Serialize(cart));

        public static int Count(ISession session)
            => Get(session).Values.Sum();

        public static void Add(ISession session, int productId, int qty)
        {
            if (qty < 1) qty = 1;
            var cart = Get(session);
            cart.TryGetValue(productId, out var cur);
            cart[productId] = cur + qty;
            Save(session, cart);
        }

        public static void SetQty(ISession session, int productId, int qty)
        {
            var cart = Get(session);
            if (qty <= 0) cart.Remove(productId);
            else cart[productId] = qty;
            Save(session, cart);
        }

        public static void Remove(ISession session, int productId)
        {
            var cart = Get(session);
            if (cart.Remove(productId)) Save(session, cart);
        }

        public static void Clear(ISession session) => session.Remove(Key);
    }
}
