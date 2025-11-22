# BackTurnedSharedStorage Plugin for Unturned

A plugin for Unturned that enables multiple players to access the same storage container simultaneously.

## Features

- **Shared Storage Access**: Multiple players can use the same storage container at the same time
- **Configurable Player Limits**: Set maximum players per storage instance
- **Toggle Functionality**: Easily enable or disable the plugin functionality
- **Lightweight**: Minimal performance impact on your server

## Installation

1. Download the latest `BackTurnedSharedStorage.dll` from the [Releases](https://github.com/backturned/BackTurnedSharedStorage/Releases) page
2. Place the DLL file in your server's `Rocket\Plugins` folder
1. Download the latest `0Harmony.dll` from the [Libraries](https://github.com/backturned/BackTurnedSharedStorage/Libraries) page
4. Place the DLL file in your server's `Rocket\Libraries` folder
5. Restart your Unturned server

## Configuration

The plugin creates a configuration file automatically. You can modify `BackTurnedSharedStorage.configuration.xml` in the `plugins\BackTurnedSharedStorage` folder:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SharedStorageConfiguration>
  <Enabled>true</Enabled>  // Enable/disable the ability for multiple players to use the same storage
  <MaxPlayersPerStorage>4</MaxPlayersPerStorage>  // Maximum number of players that can use the same storage at the same time
</SharedStorageConfiguration>
```





# BackTurnedSharedStorage Плагин для Unturned

Плагин для Unturned, который позволяет нескольким игрокам одновременно пользоваться одним и тем же хранилищем.

## Функции

- **Общий доступ к хранилищу**: Несколько игроков могут одновременно использовать один контейнер для хранения.
- **Настраиваемые лимиты игроков**: Установите максимальное количество игроков, которые могут пользоваться одним хранилищем.
- **Включение/выключение функциональности**: Легко активируйте или деактивируйте функции плагина.
- **Легковесный**: Минимальное влияние на производительность вашего сервера.

## Установка

1. Скачайте последнюю версию `BackTurnedSharedStorage.dll` со страницы [Releases](https://github.com/backturned/BackTurnedSharedStorage/Releases)
2. Поместите DLL-файл в папку `Rocket\Plugins` вашего сервера
1. Скачайте последнюю версию `0Harmony.dll` со страницы [Libraries](https://github.com/backturned/BackTurnedSharedStorage/Libraries)
4. Поместите DLL-файл в папку `Rocket\Libraries` вашего сервера
5. Перезапустите ваш сервер Unturned

## Конфигурация

Плагин автоматически создает файл конфигурации. Вы можете изменить `BackTurnedSharedStorage.configuration.xml` в папке `plugins\BackTurnedSharedStorage` вашего сервера:

```xml
<?xml version="1.0" encoding="utf-8"?>
<SharedStorageConfiguration>
  <Enabled>true</Enabled>  // Включить/выключить возможность нескольким игрокам использовать одно хранилище
  <MaxPlayersPerStorage>4</MaxPlayersPerStorage>  // Максимальное количество игроков, которые могут использовать одно хранилище одновременно
</SharedStorageConfiguration>
```
