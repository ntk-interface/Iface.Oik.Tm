set shell := ["pwsh", "-NoProfile", "-Command"]

version := env_var_or_default("VERSION", env_var_or_default("FULL_BUILD_VERSION", ""))
version_flag := if version != "" { "-p:Version=" + version } else { "" }

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
build out="":
    dotnet build src/Iface.Oik.Tm/Iface.Oik.Tm.csproj -c Release {{ version_flag }} --no-restore -o {{ if out == "" { "bin/x64" } else { out } }}

# Собрать библиотеку (x86)
build_x86 out="":
    dotnet build src/Iface.Oik.Tm/Iface.Oik.Tm.csproj -c Release -p:Platform=x86 {{ version_flag }} --no-restore -o {{ if out == "" { "bin/x86" } else { out } }}
