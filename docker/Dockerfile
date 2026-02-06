# BotFlow/docker/Dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# نسخ ملفات المشروع
COPY ["BotFlow.API/BotFlow.API.csproj", "BotFlow.API/"]
COPY ["BotFlow.Application/BotFlow.Application.csproj", "BotFlow.Application/"]
COPY ["BotFlow.Domain/BotFlow.Domain.csproj", "BotFlow.Domain/"]
COPY ["BotFlow.Infrastructure/BotFlow.Infrastructure.csproj", "BotFlow.Infrastructure/"]

# استعادة الحزم
RUN dotnet restore "BotFlow.API/BotFlow.API.csproj"

# نسخ باقي الملفات
COPY . .

# الانتقال لمجلد API والبناء
WORKDIR "/src/BotFlow.API"
RUN dotnet build "BotFlow.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BotFlow.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BotFlow.API.dll"]