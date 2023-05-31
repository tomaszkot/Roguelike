# Roguelike
Roguelike is a library created as a fork of the one used for [Once upon a Dungeon](https://store.steampowered.com/app/772090/Once_upon_a_Dungeon/) game.
It's used as a base layer for upcoming sequel: [Once upon a Dungeon II](https://kotogames.itch.io/once-upon-a-dungeon-ii) 

Features:
- generates a set of dungeons with a desired number of rooms (nodes)
- generates doors allowing to traverse the dungeon 
- nodes auto-revealing mechanism when door are opened
- nodes persistency
- loot generation\collection 
- abilities system
- quests system
- trading system
- written in .net core 3.1

There are two clients:
- a basic console renderer is provided in RoguelikeConsoleRunner subdirectory
Example game's view:
[Sample Dungeon](RoguelikeConsoleRunner/samples/Roguelike.png)

- a Godot renderer (under dev)
Example game's view:
TODO


## Configuration:
GenerationInfo class contains fields controlling generation process.
