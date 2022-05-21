#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["CourierService/CourierService.csproj", "CourierService/"]
COPY ["Library/Library.csproj", "Library/"]
COPY ["UserService/UserService.csproj", "UserService/"]
RUN dotnet restore "CourierService/CourierService.csproj"
COPY . .
WORKDIR "/src/CourierService"
RUN dotnet build "CourierService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CourierService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CourierService.dll"]