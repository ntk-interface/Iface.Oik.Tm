set shell := ["pwsh", "-NoProfile", "-Command"]

# Показать список команд
default:
    @just --list

# Восстановить NuGet-зависимости
restore:
    dotnet restore Iface.Oik.Tm.sln

# Восстановить локальные инструменты (csharpier и пр.)
tool_restore:
    dotnet tool restore

# Отформатировать код
format: tool_restore
    dotnet csharpier format .

# Проверить форматирование кода
check_format: tool_restore
    dotnet csharpier check .

# Прогнать тесты
test: restore
    dotnet test Iface.Oik.Tm.sln -c Release --no-restore

# Проверить готовность к публикации (перед Merge Request)
verify: restore check_format test

# Собрать библиотеку (x64)
build out="bin/x64" version="": (_build "" out version)

# Собрать библиотеку (x86)
build_x86 out="bin/x86" version="": (_build "x86" out version)

# Собрать библиотеку под указанную платформу и версию (аргументы: out, version)
_build platform out version:
    dotnet build src/Iface.Oik.Tm/Iface.Oik.Tm.csproj -c Release{{ if platform != "" { " -p:Platform=" + platform } else { "" } }}{{ if version != "" { " -p:Version=" + version } else { "" } }} --no-restore -o {{ out }}
