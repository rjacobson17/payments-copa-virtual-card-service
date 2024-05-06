#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
ARG AUTOMATION_NUGET_PASSWORD
ARG AUTOMATION_USER
ARG URL
RUN dotnet nuget add source $URL --name github --username $AUTOMATION_USER --password $AUTOMATION_NUGET_PASSWORD --store-password-in-clear-text
COPY ["Conservice.COPA.VirtualCardAPI/Conservice.COPA.VirtualCardAPI.csproj", "Conservice.COPA.VirtualCardAPI/"]
RUN dotnet restore "Conservice.COPA.VirtualCardAPI/Conservice.COPA.VirtualCardAPI.csproj"
COPY . .
WORKDIR "/src/Conservice.COPA.VirtualCardAPI"
RUN dotnet build "Conservice.COPA.VirtualCardAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Conservice.COPA.VirtualCardAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Conservice.COPA.VirtualCardAPI.dll"]
