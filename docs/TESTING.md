# Windows test checklist

Run this checklist on a physical Windows 10 or Windows 11 computer before a
release. Test as a normal user, not from an elevated administrator session.

## Build and first launch

1. Install Visual Studio 2026 with the **WinUI application development**
   workload and the .NET 10 SDK.
2. Open `AntiAway.Windows.sln`, select `x64`, and run the `AntiAway` project.
3. Confirm the welcome screen appears only on first launch.
4. Confirm the AntiAway icon appears in the system tray and no taskbar button
   remains after the panel is hidden.

## Core behavior

1. Enable **Stay active** and confirm the status changes immediately.
2. Leave the pointer over a precise visual target and confirm it never moves.
3. Verify each interval: 30 seconds, 1 minute, 2 minutes, and 4 minutes.
4. Confirm **Prevent system sleep while active** prevents automatic sleep, but
   does not force the display to remain on.
5. Disable AntiAway and confirm the status and tray tooltip update.
6. Lock and unlock Windows; confirm the app remains stable. AntiAway does not
   claim to keep a locked session active.

## Startup and persistence

1. Enable **Launch at login**.
2. Sign out and sign back in.
3. Confirm AntiAway starts silently in the tray.
4. Confirm the selected interval and enabled state survive a restart.
5. Disable **Launch at login** and verify the `AntiAway` entry disappears from
   Task Manager → Startup apps after refreshing.

## Tray and windows

1. Click the tray icon repeatedly; the panel should toggle cleanly.
2. Open Settings, modify every option, close it, and reopen it.
3. Close the panel and confirm AntiAway keeps running.
4. Choose **Quit** and confirm the tray icon disappears and the process exits.
5. Start AntiAway twice and confirm only one process remains.

## Presence-app validation

Test Microsoft Teams and any other target presence app separately. Synthetic
input handling is controlled by those products and can change. Record the app
version, Windows version, interval, lock/sleep state, and observed result.

## Installer

1. Run `scripts\Build-Installer.ps1`.
2. Install the generated setup as a standard user.
3. Launch, upgrade over the installed version, and uninstall.
4. Confirm uninstall removes the app and its startup registry entry.
5. Confirm user settings under `%LOCALAPPDATA%\AntiAway` are intentionally
   retained so upgrades do not erase preferences.

