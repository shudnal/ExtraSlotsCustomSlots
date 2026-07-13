# Extra Slots Custom Slots

Custom slots for various mod that supports custom items.

This mod depends on [ExtraSlots](https://thunderstore.io/c/valheim/p/shudnal/ExtraSlots/) mod to work.

This mod can add custom slot for:
* [Backpacks](https://thunderstore.io/c/valheim/p/Smoothbrain/Backpacks/) (Backpack slot)
* [AdventureBackpacks](https://thunderstore.io/c/valheim/p/Vapok/AdventureBackpacks/) (with option to equip cape and backpack at the same time)
* [Jewelcrafting](https://thunderstore.io/c/valheim/p/Smoothbrain/Jewelcrafting/) (Neck and Ring slots)
* [MagicPlugin](https://thunderstore.io/c/valheim/p/blacks7ar/MagicPlugin/) (Tome and Earring slots)
* [BowsBeforeHoes](https://thunderstore.io/c/valheim/p/Azumatt/BowsBeforeHoes/) (Quiver slot)
* [CircletExtended](https://thunderstore.io/c/valheim/p/shudnal/CircletExtended/) (Circlet slot)
* [HipLantern](https://thunderstore.io/c/valheim/p/shudnal/HipLantern/) (Lantern slot)
* [Judes Equipment](https://thunderstore.io/c/valheim/p/GoldenJude/Judes_Equipment/) (Backpack slot)
* [Rusty Bags](https://thunderstore.io/c/valheim/p/RustyMods/RustyBags/) (Bag slot)
* up to 8 user defined custom slots where you can set any item(s) to be placed in dedicated slot

You can change slots order in the game as you wish.

You can rename and enable/disable slots in the game. Restart is recommended only after toggling AdventureBackpacks and JudesEquipment slots.

"Slot progression" feature from ExtraSlots is supported and default settings is obtaining items related to particular mod. It means you will have Backpack slot the moment you first obtain you backpack.

You can use ExtraSlots translation files to add your own strings for custom slot names.

## Conditional Config Sync
* All configuration settings except logging are synchronized from the server by default
* Server administrators can change the synchronization policy for policy-controlled settings in `BepInEx/config/shudnal.ConditionalConfigSync/ConditionalConfigSync.SyncPolicy.cfg`
* Prefix an exact setting or whole-section identifier with `+` to force server control or `-` to make it client-controlled. Exact-setting rules take precedence over whole-section rules
* Slot availability, order, names, progression requirements, accepted item lists, and equipped-item visibility are policy-controlled. `Lock Configuration` always remains server-controlled
* Use shared modpack configs or distribute your config manually if you also want client-controlled settings to be identical for all players initially
* If you install this mod manually, also install [ConditionalConfigSync](https://thunderstore.io/c/valheim/p/shudnal/ConditionalConfigSync/)

## Installation (manual)
extract ExtraSlotsCustomSlots.dll into your BepInEx\Plugins\ folder

## Configurating
The best way to handle configs is [Configuration Manager](https://thunderstore.io/c/valheim/p/shudnal/ConfigurationManager/).

Or [Official BepInEx Configuration Manager](https://valheim.thunderstore.io/package/Azumatt/Official_BepInEx_ConfigurationManager/).

## Mirrors
[Nexus](https://www.nexusmods.com/valheim/mods/2911)

## Donation
[Buy Me a Coffee](https://buymeacoffee.com/shudnal)

## Discord
[Join server](https://discord.gg/e3UtQB8GFK)