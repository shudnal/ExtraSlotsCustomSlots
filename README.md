# Extra Slots Custom Slots

Custom slots for various mod that supports custom items.

This mod depends on [ExtraSlots](https://thunderstore.io/c/valheim/p/shudnal/ExtraSlots/) mod to work.

## Conditional Config Sync
* All configuration settings except logging are synchronized from the server by default
* Server administrators can change the synchronization policy for policy-controlled settings in `BepInEx/config/shudnal.ConditionalConfigSync/ConditionalConfigSync.SyncPolicy.cfg`
* Prefix an exact setting or whole-section identifier with `+` to force server control or `-` to make it client-controlled. Exact-setting rules take precedence over whole-section rules
* Slot availability, order, names, progression requirements, accepted item lists, and equipped-item visibility are policy-controlled. `Lock Configuration` always remains server-controlled
* Use shared modpack configs or distribute your config manually if you also want client-controlled settings to be identical for all players initially
* If you install this mod manually, also install [ConditionalConfigSync](https://thunderstore.io/c/valheim/p/shudnal/ConditionalConfigSync/)

This mod can add custom slot for:
* [Backpacks](https://thunderstore.io/c/valheim/p/Smoothbrain/Backpacks/) (Backpack slot)
* [AdventureBackpacks](https://thunderstore.io/c/valheim/p/Vapok/AdventureBackpacks/) (with option to equip cape and backpack at the same time)
* [Jewelcrafting](https://thunderstore.io/c/valheim/p/Smoothbrain/Jewelcrafting/) (Neck and Ring slots)
* [MagicPlugin](https://thunderstore.io/c/valheim/p/blacks7ar/MagicPlugin/) (Tome and Earring slots)
* [BowsBeforeHoes](https://thunderstore.io/c/valheim/p/Azumatt/BowsBeforeHoes/) (Quiver slot)
* [CircletExtended](https://thunderstore.io/c/valheim/p/shudnal/CircletExtended/) (Circlet slot)
* [HipLantern](https://thunderstore.io/c/valheim/p/shudnal/HipLantern/) (Lantern slot)

You can change slots order in the game as you wish.

You can rename and enable/disable slots in the game. Restart is recommended only after toggling AdventureBackpacks slot.

"Slot progression" feature from ExtraSlots is supported and default settings is obtaining items related to particular mod. It means you will have Backpack slot the moment you first obtain you backpack.

## Installation (manual)
extract ExtraSlotsCustomSlots.dll into your BepInEx\Plugins\ folder

## Configurating
The best way to handle configs is [Configuration Manager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/).

Or [Official BepInEx Configuration Manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).

## Mirrors
[Nexus](https://www.nexusmods.com/valheim/mods/2911)