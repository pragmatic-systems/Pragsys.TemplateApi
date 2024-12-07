namespace Template.TestedApi.Api;

public static class AppClaimTypes
{
    public const string PermissionClaimType = "permission";
}

public static class Permissions
{
    public const string TodoListRead = "TodoList:Read";
    public const string TodoListWrite = "TodoList:Write";
}

//TODO: Move to config
public static class AppConstantsThatShouldBeConfig
{
    public static string Issuer { get; } = $"Issuer:Dotnet:TemplateApi:Tests:Project:Issuer";
    public static string Audience { get; } = $"Issuer:Dotnet:TemplateApi:Tests:Project:Audience";
}