// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using MySql.Data.MySqlClient;
using System;
using System.Data;

namespace Hummingbird.Extensions.HealthChecks
{
    // REVIEW: What are the appropriate guards for these functions?

    public static class HealthCheckBuilderMySqExtensions
    {
        public static HealthCheckBuilder AddMySqlCheck(this HealthCheckBuilder builder, string name, string connectionString)
        {
            Guard.ArgumentNotNull(nameof(builder), builder);

            return AddMySqlCheck(builder, name, connectionString, builder.DefaultCacheDuration);
        }

        public static HealthCheckBuilder AddMySqlCheck(this HealthCheckBuilder builder, string name, string connectionString, TimeSpan cacheDuration)
        {
            builder.AddCheck($"MySqlCheck({name})", async () =>
            {
                try
                {
                    //TODO: There is probably a much better way to do this.
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();
                        using (var command = connection.CreateCommand())
                        {
                            command.CommandType = CommandType.Text;
                            command.CommandText = "SELECT 1";
                            var result = (int)await command.ExecuteScalarAsync().ConfigureAwait(false);
                            if (result == 1)
                            {
                                return HealthCheckResult.Healthy($"MySqlCheck({name}): Healthy");
                            }

                            return HealthCheckResult.Unhealthy($"MySqlCheck({name}): Unhealthy");
                        }
                    }
                }
                catch (Exception ex)
                {
                    return HealthCheckResult.Unhealthy($"MySqlCheck({name}): Exception during check: {ex.GetType().FullName}");
                }
            }, cacheDuration);

            return builder;
        }
    }
}
