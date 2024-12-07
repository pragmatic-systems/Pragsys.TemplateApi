namespace Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

public class Consts
{
    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();

    public static string WellKnownOpenIdConfiguration { get; set; } = "https://i.do.not.exist/.well-known/openid-configuration";
}
