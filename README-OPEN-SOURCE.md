# AntiAway for Windows — open-source reference

This project is an independent Windows implementation inspired by the
MIT-licensed [`sleekhost/stay-active`](https://github.com/sleekhost/stay-active)
utility and by the separate AntiAway macOS application.

It does not copy the reference Bash implementation. The Windows app uses C#,
WinUI 3, `SendInput`, `SetThreadExecutionState`, and native system-tray APIs.
It never invokes `cliclick`, Homebrew, shell daemons, or macOS frameworks.

Before publishing this repository publicly, choose and add a license for the
AntiAway codebase. The upstream MIT license does not automatically license this
independent implementation. Do not commit signing certificates or private keys.

