using System.Reflection;
using Acme.Core.Enums;
using Acme.Core.Filters;
using Acme.Core.Models.AppSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Acme.Core.Extensions
{
    /// <summary>
    /// Used in startup in conjunction with an IConfiguration reading from an appsettings.json.
    /// Utilizes the AppName and AppVersion fields to auto generate a swagger page.
    /// ConfigureServices -> GenerateSwagger(IConfiguration Configuration)
    /// Configure -> UseSwagger(IConfiguration Configuration)
    /// </summary>
    public static class SwaggerGenerationExtensions
    {
        public const string AppName = "AppName";
        public const string AppVersion = "AppVersion";
        public const string OAuth2 = "oauth2";
        public const string Host = "Host";

        /// <summary>
        /// Generates the backing Swagger Code. Can be provided an Authentication type to generate specific Authentication mechanisms. Also adds OData compatibility.
        /// </summary>
        /// <param name="serviceCollection"></param>
        /// <param name="configuration"></param>
        /// <param name="scopes"></param>
        /// <param name="authType"></param>
        public static void GenerateSwagger(this IServiceCollection serviceCollection, IConfiguration configuration,
            Dictionary<string, string> scopes, AuthenticationType authType)
        {
            var appName = configuration.GetValue<string>(AppName);
            var version = configuration.GetValue<string>(AppVersion);
            var host = configuration.GetValue<string>(Host);

            if (string.IsNullOrWhiteSpace(appName))
            {
                ThrowNullError(nameof(GenerateSwagger), "Swagger Generation Configuration Section null.");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                //Version field should always be set in appsettings.json.
                //In the event it is not we default to V1, appsettings.json override in CD should always be populated.
                version = "V1";
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                ThrowNullError(nameof(GenerateSwagger), "Swagger Generation Configuration Section null.");
            }

            serviceCollection.AddSwaggerGen(
                c =>
                {
                    #region SwaggerFilters

                    c.SchemaFilter<EnumSchemaFilter>();
                    c.SchemaFilter<SwaggerIgnoreFilter>();

                    #endregion

                    /*
                        "host": "api.nprd.ccbcc.com", //TODO: ADD THIS GUY TO THE SWAGGER 2 GENERATION
                        "basePath": "/APIPOC", //TODO: ADD THIS GUY TO THE SWAGGER 2 GENERATION
                     */

                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = appName,
                        Version = version,
                        Extensions = new Dictionary<string, IOpenApiExtension>
                        {
                            //This is some specific code for Nintex, as it enables automatic object parsing instead of its older v1 variant which required expression parsing.
                            //If you dont have this Nintex won't work as intended. Blame them ¯\_(ツ)_/¯ 
                            {"x-ntx-render-version", new OpenApiString("2")},
                            {"host", new OpenApiString(host)}
                        }
                    });

                    //Authentication switch for multiple implementations
                    if (!authType.Equals(AuthenticationType.None))
                    {
                        OpenApiSecurityScheme scheme = new()
                        {
                            Type = SecuritySchemeType.OAuth2,
                            Flows = new OpenApiOAuthFlows()
                        };

                        var oAuth2 = configuration.GetSection(OAuth2).Get<OAuth2Credentials>();

                        if (oAuth2 == null || oAuth2.IsAnyNullOrEmpty())
                        {
                            ThrowNullError(nameof(GenerateSwagger), "OAuth2 Configuration Section null.");
                        }

                        if (authType.HasFlag(AuthenticationType.OAuthPKCE))
                        {
                            scheme.Flows.AuthorizationCode = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(oAuth2.AuthorizationEndpoint),
                                TokenUrl = new Uri(oAuth2.TokenEndpoint),
                                Scopes = scopes,
                                //Extensions = ,
                                //RefreshUrl = new Uri("")
                            };
                        }

                        if (authType.HasFlag(AuthenticationType.OAuthImplicit))
                        {
                            scheme.Flows.Implicit = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(oAuth2.AuthorizationEndpoint),
                                TokenUrl = new Uri(oAuth2.TokenEndpoint),
                                Scopes = scopes,
                                //Extensions = ,
                                //RefreshUrl = new Uri("")
                            };
                        }

                        if (authType.HasFlag(AuthenticationType.OAuthClientCredentials))
                        {
                            scheme.Flows.ClientCredentials = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(oAuth2.AuthorizationEndpoint),
                                TokenUrl = new Uri(oAuth2.TokenEndpoint),
                                Scopes = scopes,
                                //Extensions = ,
                                //RefreshUrl = new Uri("")
                            };
                        }

                        if (authType.HasFlag(AuthenticationType.OAuthPassword))
                        {
                            scheme.Flows.Password = new OpenApiOAuthFlow
                            {
                                AuthorizationUrl = new Uri(oAuth2.AuthorizationEndpoint),
                                TokenUrl = new Uri(oAuth2.TokenEndpoint),
                                Scopes = scopes,
                                //Extensions = ,
                                //RefreshUrl = new Uri("")
                            };
                        }

                        //Once Flows are finished building add definition and then define security requirements
                        c.AddSecurityDefinition("oauth2", scheme);

                        c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                        {
                            {
                                new OpenApiSecurityScheme
                                {
                                    Reference = new OpenApiReference
                                    {
                                        Type = ReferenceType.SecurityScheme,
                                        Id = OAuth2
                                    },
                                    Scheme = OAuth2,
                                    Name = OAuth2,
                                    In = ParameterLocation.Header
                                },
                                scopes.Keys.ToArray()
                            }
                        });

                        //Set Comments Path
                        var xmlFile = $"{Assembly.GetEntryAssembly()?.GetName().Name}.xml";
                        var environment = AppContext.BaseDirectory;
                        var xml = Path.Combine(environment + xmlFile);
                        c.IncludeXmlComments(xml);
                    }
                });

            serviceCollection.AddSwaggerGenNewtonsoftSupport();
        }

        /// <summary>
        /// Generates the Swagger UI that can be found at the api-docs endpoint. Ex: {baseUrl}/api-docs/index.html | OpenApi3 spec can be located at: {baseUrl}/swagger/{version}/swagger.json
        /// </summary>
        /// <param name="application"></param>
        /// <param name="configuration"></param>
        /// <param name="authType"></param>
        public static void UseSwagger(this IApplicationBuilder application, IConfiguration configuration,
            AuthenticationType authType = AuthenticationType.None)
        {
            var appName = configuration.GetValue<string>(AppName);
            var version = configuration.GetValue<string>(AppVersion);

            if (string.IsNullOrWhiteSpace(appName))
            {
                ThrowNullError(nameof(UseSwagger), "Swagger Generation Configuration Section null.");
            }

            if (string.IsNullOrWhiteSpace(version))
            {
                //Version field should always be set in appsettings.json.
                //In the event it is not we default to V1, appsettings.json override in CD should always be populated.
                version = "V1";
            }

            application.UseSwagger();

            //Experimental
            // serve v3 from /swagger/v1/swagger.json
            application.UseSwagger(o => o.RouteTemplate = "swagger/{documentName}/swagger.json");

            // serve v2 from /swagger2/v1/swagger.json
            application.Map("/swagger2", swaggerApp =>
                swaggerApp.UseSwagger(options =>
                {
                    // note the dropped prefix "swagger/"
                    options.RouteTemplate = "{documentName}/swagger.json";
                    options.SerializeAsV2 = true;
                })
            );

            application.UseSwaggerUI(s =>
            {
                s.RoutePrefix = "api-docs";

                //OpenApi3 definition
                var openApi3Endpoint = "../swagger/" + version + "/swagger.json";
                s.SwaggerEndpoint(openApi3Endpoint, appName);

                if (authType != AuthenticationType.None)
                {
                    var oAuth2 = configuration.GetSection(OAuth2).Get<OAuth2Credentials>();

                    if (oAuth2 == null || oAuth2.IsAnyNullOrEmpty())
                    {
                        ThrowNullError(nameof(UseSwagger), "OAuth2 Configuration Section null.");
                    }

                    s.OAuthClientId(oAuth2?.ClientId);
                    s.OAuthAppName(AppName);
                    s.OAuthScopeSeparator(" ");
                    s.OAuthUsePkce();
                    s.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                }

                //Used to improve render performance for the Swagger UI for large response objects
                s.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
                {
                    ["activated"] = true
                };
            });
        }

        private static void ThrowNullError(string methodName, string msg)
        {
            var error = $"Failed to Generate Swagger. {methodName} | {msg}";

            throw new ArgumentNullException(error);
        }
    }
}