# Pragsys .NET API Template Header
This project is a template published by https://github.com/pragmatic-systems

To Install: 
* Clone Repo to local folder, eg `c:\git\Pragsys.TemplateApi`
* Run `dotnet new install c:\git\Pragsys.TemplateApi`

To Create a project: 
* Navigate to your new empty project directory.
* Run `dotnet new Pragsys.TemplateApi --ProjectName:MyAppName`

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=alert_status&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=security_rating&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=vulnerabilities&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=reliability_rating&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=sqale_rating&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=pragmatic-systems_Pragsys.Template.Api&metric=coverage&token=6047a2bbb68224d6ef02044a0b82b9aae2f68067)](https://sonarcloud.io/summary/new_code?id=pragmatic-systems_Pragsys.Template.Api)

## End Of Template Project Header

## Overview

This solution contains a postgres DB, a simple Todo List API, and a DB up project to handle DB migrations.

### Features
* OpenIdConnect support for JWT based auth and RBAC.
* `/_system/Ping`, `/_system/Health`, and `/_system/Metrics` endpoints for service observability.
* Database migration project to managed postgres migrations.
* Unit Tests, Integration Tests and Benchmarks with packaged reports.
* Test Server / Test Container based test suite with Specflow.

### Prerequisites

| Requirement | Version | Notes |
|-------------|---------|-------|
| **.NET SDK** | 8.0.401+ (rollForward: latestMinor) | [Download](https://dotnet.microsoft.com/download) |
| **Docker Desktop** | Latest stable | Required for `docker compose` and all local services |
| **Git** | Latest | Required for cloning and versioning |
| **Cake CLI** | 4.x | `dotnet tool install --global Cake.Tool` — used for the build pipeline |

### To Run

Run `docker compose -f docker-compose.infra.yml up` 
Launch apps:
* Pragsys.TemplateApi.API

## Formatting
The project needs to be correctly formatted to pass the build. To format the project using the dotnet format tool - 

Run `dotnet format`

Note that not all errors are auto resolvable and some require manual fixing.

## Local Services

### Seq
Url: https://localhost:5030

### Postgres (+ PgAdmin)
Adminer: https://localhost:5434
Username: user@local.com
Password: password

Postgres: https://localhost:5432
Username: pgadmin
Password: password

### Keycloak
Url: https://localhost:8443
Username: admin
Password: password

### Database Migrations
Currently we run migrations on app-start, this simplifies startup and development, but for more mature projects we can separate the launch application and run this prior to deploying a cluster.

### Authentication
If you have an existing AWS Cognito or Azure Entra setup, you can skip local keycloak setup and register the application with them.

## Local Keycloak

For full local development, you will need to configure Keycloak, which takes a bit more setup than just launching a docker container.

### Keycloak SSL Setup

For local keycloak to work properly as an OIDC server with another application, it needs to be running HTTPS with a valid, trusted certificate, otherwise you will receive an SSL error at runtime. To support this, you will need to generate a certificate for localhost and add it to trusted root, and load this certificate into Keycloak.

See the SSL Setup guide for local Keycloak in this repo: https://github.com/pragmatic-systems/Pragsys.DockerTools

### Keycloak Configuration

### Create Realm
Go to the dropdown `master` and create a new realm `todolist-realm`. It's worth bearing in mind that a Realm can represent all your users across multiple applications.

In realm settings, set Unmanaged Attributes to `Only administrators can write`. This causes the Attributes table to be shown in User accounts, and will be where we add our custom role configurations.

### Create Client
In your new Realm, create a new client for your application `todolist-client`, ensuring you have enabled `Client authentication` and `Client authorization` and `Direct access grants`.

In your new client, go to `client Scopes` => `todolist-client-dedicated` and:
* add a custom attribute mapping for user attribute `roles`. `Add mapper by configuration` => `User Attribute`. Name: `roles-mapper`, User Attribute: `roles`, Token Claim Name: `roles` and set `Multivalued` = true. This will take any user attributes called `roles` and include them in the JWT token when loaded.
* add a custom attribute mapping for audience and include the client name.

Under `Client > Client Details > Credentials`, record the `clientsecret` for later.

### Create User
Create an app user `todolist-user`. As we are creating a service to service role the account will need to be interaction free. Configure the user completely, including first and last name and email (this is required to activate a User account), set and record the password, ensure that it is not transient and there are no pending actions for the user.
Now add attributes (`roles`, `TodoList:Write`) and (`roles`, `TodoList:Read`) to the Attributes page.
	
### Generate JWT

Post: https://localhost:8443/realms/{myrealm}/protocol/openid-connect/token

With URL form:
grant_type: password
client_id: todolist-client
client_secret: {clientsecret}
username: todolist-user
password: {userpassword}

### Validate JWT
For the configuration in your dotnet application, you will need:

Issuer: `https://localhost:8443/realms/todolist-realm`
Audience: `todolist-client`

## Azure EntraId Config
* Go to Entra ID. (https://entra.microsoft.com/)
* Go to `App Registrations` -> Register your App. Note the `ApplicationId` / `ClientId`
* Go to `Authentication` -> Go to settings. Enable `Access tokens` and `ID tokens`
* Go to `Certificates and Secrets` -> Create a client secret, record the value.
* Go to `Token Configuration` -> Add optional claim `Access Token` -> , then select `acct`, `acrs`, `aud`, `email`, `family_name`, `given_name`
* Go to `Expose an API` -> Setup your `ApplicationID URI` -> formatted `api://todolist`
* Add a scope -> This will create a scope.
* Add a `Client Application` -> This will be the application consuming your API. It should be the ID for another registered app, and in the case of the demo, we will be our own owners. Use the AppId you just created.
* Go to `App roles` -> Create the roles for your applicaiton. In this case we are using `TodoList:Read` amd `TodoList:Write`
* Go to `Owners` -> Add your root account and any relevant accounts you want to manage this app.
* Go to `Api Permissions` -> Add a permission -> `My APIs` -> Chose your API -> `Application Permissions`
* Select the permissions you are interested in, both `TodoList:Read` and `TodoList:Write`.
* Ensure you `Grant Admin Consent` -> This means the roles show up in the JWT.

### Generate JWT

Post: https://login.microsoftonline.com/{tenant-id}/oauth2/v2.0/token

With URL form:
grant_type: client_credentials
client_id: {your-application-client-id}
client_secret: {your-client-secret}
scope: api://{your-application-client-id}/.default

## AWS Cognito Config
* Go to Amazon Cognito -> Manage User Pools -> Select your pool.
* Go to App Integration -> App clients.
* Select your client (or create one).
* Enable "Generate client secret".
* Go to "Authorization flows".
* Check "Access token - client credentials". (This is critical; without this, the request will fail).
* Go to "Token settings".
* Ensure Access Token is enabled.
* Go to "Custom Attributes" (Optional): If you want roles, you usually map Cognito Groups to claims or use Custom Attributes. Cognito doesn't have native "App Roles" like Azure. You typically use Cognito Groups (e.g., TodoList-Readers, TodoList-Writers) and assign the App Client to these groups, or use a Lambda Trigger to inject claims.

### Generate JWT

Post: https://your-region.auth.cognito-idp.amazonaws.com/{user-pool-id}/oauth2/token

With URL form:
grant_type: client_credentials
client_id: {your-cognito-client-id}
client_secret: {your-cognito-client-secret}

## Docker Pack and Push

`dotnet cake --Target=DockerPackAndPush --ContainerRegistry={container-registry} --ContainerRegistryToken={token} --ContainerRegistryUserName={user}`