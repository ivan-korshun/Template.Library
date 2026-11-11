// Copyright © Korshun Ivan. All rights reserved.

using System.Reflection;
using Microsoft.Data.SqlClient;

namespace Template.Library.UnitTests;

public static class SqlExceptionFactory
{
    public static SqlException CreateSqlException(int number, string errorMessage = "error message")
    {
        SqlErrorCollection errorCollection = CreateSqlErrorCollection();

        SqlError error = CreateSqlError(number);

        AddSqlErrorToSqlErrorCollection(errorCollection, error);

        return CreateSqlException(errorMessage, errorCollection);
    }

    private static SqlErrorCollection CreateSqlErrorCollection()
    {
        var collectionConstructor = typeof(SqlErrorCollection).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);

        return (SqlErrorCollection)collectionConstructor!.Invoke(null);
    }

    private static SqlError CreateSqlError(int number)
    {
        var errorConstructor = typeof(SqlError).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [
                typeof(int),
                typeof(byte),
                typeof(byte),
                typeof(string),
                typeof(string),
                typeof(string),
                typeof(int),
                typeof(Exception)
            ],
            null);

        return (SqlError)errorConstructor!.Invoke([
            number,
            (byte)0,
            (byte)0,
            "server",
            "errMsg",
            "procedure",
            100,
            null!
        ]);
    }

    private static void AddSqlErrorToSqlErrorCollection(SqlErrorCollection errorCollection, SqlError error)
    {
        var addMethod = typeof(SqlErrorCollection).GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance);
        addMethod!.Invoke(errorCollection, [error]);
    }

    private static SqlException CreateSqlException(string errorMessage, SqlErrorCollection errorCollection)
    {
        var constructor = typeof(SqlException).GetConstructor(
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            [
                typeof(string),
                typeof(SqlErrorCollection),
                typeof(Exception),
                typeof(Guid)
            ],
            null);

        return (SqlException)constructor!.Invoke([
            errorMessage,
            errorCollection,
            null,
            Guid.NewGuid()
        ]);
    }
}
