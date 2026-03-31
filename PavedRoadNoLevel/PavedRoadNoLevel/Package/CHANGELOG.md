### 1.0.8
- Smoothing is now also removed from the Cultivate ground

### 1.0.7
- Updated to Call to Arms update, increased Jotunn dependency to 2.26.1

### 1.0.6
- Updated to Bog Witch update

### 1.0.5
- Changed Network compatibility to "ClientMustHaveMod". Meaning installation and version checks are only executed when the server enforces the mod. This will allow users to use this mod as a clientside mod on servers without it installed the server. Only caveat is that when the server has Jotunn installed but not this mod you won't be able to change the config unless you are an admin!

### 1.0.4
- Fixed ServerSync events

### 1.0.3
- Reworked mod, no longer uses custom prefabs but alters the original prefab directly (used to remove the original and add my custom prefab)
- Enabled client/server requiring the mod and same version
- Added Filewatcher to watch for config changes & tested ServerSync, config changes should synchronize and update in game

### 1.0.2
- Fixed internal name to be unique from JotunnModStub causing issues with my other mods

### 1.0.1
- Fixed some mistakes in the readme

### 1.0.0
- First release
