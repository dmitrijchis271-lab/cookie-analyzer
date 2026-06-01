# Анализатор Cookie

Приложение для анализа экспортированных cookie-файлов в формате JSON с полной поддержкой сканирования директорий, валидации данных и экспорта отчетов.

## Функциональность

✅ **Рекурсивное сканирование** директорий на наличие JSON-файлов с cookie  
✅ **Чтение и парсинг** cookie-данных из различных форматов JSON  
✅ **Валидация структуры** данных с выявлением ошибок  
✅ **Анализ cookie**:
  - Домен
  - Имя cookie
  - Дата истечения (с определением статуса)
  - Флаг Secure
  - Флаг HttpOnly
  - Дни до истечения
  - SameSite атрибут

✅ **Выявление просроченных cookie**  
✅ **Фильтрация** по статусу (истекшие, Secure только)  
✅ **Экспорт отчетов**:
  - CSV формат
  - JSON формат
  - Выбор папки для сохранения

✅ **Логирование ошибок** (Serilog)  
✅ **Графический интерфейс** (Avalonia UI + MVVM)  
✅ **ReactiveUI** для реактивного программирования  
✅ **Модульные тесты** (xUnit)

## Безопасность

🔒 **НЕ выполняет** авторизацию на веб-сайтах  
🔒 **НЕ отправляет** cookie на внешние серверы  
🔒 **Только локальный анализ** экспортированных файлов

## Требования

- .NET 8 SDK
- Windows, macOS или Linux

## Установка и запуск

### 1. Клонирование репозитория

```bash
git clone https://github.com/dmitrijchis271-lab/cookie-analyzer.git
cd cookie-analyzer
```

### 2. Восстановление зависимостей

```bash
dotnet restore
```

### 3. Запуск приложения

```bash
dotnet run --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
```

### 4. Запуск тестов

```bash
dotnet test
```

## Использование

### Основной процесс

1. **Выберите папку с cookie файлами**
   - Нажмите кнопку "🔎 Выбрать папку"
   - Выберите директорию, содержащую JSON-файлы с cookie

2. **Запустите сканирование**
   - Нажмите "🔍 Сканировать"
   - Приложение рекурсивно найдет и проанализирует все JSON-файлы

3. **Просмотрите результаты**
   - Таблица отображает все найденные cookie
   - Статистика показывает сводку по найденным данным

4. **Применяйте фильтры (опционально)**
   - "Только истекшие" - показать только просроченные cookie
   - "Только Secure" - показать только защищённые cookie

5. **Экспортируйте отчет**
   - Нажмите "💾 Экспортировать отчет"
   - Отчёты сохранятся на рабочий стол в папку `CookieAnalyzerReports`

## Структура проекта

```
cookie-analyzer/
├── src/
│   ├── CookieAnalyzer.Core/
│   │   ├── Models/
│   │   │   ├── Cookie.cs              # Модель cookie
│   │   │   ├── CookieAnalysisResult.cs
│   │   │   └── ExportSettings.cs
│   │   └── Services/
│   │       ├── CookieReaderService.cs    # Чтение JSON
│   │       ├── DirectoryScannerService.cs # Сканирование папок
│   │       └── ExportService.cs           # Экспорт в CSV/JSON
│   └── CookieAnalyzer.UI/
│       ├── Views/
│       │   └── MainWindow.xaml          # UI интерфейс
│       ├── ViewModels/
│       │   └── MainWindowViewModel.cs   # MVVM ViewModel
│       ├── App.xaml                     # Avalonia приложение
│       └── Program.cs
└── tests/
    └── CookieAnalyzer.Tests/
        ├── CookieReaderServiceTests.cs
        ├── CookieValidationTests.cs
        ├── DirectoryScannerServiceTests.cs
        └── ExportServiceTests.cs
```

## Пример JSON-файла с cookie

```json
[
  {
    "domain": "example.com",
    "name": "session_id",
    "value": "abc123def456",
    "expirationDate": 1735689600,
    "secure": true,
    "httpOnly": true,
    "path": "/",
    "sameSite": "Strict",
    "session": false
  },
  {
    "domain": "google.com",
    "name": "NID",
    "value": "xyz789",
    "expirationDate": 1735689600,
    "secure": true,
    "httpOnly": false,
    "path": "/",
    "sameSite": "None",
    "session": false
  }
]
```

## Логирование

Логи сохраняются в папке `logs/` с ежедневной ротацией:
- `logs/cookie-analyzer-{дата}.txt`

## Форматы экспорта

### CSV
Получается файл `cookie_report_YYYYMMDD_HHMMSS.csv` с колонками:
- Domain (Домен)
- Name (Имя)
- Value (Значение)
- ExpirationDate (Дата истечения)
- Secure (Флаг Secure)
- HttpOnly (Флаг HttpOnly)
- Path (Путь)
- Session (Сессионная)
- SameSite (SameSite)
- IsExpired (Истекла/Действительна)
- DaysUntilExpiration (Дней до истечения)

### JSON
Получается файл `cookie_report_YYYYMMDD_HHMMSS.json` с полной информацией о cookie.

## Технологический стек

| Компонен�� | Технология |
|-----------|------------|
| UI Framework | Avalonia 11.0.10 |
| MVVM Pattern | ReactiveUI 19.5.41 |
| JSON Processing | System.Text.Json |
| CSV Export | CsvHelper 30.0.0 |
| Logging | Serilog 3.1.1 |
| Testing | xUnit 2.6.6, Moq 4.20.70, FluentAssertions 6.12.0 |
| Language | C# 12 (.NET 8) |

## Примеры использования

### Запуск тестов с подробным выводом

```bash
dotnet test --verbosity detailed
```

### Сборка в режиме Release

```bash
dotnet build --configuration Release
```

### Публикация приложения

```bash
dotnet publish -c Release -o ./publish
```

## Известные ограничения

- ❌ Не поддерживает Netscape cookie jar формат
- ❌ Не работает с зашифрованными cookie файлами
- ❌ Требует правильный JSON формат в исходных файлах

## Лицензия

MIT License

## Разработчик

📧 dmitrijchis271-lab

## Версия

**v1.0.0** - Первый релиз

## Решение проблем

### Приложение не запускается

1. Убедитесь, что установлена .NET 8 SDK:
   ```bash
   dotnet --version
   ```

2. Удалите кеш и пересоберите:
   ```bash
   dotnet clean
   dotnet restore
   dotnet run --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
   ```

### Тесты не проходят

```bash
dotnet test --no-build --verbosity detailed
```

### Cookie не читаются из файла

- Убедитесь, что файл содержит валидный JSON
- Проверьте, что поля "domain" и "name" присутствуют
- Посмотрите логи в папке `logs/`

---

**Спасибо за использование Анализатора Cookie!** 🍪
