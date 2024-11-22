#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Template.TestedApi.DbUp/Template.TestedApi.DbUp.csproj", "src/Template.TestedApi.DbUp/"]
RUN dotnet restore "src/Template.TestedApi.DbUp/Template.TestedApi.DbUp.csproj"
COPY . .
WORKDIR "/src/src/Template.TestedApi.DbUp"
RUN dotnet build "Template.TestedApi.DbUp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.TestedApi.DbUp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.TestedApi.DbUp.dll"]