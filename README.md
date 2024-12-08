### Message Template.TestedApi

## Template Project
This project was created from a template.

## Overview

This project contains a postgres DB, an API, and a DB up project to handle DB migrations.

* OpenIdConnect support for JWT based auth.
* `Ping`, `Health`, and `Metrics` endpoints for service observability.
* Database migration project to managed postgres migrations.
* Unit Tests, Integration and Benchmarks with packaged reports.
* Test Server / Test Container based test suite with Specflow.

### Database Migrations
Currently we run migrations on app-start, this simplifies startup and development, but for more mature projects we can 

### To Run

Run `docker compose -f docker-compose.infra.yml up` 
Launch apps:
* Template.TestedApi.API


### Docker Pack and Push

for the purpose of getting started, I am using `ghcr.io/TristanRhodes` and the default `USERNAME`

`dotnet cake --Target=DockerPackAndPush --ContainerRegistry=ghcr.io/TristanRhodes --ContainerRegistryToken={token} --ContainerRegistryUserName=USERNAME`