// Copyright © Korshun Ivan. All rights reserved.

namespace Template.Library.UnitTests;

public static class ObjectExtensions
{
    public static T? GetTypeProperty<T>(this object obj, string property)
    {
        return (T?)obj.GetType().GetProperty(property)?.GetValue(obj);
    }
}
