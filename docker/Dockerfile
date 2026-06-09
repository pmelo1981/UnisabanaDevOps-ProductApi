# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/ProductAPI/ProductAPI.csproj", "src/ProductAPI/"]
RUN dotnet restore "src/ProductAPI/ProductAPI.csproj"
COPY . .
RUN dotnet build "src/ProductAPI/ProductAPI.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "src/ProductAPI/ProductAPI.csproj" -c Release -o /app/publish

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "ProductAPI.dll"]
