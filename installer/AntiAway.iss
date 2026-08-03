#ifndef MyAppVersion
  #define MyAppVersion "0.1.0"
#endif

#define MyAppName "AntiAway"
#define MyAppExeName "AntiAway.exe"

[Setup]
AppId={{E2E2B81A-4D8B-4A79-9C35-7F96D961C58A}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=AntiAway
DefaultDirName={localappdata}\Programs\AntiAway
DefaultGroupName=AntiAway
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
OutputDir=..\artifacts\installer
OutputBaseFilename=AntiAway-{#MyAppVersion}-Setup
SetupIconFile=..\src\AntiAway\Assets\AntiAway.ico
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
AppMutex=Local\AntiAway.Desktop.SingleInstance
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\{#MyAppExeName}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "..\artifacts\publish\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\AntiAway"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueName: "AntiAway"; Flags: uninsdeletevalue

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch AntiAway"; Flags: nowait postinstall skipifsilent

