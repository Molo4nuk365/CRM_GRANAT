# CRM Jewelry Workshop

CRM-система для ювелирной мастерской с управлением заказами, пользователями, материалами и товарами.

## Технологии

- .NET 9 (ASP.NET Core Web API)
- Entity Framework Core  (ORM)
- SQL Server (база данных) года
- JWT (аутентификация)
- BCrypt (хеширование паролей)
- Docker (контейнеризация)
- Visual Studio 2026 года
- Visual Code 1.119.0 
## Роли и права доступа

- **admin** – Полный доступ: управление пользователями, ролями, материалами, товарами
- **manager** – Просмотр всех заказов, назначение ювелира, изменение статуса
- **jeweler** – Просмотр и выполнение своих заказов, установка срока, изменение статуса
- **client** – Создание заказов, оплата, просмотр своих заказов

## Тестовые учётные записи

Автоматически создаются через `SeedData` при первом запуске.

- Логин: admin, пароль: admin123 (роль admin)
- Логин: manager, пароль: manager123 (роль manager)
- Логин: jeweler, пароль: jeweler123 (роль jeweler)
 - Логин: client, пароль: client123 (роль client)

## Ошибка
Заказы фиксируются из API в SQL Server , но с SQL Server на панель не фиксируется в ролях:admin,manager,jeweler,client

## Быстрый старт

### 1. Клонирование репозитория

```bash
git clone https://github.com/yourusername/CRM_Jewelry_workshop.git
cd CRM_Jewelry_workshop

## Запуск через Docker
### Предварительные требования

- Установленный [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows/Linux/macOS) или Docker Engine + Docker Compose.
- Свободные порты: `5054` (API), `1433` (SQL Server). При необходимости измените в `docker-compose.yml`.

### 1. Сборка и запуск контейнеров

Из корневой папки проекта выполните:

```bash
docker-compose up --build

