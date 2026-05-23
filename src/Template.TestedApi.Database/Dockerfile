#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

RUN addgroup --system nonroot \
    && adduser --system --ingroup nonroot --shell /bin/sh nonroot

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Template.TestedApi.Database/Template.TestedApi.Database.csproj", "src/Template.TestedApi.Database/"]
RUN dotnet restore "src/Template.TestedApi.Database/Template.TestedApi.Database.csproj"
COPY /src /src
WORKDIR "/src/Template.TestedApi.Database"
RUN dotnet build "Template.TestedApi.Database.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.TestedApi.Database.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
USER nonroot
ENTRYPOINT ["dotnet", "Template.TestedApi.Database.dll"]