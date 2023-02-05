using Acme.Core.Models.AppSettings;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Snowflake.Data.Client;

namespace Acme.Core.Extensions;

public static class SnowflakeExtensions
{
    private const string SnowflakeConnectionTokenTemplate =
        "SCHEMA={0};DB={1};WAREHOUSE={2};ROLE={3};AUTHENTICATOR=OAUTH;TOKEN={4};ACCOUNT={5};HOST={6}";

    private const string SnowflakeConnectionServiceAccountTemplate =
        "SCHEMA={0};DB={1};WAREHOUSE={2};ROLE={3};AUTHENTICATOR=OAUTH;ACCOUNT={4};HOST={5};USER={6};PASSWORD={7};";

    /// <summary>
    ///     Used when passing an OAuth2 Token to Snowflake
    /// </summary>
    /// <param name="credentials"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    public static SnowflakeDbConnection TokenConnect(this SnowflakeAccessCredentials credentials, string bearerToken)
    {
        var connection = new SnowflakeDbConnection();

        var connectionString = string.Format(SnowflakeConnectionTokenTemplate, credentials.Schema,
            credentials.Database, credentials.Warehouse, credentials.Role, bearerToken, credentials.Account,
            credentials.Host);

        connection.ConnectionString = connectionString;

        return connection;
    }

    /// <summary>
    ///     Used when passing a User to Snowflake and authenticating via an external browser
    /// </summary>
    /// <param name="snowflakeCredentials"></param>
    /// <param name="serviceAccountCredentials"></param>
    /// <returns></returns>
    public static SnowflakeDbConnection ServiceAccountConnect(this SnowflakeAccessCredentials snowflakeCredentials,
        ServiceAccountCredentials serviceAccountCredentials)
    {
        var connection = new SnowflakeDbConnection();

        var connectionString = string.Format(SnowflakeConnectionServiceAccountTemplate, snowflakeCredentials.Schema,
            snowflakeCredentials.Database, snowflakeCredentials.Warehouse, snowflakeCredentials.Role,
            snowflakeCredentials.Account, snowflakeCredentials.Host, serviceAccountCredentials.Username,
            serviceAccountCredentials.Password);

        connection.ConnectionString = connectionString;

        return connection;
    }

    public static async void Disconnect<T>(this SnowflakeDbConnection connection, ILogger<T> logger)
    {
        logger.LogInformation("Closing Snowflake DB Connection.");
        await connection.CloseAsync();
    }

    public static async Task<IEnumerable<T>> QueryAsync<T>(this SnowflakeDbConnection connection, SqlCommand command)
    {
        return await connection.QueryAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> ExecuteScalarAsync<T>(this SnowflakeDbConnection connection, SqlCommand command)
    {
        return await connection.ExecuteScalarAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> QueryFirstOrDefaultAsync<T>(this SnowflakeDbConnection connection, SqlCommand command)
    {
        return await connection.QueryFirstOrDefaultAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }

    public static async Task<T> QueryFirstAsync<T>(this SnowflakeDbConnection connection, SqlCommand command)
    {
        return await connection.QueryFirstAsync<T>(command.CommandText, command.Parameters.ToDapperParams());
    }
}