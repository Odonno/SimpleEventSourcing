using System;

namespace SimpleEventSourcing
{
    public static class Extensions
    {
        public static bool IsValidGuid(string id)
        {
            return !string.IsNullOrWhiteSpace(id) && Guid.TryParse(id, out var _);
        }
    }
}
