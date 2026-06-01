# КАК ГОТОВО ГОТОВИТЬ EXE ФАЙЛ

## Метод 1: Автоматическая сборка (ПРОСТО)

### На Windows:

1. Откройте **PowerShell** (Навигатор - правоц на папке репозитория)

2. Выполните команду:

```powershell
# На Windows открыть батник
.\build-release.bat
```

Или двойной клик на **build-release.bat** в файловом менеджере.

3. Ожидайте завершения сборки (около 3-5 минут).

### На macOS/Linux:

```bash
chmod +x build-release.sh
./build-release.sh
```

---

## Метод 2: Мануальная сборка

### Требования:
- **.NET 8 SDK** (скачать https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- **PowerShell** или **Command Prompt** (При наличии Windows)

### Шаги:

1. Откройте **Command Prompt** или **PowerShell**

2. Открыть папку репозитория:

```bash
cd C:\path\to\cookie-analyzer
```

3. Встроить зависимости:

```bash
dotnet restore
```

4. Сборите Release версию:

```bash
dotnet build -c Release
```

5. Опубликуйте как единость:  

**На Windows (Command Prompt):**
```bash
dotnet publish -c Release -o ./publish --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    -p:PublishReadyToRun=true ^
    --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
```

**На Windows (PowerShell):**
```powershell
dotnet publish -c Release -o ./publish --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true `
    --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
```

**На macOS/Linux:**
```bash
dotnet publish -c Release -o ./publish --self-contained true \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishReadyToRun=true \
    --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
```

---

## Ожидаемые результаты:

### После успешной сборки:

```
Папка ./publish содержит:
└── CookieAnalyzer.UI.exe  (← ЭТО с майновый EXE файл)
└── CookieAnalyzer.UI.pdb
└── ... (другие вспомогательные файлы)
```

### Проблемы при сборке?

**Ошибка: "dotnet is not recognized"**
- Убедитесь, что .NET 8 SDK установлен:
  ```bash
  dotnet --version
  ```
- Перезагрузите Command Prompt/PowerShell

**Ошибка сборки**
```bash
# Очистите кеш
 dotnet clean
 
# Переставьте зависимости
 dotnet restore
 
# Снова сборите
 dotnet build -c Release
```

---

## После сборки:

### Расположение:

**готовый EXE с одной папке:**

```
./publish/CookieAnalyzer.UI.exe
```

### Как использовать:

1. Открыть папку `./publish`
2. Двойной клик на **CookieAnalyzer.UI.exe**
3. Приложение автоматически начнет

### Дополнительные опции:

- Можно ознакомить файл к **рабочему столу** для срюдару
- Логи сохраняются в папке `logs/`

---

## Этхарактеристики EXE:

| Параметр | Значение |
|---------|--------|
| **Наименование** | CookieAnalyzer.UI.exe |
| **Размер** | ~150 МБ |
| **Платформа** | Windows 7+ (x64) |
| **Примерное время сборки** | 3-5 минут |
| **ОТ | .NET 8 |
| **Мемория** | 50-150 МБ |
| **Автономность** | Полная (не требует .NET SDK) |

---

## Важно:

⚠ **Это стандалонный EXE** - Он НЕ требует установки .NET и библиотек на компьютере другого завершаемого.

📆 **Логи и данные** сохраняются в запускаемом директориалы, а не на диске.

🔐 **Безопасно** - Тот же источник кодов, так как репозиторий гитхаб.

---

**Вссборка должна работать беспроблемно!** 🙋
