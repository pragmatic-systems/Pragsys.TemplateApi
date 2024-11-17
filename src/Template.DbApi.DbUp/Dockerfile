#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["src/Template.DbApi.DbUp/Template.DbApi.DbUp.csproj", "src/Template.DbApi.DbUp/"]
RUN dotnet restore "src/Template.DbApi.DbUp/Template.DbApi.DbUp.csproj"
COPY . .
WORKDIR "/src/src/Template.DbApi.DbUp"
RUN dotnet build "Template.DbApi.DbUp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Template.DbApi.DbUp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Template.DbApi.DbUp.dll"]