# Building Extra Slots Custom Slots

## Compatibility baseline

Version 1.0.22 targets Valheim 1.0.7 and requires Extra Slots 1.2.1 or newer adapted for that game version. ConditionalConfigSync remains a separate dependency.

The source adaptation was based on:

- Game sources: `shudnal/assemblies_combined`, commit `02f009229ca3d045153d8e9b3a516af6cf68c158`.
- Extra Slots: `shudnal/ExtraSlots`, branch `fix/valheim-1.0.7-compatibility`, commit `b86b65e086f93b2af5aa1d14bea8d3e0f6993dde`.

## Local dependencies

Use Visual Studio/MSBuild with the .NET Framework 4.8 targeting pack and C# 8 support. Restore the NuGet package references.

The project retains the existing game and BepInEx reference layout. Check those paths in `ExtraSlotsCustomSlots.csproj` before building. In particular, `../Assemblies/stable/publicized_assemblies` must contain the Valheim 1.0.7 publicized assemblies, and the Unity assemblies must come from the same game installation. `ConditionalConfigSync.dll` is referenced from `../Assemblies/Managers`.

The obsolete `Libs/ExtraSlots.dll` is no longer used or shipped. Supply an already-built compatible Extra Slots DLL using the `ExtraSlotsAssemblyPath` MSBuild property. By default, the project uses a sibling checkout's `../ExtraSlots/bin/Release/ExtraSlots.dll`. No dependency project is built automatically.

From a Visual Studio Developer PowerShell, for example:

```powershell
msbuild .\ExtraSlotsCustomSlots.csproj /restore /t:Build /p:Configuration=Release "/p:ExtraSlotsAssemblyPath=C:\ValheimModding\ExtraSlots\bin\Release\ExtraSlots.dll"
```

The project checks that this file exists and its assembly version is at least 1.2.1.0. Extra Slots is not copied into this mod's output as a private dependency; install it separately in the game.

## Packaging and verification

Review the existing paths and targets in `ILRepack.targets` before invoking a build. The existing post-build pipeline updates the Thunderstore manifest, prepares the package and archive, and may copy the DLL into an r2modman profile. Its local paths are independent of `ExtraSlotsAssemblyPath`.

Plugin, assembly and file versions derive from `ExtraSlotsCustomSlots.pluginVersion`. Keep the checked-in Thunderstore manifest and changelog aligned with that version.

Compilation and in-game checks must be performed in the local Valheim environment. Static source review does not validate third-party mod binaries or multiplayer behavior.

## Inventory integration constraint

`Inventory.m_temoraryInventory` is readonly. The game's `Inventory(bool)` constructor always sets it to true, regardless of the argument. Its load path creates temporary serialization records instead of fully initialized runtime equipment.

Do not use that constructor to stage, resize or recover player items, and do not try to change the flag after loading. Custom inventory-processing hooks must reject temporary inventories. Slot relocation and deferred recovery belong to the Extra Slots API and its real player-inventory operations.
