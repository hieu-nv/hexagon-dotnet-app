using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Configuration;
using Sustainsys.Saml2.Metadata;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Extension methods for configuring SAML 2.0 authentication.
/// </summary>
public static class Saml2Extensions
{
    /// <summary>
    /// Adds SAML 2.0 authentication to the application.
    /// </summary>
    /// <param name="builder">The WebApplicationBuilder</param>
    /// <param name="configureOptions">Action to configure SAML options</param>
    /// <returns>The WebApplicationBuilder</returns>
    public static WebApplicationBuilder AddSaml2Authentication(
        this WebApplicationBuilder builder,
        Action<App.Api.Auth.Saml2Options>? configureOptions = null
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        // Load SAML configuration from appsettings or use provided options
        var samlOptions = new App.Api.Auth.Saml2Options();
        configureOptions?.Invoke(samlOptions);

        // If not configured via action, try loading from configuration
        if (string.IsNullOrEmpty(samlOptions.KeycloakUrl))
        {
            builder.Configuration.GetSection("Saml2").Bind(samlOptions);
        }

        if (string.IsNullOrEmpty(samlOptions.Realm))
        {
            throw new InvalidOperationException(
                "SAML2 Realm is not configured. Set 'Saml2:Realm' in appsettings.json or configure via AddSaml2Authentication"
            );
        }

        if (string.IsNullOrEmpty(samlOptions.KeycloakUrl))
        {
            throw new InvalidOperationException(
                "SAML2 KeycloakUrl is not configured. Set 'Saml2:KeycloakUrl' in appsettings.json or configure via AddSaml2Authentication"
            );
        }

        if (!samlOptions.ValidateSslCertificate)
        {
#pragma warning disable S5415,SCS0004 // Certificate validation is disabled - only for development
            System.Net.ServicePointManager.ServerCertificateValidationCallback = (_, _, _, _) =>
                true;
#pragma warning restore S5415,SCS0004
        }

        builder
            .Services.AddAuthentication(Saml2Defaults.Scheme)
            .AddSaml2(options =>
            {
                options.SPOptions.EntityId = new EntityId(samlOptions.ClientId);
                options.SPOptions.ReturnUrl = new Uri(samlOptions.AssertionConsumerServiceUrl);

                // Load metadata from Keycloak
                var metadataUrl = samlOptions.GetMetadataUrl();

                // Create identity provider from metadata
                var identityProvider = new IdentityProvider(
                    new EntityId(samlOptions.GetSsoUrl()),
                    options.SPOptions
                )
                {
                    MetadataLocation = metadataUrl,
                };

                options.IdentityProviders.Add(identityProvider);
            });

        // Store SAML options in DI for later use
        builder.Services.AddSingleton(samlOptions);

        return builder;
    }

    /// <summary>
    /// Maps SAML 2.0 authentication endpoints and metadata.
    /// </summary>
    /// <param name="app">The WebApplication</param>
    /// <returns>The WebApplication</returns>
    public static WebApplication UseSaml2(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);

        // SAML middleware is automatically added by the authentication system
        // This method can be used for additional configuration if needed

        return app;
    }
}
