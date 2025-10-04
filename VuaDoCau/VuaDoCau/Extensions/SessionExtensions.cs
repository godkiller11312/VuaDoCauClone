﻿using System.Text.Json;

namespace VuaDoCau.Extensions;

public static class SessionExtensions
{
    public static void SetObject<T>(this ISession session, string key, T value)
        => session.SetString(key, JsonSerializer.Serialize(value));

    public static T? GetObject<T>(this ISession session, string key)
        => session.GetString(key) is string json ? JsonSerializer.Deserialize<T>(json) : default;
}