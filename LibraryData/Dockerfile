#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["LibraryData/LibraryData.csproj", "LibraryData/"]
RUN dotnet restore "LibraryData/LibraryData.csproj"
COPY . .
WORKDIR "/src/LibraryData"
RUN dotnet build "LibraryData.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibraryData.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "LibraryData.dll"]