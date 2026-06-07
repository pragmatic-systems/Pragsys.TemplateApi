using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;

namespace Template.TestedApi.Api.Auth;

// Transforms AWS Cognito "scope" claims into ClaimTypes.Role claims so that
// policy-based authorization (e.g. [Authorize(Policy = Roles.TodoListRead)]) works.
//
// Scopes are expected to be space-separated and prefixed with "todolist-permissions/".
// Example scope value: "todolist-permissions/TodoList:Read todolist-permissions/TodoList:Write"
public class ScopeToRoleClaimsTransformer : IClaimsTransformation
{
    private const string ScopeType = "scope";
    private const string RolePrefix = "todolist-permissions/";

    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not { IsAuthenticated: true })
            return Task.FromResult(principal);

        var roleClaims = principal.FindAll(ScopeType)
            .SelectMany(scopeClaim => scopeClaim.Value.Split(' '))
            .Select(raw => raw.Replace(RolePrefix, string.Empty))
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(role => new Claim(ClaimTypes.Role, role!));

        var identity = new ClaimsIdentity(roleClaims);
        principal.AddIdentity(identity);

        return Task.FromResult(principal);
    }
}
