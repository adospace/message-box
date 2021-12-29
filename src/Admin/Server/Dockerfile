FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 8300
EXPOSE 8301

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Admin/Server/MessageBox.Server.Tcp.Host.Server.csproj", "Server/"]
COPY ["MessageBox.Server.Tcp/MessageBox.Server.Tcp.csproj", "MessageBox.Server.Tcp/"]
COPY ["MessageBox.Server/MessageBox.Server.csproj", "MessageBox.Server/"]
COPY ["MessageBox.Core/MessageBox.Core.csproj", "MessageBox.Core/"]
COPY ["MessageBox.Tcp/MessageBox.Tcp.csproj", "MessageBox.Tcp/"]
COPY ["Admin/Client/MessageBox.Server.Tcp.Host.Client.csproj", "Client/"]
COPY ["Admin/Shared/MessageBox.Server.Tcp.Host.Shared.csproj", "Shared/"]
RUN dotnet restore "Admin/Server/MessageBox.Server.Tcp.Host.Server.csproj"
COPY . .
WORKDIR "/src/Server"
RUN dotnet build "MessageBox.Server.Tcp.Host.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MessageBox.Server.Tcp.Host.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MessageBox.Server.Tcp.Host.Server.dll"]
