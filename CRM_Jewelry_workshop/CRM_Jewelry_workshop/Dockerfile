# Cборка приложения на .NET 9
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Копируем csproj и восстанавливаем зависимости
COPY CRM_Jewelry_workshop.csproj .
RUN dotnet restore "CRM_Jewelry_workshop.csproj"

# Копируем весь исходный код (включая wwwroot)
COPY . .

# Публикуем приложение (Release)
RUN dotnet publish "CRM_Jewelry_workshop.csproj" -c Release -o /app/publish

# запуск (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Создаём папку для SQLite (будет использоваться том)
RUN mkdir -p /app/Data

# Копируем собранное приложение 
COPY --from=build /app/publish .

# Порт приложения
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Точка входа
ENTRYPOINT ["dotnet", "CRM_Jewelry_workshop.dll"]