using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Acme.Core.Extensions;

public static class SqlExtensions
{
    public static async void Disconnect<T>(this SqlConnection connection, ILogger<T> logger)
    {
        logger.LogInformation("Closing Snowflake DB Connection.");
        await connection.CloseAsync();
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(this SqlConnection connection, SqlCommand command)
    {
        return await connection.QueryAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> ExecuteScalarAsync<T>(this SqlConnection connection, SqlCommand command)
    {
        return await connection.ExecuteScalarAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> QueryFirstOrDefaultAsync<T>(this SqlConnection connection, SqlCommand command)
    {
        return await connection.QueryFirstOrDefaultAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> QueryFirstAsync<T>(this SqlConnection connection, SqlCommand command)
    {
        return await connection.QueryFirstAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }
}