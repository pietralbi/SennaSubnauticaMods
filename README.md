# Slot Extender

Slot Extender is a Subnautica BepInEx plugin that adds configurable extra vehicle upgrade slots for the Seamoth and Exosuit.

## Build

This workspace expects Subnautica, BepInEx, and Nautilus to be installed locally. By default it uses:

```text
C:\Program Files (x86)\Steam\steamapps\common\Subnautica
```

To build to a local artifact folder:

```powershell
dotnet msbuild .\SennaSubnauticaMods.sln /p:Configuration=Debug /p:OutputPath=.\artifacts\SlotExtender\Debug\
```

To use a different game install, set either the `GameDir` MSBuild property or the `SUBNAUTICA_GAME_DIR` environment variable.
