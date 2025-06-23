# Use the official .NET 9.0 SDK image for build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and restore as distinct layers
COPY "DealershipManagement.sln" ./
COPY "DealershipManagement/DealershipManagement.csproj" "DealershipManagement/DealershipManagement.csproj"
RUN dotnet restore "DealershipManagement/DealershipManagement.csproj"

# Copy the rest of the source code
COPY "DealershipManagement/" "DealershipManagement/"
WORKDIR "/src/DealershipManagement"
RUN dotnet publish -c Release -o /app/publish

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port 80
EXPOSE 80

# Ensure app listens on all interfaces
ENV ASPNETCORE_URLS=http://+:80

# Set environment variables if needed
# ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "DealershipManagement.dll"]
