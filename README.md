# DesignDemolish

Design Demolish is a in-progress voxel engine that I'm working on for Unity. The goal is to create a highly scalable and moddable game.

## Features

### Terrain Generation 

Generate realistic terrain using the [FastNoise Library.](https://github.com/Auburn/FastNoise_CSharp) Supports tree, ore and foliage generation as well.

### Floodfill Lighting 

An RGB light propagation system based off of Seed of Andromeda's great articles. Supports Sunlight & Colored Blocklight. Integrated smooth lighting is also included.

### A* Pathfinding 

A* pathfinding lets your AI walk around and avoid obstacles. Request paths via the Pathfinding Manager.  

### JSON Block and Item Registry

Define your own blocks and items from within a JSON file and the game will load them automatically.

### Minecraft-Esque Inventory

Shortcuts include: Stack-Split, Combining Item Stacks and Shift-Holding.

### Dynamic Texture Atlas Packer 

Automatically packs textures into an atlas and applies the atlas to the world without having to manually make one. 

### Multithreading

Chunk Generation, Chunk Mesh Generation and the Lighting Engine are all run on a separate thread so the main render thread does not stutter.

### Saving and Loading

Allows you to save and load your progress. Chunks which are in the same region are stored in the same region file. (i.e. r.0.0.sav for chunks [0, 0] to [31, 31])

### Crafting System

Coming soon.™


## Sources

#### A* Pathfinding 

[Amit’s A* Pages](http://theory.stanford.edu/~amitp/GameProgramming/)

[Sebastian Lague's Great Tutorial Series on A*](https://www.youtube.com/watch?v=-L-WgKMFuhE&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW)

#### Other 

[Sam Hogan - Minecraft in 24 Hours](https://github.com/samhogan/Minecraft-Unity3D)

[Seed of Andromeda Fast Flood-Fill Lighting - Part 1](https://www.seedofandromeda.com/blogs/29-fast-flood-fill-lighting-in-a-blocky-voxel-game-pt-1)

[Seed of Andromeda Fast Flood-Fill Lighting - Part 2](https://www.seedofandromeda.com/blogs/30-fast-flood-fill-lighting-in-a-blocky-voxel-game-pt-2)

[Procedural Mesh Tutorial Series](https://www.youtube.com/watch?v=ucuOVL7c5Hw&list=PL5KbKbJ6Gf9-d303Lk8TGKCW-t5JsBdtB)


## Credits 

Using block, item and inventory textures from:

**Good Vibes** Texture Pack by Acaitart Licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)