# Use the official .NET SDK image to build and publish the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy the project files and restore dependencies
COPY *.sln .
COPY AuctionX/*.csproj ./AuctionX/
RUN dotnet restore

# Copy the rest of the files and publish the application
COPY . .
WORKDIR /src/AuctionX
RUN dotnet publish -c Release -o /app/publish

# Use the ASP.NET Core runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expose port and run the app
EXPOSE 80
ENTRYPOINT ["dotnet", "AuctionX.dll"]
