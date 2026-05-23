#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

RUN addgroup --system nonroot \
    && adduser --system --ingroup nonroot --shell /bin/sh nonroot

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Template.TestedApi.Api/Template.TestedApi.Api.csproj", "src/Template.TestedApi.Api/"]
RUN dotnet restore "src/Template.TestedApi.Api/Template.TestedApi.Api.csproj"
COPY /src /src
WORKDIR "/src/Template.TestedApi.Api"
RUN dotnet build "Template.TestedApi.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.TestedApi.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER nonroot
ENTRYPOINT ["dotnet", "Template.TestedApi.Api.dll"]