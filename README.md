## Template Project
This project was created from a template.

## Overview

This solution contains a postgres DB, a simple Todo List API, and a DB up project to handle DB migrations.

### Features
* OpenIdConnect support for JWT based auth and RBAC.
* `/_system/Ping`, `/_system/Health`, and `/_system/Metrics` endpoints for service observability.
* Database migration project to managed postgres migrations.
* Unit Tests, Integration Tests and Benchmarks with packaged reports.
* Test Server / Test Container based test suite with Specflow.

### Database Migrations
Currently we run migrations on app-start, this simplifies startup and development, but for more mature projects we can seperate the launch application and run this prior to deploying a cluster.

### To Run

Run `docker compose -f docker-compose.infra.yml up` 
Launch apps:
* Template.TestedApi.API

## Local Services

### Seq
Url: https://localhost:5030

### Postgres (+ PgAdmin)
Adminer: https://localhost:8443
Username: user@local.com
Password: password

Postgres: https://localhost:5432
Username: pgadmin
Password: password

### Keycloak
Url: https://localhost:8443
Username: admin
Password: password

### Authentication
If you have an existing AWS Cognito or Azure Entra setup, you can skip local keycloak setup and register the application with them.

## Local Keycloak

For full local development, you will need to configure Keycloak, which takes a bit more setup than just launching a docker container.

### Keycloak SSL Setup

For local keycloak to work properly as an OIDC server with another application, it needs to be running HTTPS with a valid, trusted certificate, otherwise you will recieve an SSL error at runtime. To support this, you will need to generate a certificate for localhost and add it to trusted root, and load this certificate into Keycloak.

See the SSL Setup guide for local Keycloak here: https://github.com/TristanRhodes/docker-compose

### Keycloak Configuration

### Create Relm
Go to the dropdown `master` and create a new realm `todolist-realm`. It's worth bearing in mind that a Realm can represent all your users across multiple applications.

In realm settings, set Unmanaged Attributes to `Only administrators can write`. This causes the Attributes table to be shown in User accounts, and will be where we add our custom role configruations.

### Create Client
In your new Realm, create a new client for your application `todolist-client`, ensuring you have enabled `Client authentication` and `Client authorization` and `Direct access grants`.

In your new client, go to `client Scopes` => `todolist-client-dedicated` and:
* add a custom attribute mapping for user attribute `roles`, and set `Multivalued` = true. This will take any user attributes called `roles` and include them in the JWT token when loaded.
* add a custom attribute mapping for audience and include the client name.

Under Client Credentials, record the `clientsecret` for later.

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
OpenIdConfigUrl: `https://localhost:8443/realms/todolist-realm/.well-known/openid-configuration`

## Azure EntraId Config
* TODO

## AWS Cognito Config
* TODO


## Docker Pack and Push

For the purpose of getting started, I am using `ghcr.io/TristanRhodes` and the default `USERNAME`

`dotnet cake --Target=DockerPackAndPush --ContainerRegistry=ghcr.io/TristanRhodes --ContainerRegistryToken={token} --ContainerRegistryUserName=USERNAME`