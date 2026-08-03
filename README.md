# AntiAway para Windows

AntiAway es una utilidad nativa para la bandeja del sistema de Windows. Envía
una señal local de movimiento del mouse, sin desplazar el puntero, en el
intervalo seleccionado por el usuario.

La aplicación funciona completamente en la computadora. No hace clic, no
escribe, no registra teclas, no toma capturas de pantalla, no usa la red y no
recopila analíticas.

## Funciones incluidas

- Interfaz nativa construida con C#, .NET 10 y WinUI 3.
- Panel oscuro Acrylic inspirado en la versión de AntiAway para macOS.
- Icono en la bandeja del sistema, sin mantener un botón en la barra de tareas.
- Señal de actividad mediante la API de Windows `SendInput`.
- Prevención opcional de suspensión mediante `SetThreadExecutionState`.
- Intervalos de 30 segundos, 1 minuto, 2 minutos y 4 minutos.
- Estado y preferencias guardados en
  `%LOCALAPPDATA%\AntiAway\settings.json`.
- Inicio automático al entrar a Windows.
- Pantalla de bienvenida, ventana de ajustes y manejo de errores.
- Una sola instancia de AntiAway por sesión de Windows.
- Publicación autocontenida para x64 y ARM64, instalador `.exe` y compilación
  automática en GitHub Actions.

## Cómo instalar y usar AntiAway

### 1. Instalar

Para una instalación normal, ejecuta el archivo generado con un nombre como:

```text
AntiAway-0.1.0-Setup.exe
```

El instalador guarda AntiAway para el usuario actual en:

```text
%LOCALAPPDATA%\Programs\AntiAway
```

No necesita permisos de administrador. Mientras el instalador no esté firmado,
Microsoft Defender SmartScreen puede mostrar una advertencia antes de abrirlo.

### 2. Primer inicio

1. Abre **AntiAway** desde el menú Inicio.
2. Lee la pantalla de bienvenida y selecciona **Continue**.
3. Busca el icono de AntiAway en la bandeja del sistema. Si no aparece junto al
   reloj, abre el menú de iconos ocultos con la flecha `^`.
4. Haz clic en el icono para mostrar u ocultar el panel.

Windows no solicita permisos de accesibilidad para esta implementación y la
aplicación se ejecuta con los permisos normales del usuario.

### 3. Mantener la actividad

1. Abre el panel desde la bandeja.
2. Activa **Stay active**.
3. Elige **Activity interval**:

   - `30 sec`: señal cada 30 segundos.
   - `1 min`: señal cada minuto; valor recomendado inicialmente.
   - `2 min`: señal cada 2 minutos.
   - `4 min`: menor consumo, pero puede ser demasiado lento para algunas
     aplicaciones de presencia.

El estado cambia a **Staying active** y muestra cuándo se envió la última
señal. Puedes ocultar el panel: AntiAway continúa funcionando desde la bandeja.

### 4. Iniciar con Windows

Activa **Launch at login** desde el panel o desde Settings. En el siguiente
inicio de sesión, AntiAway arrancará oculto en la bandeja y recuperará el último
estado guardado.

Para desactivarlo, apaga la misma opción dentro de AntiAway. La configuración
usa la entrada `AntiAway` del usuario actual en:

```text
HKCU\Software\Microsoft\Windows\CurrentVersion\Run
```

### 5. Evitar la suspensión

Abre **Settings** y activa **Prevent system sleep while active**. Esta opción:

- Solo se aplica mientras **Stay active** está encendido.
- Evita la suspensión automática del sistema.
- No obliga a que la pantalla permanezca encendida.
- Se libera inmediatamente al desactivar AntiAway o cerrar la aplicación.

### 6. Ocultar o cerrar

- Cerrar u ocultar el panel no detiene AntiAway.
- Hacer clic en el icono de bandeja vuelve a mostrar el panel.
- Para finalizar el proceso y retirar el icono, selecciona **Quit**.

## Requisitos para desarrollar

- Windows 10 versión 1809 o posterior.
- Windows 11 recomendado para obtener la apariencia Mica/Acrylic completa.
- .NET 10 SDK.
- Visual Studio 2026 con el workload **WinUI application development**, o las
  herramientas de compilación equivalentes.
