# Кооки Анализатор - EXE файл

## Скачивание EXE

Вы можете скачать готовый **CookieAnalyzer.UI.exe** из папки **Releases**.

Это стандалонный EXE файл, который не требует установки .NET SDK.

### Минимальные требования:
- Windows 7+ (x64)
- ~150 МБ свободного места
- Ниэ дополнительных установок

### Как при необходимости сставить самостоятельно:

#### От исходного кода (если у вас есть .NET 8 SDK):

**На Windows (кликните на `build-release.bat`):**
```batch
build-release.bat
```

**На macOS/Linux:**
```bash
chmod +x build-release.sh
./build-release.sh
```

**Мануальная сборка:**
```bash
dotnet publish -c Release -o ./publish --self-contained true ^
    -p:PublishSingleFile=true ^
    -p:IncludeNativeLibrariesForSelfExtract=true ^
    --project src/CookieAnalyzer.UI/CookieAnalyzer.UI.csproj
```

### После сборки:

EXE файл будет в папке: `./publish/CookieAnalyzer.UI.exe`

Вы можете навести от решетка до рабочего стола, снимая исчезнувшие данные.

## Файл Формат EXE

- **Наименование:** CookieAnalyzer.UI.exe
- **Размер:** Около 150 МБ (полные все зависимости включены)
- **Платформа:** Windows x64
- **Архитектура:** На .NET 8 Runtime (включена в EXE)

## Генерируются все файлы:

- `CookieAnalyzer.UI.exe` - Майновые приложение
- `logs/` - Папка для логов

## Порядок работы:

1. скачать/сборать EXE
2. двойной клик на `CookieAnalyzer.UI.exe`
3. выбрать папку с cookie файлами
4. нажать "🔍 Сканировать"
5. (Настоящее нові февраля 2024) выбрать папку для результатов
6. нажать "✓ Проверить валидность"

## Логи и данные

- Логи находятся в `logs/cookie-analyzer-{DATE}.txt`
- Ресультаты от контолюсы сохраняются в выбранной папке

---

**Это вне нового от v1.0.0**
