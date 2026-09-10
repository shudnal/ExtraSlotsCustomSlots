# 1.0.22
* updated for Valheim 1.0.7 and Extra Slots 1.2.1 (now required)
* fixed inventory loading hooks for both game load overloads and excluded temporary inventories from backpack and quiver processing
* fixed custom equipment bypassing rejected equip attempts; native refusal order and shared item data are preserved during nested equips
* synchronized the variant and quality of custom equipment visuals, including changes that keep the same item prefab
* preserved unchanged slot registrations during configuration updates and ignored repeated IDs in the slot order
* matched user-defined equipment to its actual registered slot after inventory validation and refreshed equipment when slot rules change
* fixed overlapping user-defined slots choosing inconsistent destinations or replacing an item still awaiting physical placement
* fixed the Rusty Bags quiver option retaining an outdated item filter

# 1.0.21
* fixed compatibility with both legacy Epic Loot versions and Epic Loot 0.13.0 or later for magic effects in custom slots and Shardstones on custom backpacks

# 1.0.20
* migrated configuration synchronization from ServerSync to ConditionalConfigSync
* server administrators can override the ownership of policy-controlled settings through ConditionalConfigSync policy

# 1.0.19
* added an option to hide equipped Adventure Backpacks and Judes Equipment backpacks on the player model

# 1.0.18
* added Quiver slot for RustyBags

# 1.0.17
* fixed occasional bug with JudesEquipment config sync

# 1.0.16
* Vikings Summoner grimoire slot fixed once again

# 1.0.15
* Vikings Summoner grimoire slot fixed

# 1.0.14
* Vikings Summoner grimoire slot

# 1.0.13
* fixed rare issue when item loses its durability and breaks in custom slot

# 1.0.12
* items in user defined custom slots are visible on player now

# 1.0.11
* 8 used defined custom slots. You can create your own custom slots for any item(s). Use ExtraSlots translation files to add localized slot names.

# 1.0.10
* RustyBags slot

# 1.0.9
* patch 0.220.3
* ServerSync updated

# 1.0.8
* CircletExtended slot will only be available after circlet upgrade to lvl2

# 1.0.7
* changed the logic of checking slots activity. If only one of the values ​​(global key or discovered item) is specified - checking for the unfilled one will not be performed

# 1.0.6
* fixed rare occasional warning when AdventureBackpack is installed with slot disabled

# 1.0.5
* JudesEquipment backpacks slot

# 1.0.4
* fixed AdventureBackpack slot making some other items "equippable"

# 1.0.3
* fixed BowsBeforeHoes quiver unequipping cape

# 1.0.2
* AdventureBackpack double equip issue fix

# 1.0.1
* AdventureBackpack opening fixed
* AdventureBackpack compatibility with EpicLoot

# 1.0.0
 * Initial Release