- Git, si se clonará o publicará el repositorio.

El proyecto usa
[`Microsoft.WindowsAppSDK` 2.3.1](https://www.nuget.org/packages/Microsoft.WindowsAppSDK/2.3.1)
y Windows SDK 26100. La compilación completa de WinUI debe ejecutarse en
Windows; el código puede editarse desde macOS, pero el compilador XAML de WinUI
es exclusivo de Windows.

Comprueba las herramientas desde PowerShell:

```powershell
dotnet --version
git --version
```

`dotnet --version` debe mostrar la versión 10 o una versión posterior
compatible.

## Obtener el proyecto

Si el repositorio ya está publicado:

```powershell
git clone YOUR_REPOSITORY_URL anti-away-windows
cd anti-away-windows
```

Si copiaste la carpeta manualmente, abre PowerShell dentro de la carpeta que
contiene `AntiAway.Windows.sln`.

## Restaurar, compilar y ejecutar

### Restaurar dependencias

Se necesita conexión a Internet la primera vez:

```powershell
dotnet restore src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64
```

### Compilar en modo Debug

```powershell
dotnet build src\AntiAway\AntiAway.csproj `
  --configuration Debug `
  --no-restore `
  -p:Platform=x64 `
  -r win-x64
```

### Ejecutar desde el código fuente

```powershell
dotnet run `
  --project src\AntiAway\AntiAway.csproj `
  --configuration Debug `
  -p:Platform=x64 `
  -r win-x64
```

También puedes abrir `AntiAway.Windows.sln` en Visual Studio, seleccionar
**x64** y presionar `F5`.

### Limpiar archivos compilados

```powershell
dotnet clean src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64
```

## Comandos de desarrollo frecuentes

Ejecuta los comandos desde la raíz del repositorio.

| Objetivo | Comando |
|---|---|
| Revisar cambios | `git status --short` |
| Restaurar paquetes | `dotnet restore src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64` |
| Compilar Debug | `dotnet build src\AntiAway\AntiAway.csproj -c Debug -p:Platform=x64 -r win-x64` |
| Compilar Release | `dotnet build src\AntiAway\AntiAway.csproj -c Release -p:Platform=x64 -r win-x64` |
| Ejecutar | `dotnet run --project src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64` |
| Limpiar | `dotnet clean src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64` |
| Publicar x64 | `.\scripts\Publish.ps1 -RuntimeIdentifier win-x64 -Configuration Release` |
| Publicar ARM64 | `.\scripts\Publish.ps1 -RuntimeIdentifier win-arm64 -Configuration Release` |
| Crear instalador | `.\scripts\Build-Installer.ps1 -Version 0.1.0` |

## Crear una versión autocontenida

Una versión autocontenida incluye .NET y Windows App SDK. La computadora de
destino no necesita instalar esos componentes por separado.

### Windows x64

```powershell
.\scripts\Publish.ps1 -RuntimeIdentifier win-x64 -Configuration Release
```

Resultado:

```text
artifacts\publish\win-x64\AntiAway.exe
```

Debes conservar juntos todos los archivos de
`artifacts\publish\win-x64`; no copies únicamente `AntiAway.exe`.

### Windows ARM64

```powershell
.\scripts\Publish.ps1 -RuntimeIdentifier win-arm64 -Configuration Release
```

Resultado:

```text
artifacts\publish\win-arm64\AntiAway.exe
```

### Si PowerShell bloquea el script

Puedes permitirlo únicamente para el comando actual sin cambiar la política
permanente del sistema:

```powershell
powershell -ExecutionPolicy Bypass -File .\scripts\Publish.ps1 `
  -RuntimeIdentifier win-x64 `
  -Configuration Release
```

## Crear el instalador `.exe`

Instala Inno Setup 6 una sola vez:

```powershell
winget install JRSoftware.InnoSetup
```

Cierra y vuelve a abrir PowerShell si el instalador no se detecta. Después
ejecuta:

```powershell
.\scripts\Build-Installer.ps1 -Version 0.1.0
```

Resultado:

```text
artifacts\installer\AntiAway-0.1.0-Setup.exe
```

El script publica primero la versión x64 y luego la empaqueta como instalación
por usuario. Para distribuirla públicamente, firma el instalador y los binarios
con un certificado de firma de código confiable.

## Validar antes de publicar

Como mínimo:

```powershell
dotnet restore src\AntiAway\AntiAway.csproj -p:Platform=x64 -r win-x64
dotnet build src\AntiAway\AntiAway.csproj -c Release --no-restore -p:Platform=x64 -r win-x64
.\scripts\Publish.ps1 -RuntimeIdentifier win-x64 -Configuration Release
```

Después sigue la [lista completa de pruebas para Windows](docs/TESTING.md). En
particular, comprueba la bandeja, todos los intervalos, la persistencia, el
inicio automático, suspensión, actualización y desinstalación.

## Publicar cambios en Git

Revisa primero qué se modificó:

```powershell
git status --short
git diff
```

Cuando la compilación y las pruebas hayan pasado:

```powershell
git add .
git commit -m "Describe el cambio"
git push origin main
```

El workflow `.github/workflows/windows-build.yml` compila y publica un artefacto
`AntiAway-win-x64` en GitHub Actions después de cada push a `main`.

## Diagnóstico

### El icono no aparece

Comprueba primero el menú de iconos ocultos de Windows. Después verifica si el
proceso está ejecutándose:

```powershell
Get-Process AntiAway -ErrorAction SilentlyContinue
```

Si el proceso ya existe, vuelve a la bandeja en lugar de abrir otra copia;
AntiAway permite una sola instancia.

### Revisar el inicio automático

```powershell
Get-ItemProperty `
  HKCU:\Software\Microsoft\Windows\CurrentVersion\Run `
  -Name AntiAway `
  -ErrorAction SilentlyContinue
```

Usa el interruptor **Launch at login** para crear o retirar esa entrada.

### Abrir la carpeta de preferencias

```powershell
explorer "$env:LOCALAPPDATA\AntiAway"
```

Para restablecer las preferencias de forma recuperable, cierra AntiAway y
renombra el archivo:

```powershell
Rename-Item `
  "$env:LOCALAPPDATA\AntiAway\settings.json" `
  "settings.backup.json"
```

Al abrir AntiAway nuevamente se crearán los valores predeterminados. Puedes
restaurar el archivo de respaldo si lo necesitas.

### La presencia no cambia

Confirma que el estado diga **Staying active** y prueba primero el intervalo de
1 minuto. Microsoft Teams y otras aplicaciones controlan sus propias reglas de
presencia y pueden ignorar eventos sintéticos, llamadas desde procesos con un
nivel de privilegios distinto, una sesión bloqueada o políticas de la empresa.

## Arquitectura

- `AppViewModel` controla el estado persistido, los temporizadores, la
  prevención de suspensión, los errores y el texto de estado.
- `ActivityService` envía un evento `MOUSEEVENTF_MOVE` con desplazamiento X/Y
  igual a cero.
- `PowerService` solicita `ES_SYSTEM_REQUIRED` solamente cuando AntiAway está
  activo y la opción correspondiente está habilitada.
- `StartupService` administra la entrada Run del usuario y utiliza `--startup`
  para iniciar con el panel oculto.
- `TrayIconService` usa `Shell_NotifyIcon` y restaura el icono si Windows
  Explorer se reinicia.
- `SettingsService` guarda un archivo JSON de forma atómica y vuelve a los
  valores predeterminados si el archivo no es válido.

## Limitaciones y uso responsable

- Microsoft Teams y otras aplicaciones pueden ignorar la actividad sintética o
  cambiar sus reglas en cualquier versión.
- `SendInput` está sujeto a User Interface Privilege Isolation. Un proceso
  normal no puede inyectar eventos en otro ejecutado como administrador.
- AntiAway no mantiene activa una sesión bloqueada ni evita políticas de la
  empresa, administración del dispositivo u otros cambios manuales de estado.
- Utiliza actividad sintética únicamente cuando esté permitida por la política
  de tu lugar de trabajo.
- Un instalador sin firma puede activar Microsoft Defender SmartScreen.

