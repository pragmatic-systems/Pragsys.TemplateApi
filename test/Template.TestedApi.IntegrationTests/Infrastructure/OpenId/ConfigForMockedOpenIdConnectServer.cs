using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

public class ConfigForMockedOpenIdConnectServer
{
    public static IConfigurationManager<OpenIdConnectConfiguration> Create(
        OpenIdConnectDiscoveryDocumentConfiguration config,
        PemCertificate signingCertificate,
        string openIdConfigUrl)
    {
        var openIdHttpClient = new HttpClient(
            new MockingOpenIdProviderMessageHandler(config, signingCertificate, openIdConfigUrl));

        return new ConfigurationManager<OpenIdConnectConfiguration>(
            openIdConfigUrl, new OpenIdConnectConfigurationRetriever(),
            new HttpDocumentRetriever(openIdHttpClient));
    }
}
