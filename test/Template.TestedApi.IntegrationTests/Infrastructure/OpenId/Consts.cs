namespace Template.TestedApi.IntegrationTests.Infrastructure.OpenId;

public class Consts
{
    public static PemCertificate ValidSigningCertificate { get; } = SelfSignedAccessTokenPemCertificateFactory.Create();
}
