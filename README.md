![screenshot1](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot1.jpg?token=AFJ43CBVC35EXFDRAGAOXSDB24YJC)

### Design Demolish is a work-in-progress voxel game engine that I'm working on for Unity. The goal is to create a highly scalable and moddable game.

## Features

### Terrain Generation 

Generate realistic terrain using the [FastNoise Library.](https://github.com/Auburn/FastNoise_CSharp) Supports tree, ore and foliage generation as well.

### Floodfill Lighting 

An RGB light propagation system based off of Seed of Andromeda's great articles. Supports Sunlight & Colored Blocklight. Integrated smooth lighting is also included.

![lighting](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot2.png?token=AFJ43CDRGVIJR3WILUTSTTDB24YIC)

### A* Pathfinding 

A* pathfinding lets your AI walk around and avoid obstacles. Request paths via the Pathfinding Manager.  

![pathfinding](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot3.png?token=AFJ43CDUGKEKKKW3J5XILIDB24YKI)

### JSON Block and Item Registry

Define your own blocks and items from within a JSON file and the game will load them automatically.

![json](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot4.png?token=AFJ43CHW7S5CQCAKOVHNIY3B24YPC)

### Minecraft-Esque Inventory

Shortcuts include: Stack-Split, Combining Item Stacks and Shift-Holding.

![inventory](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot5.png?token=AFJ43CBLBLW7GDM75HWL2VTB24YRO)

### Dynamic Texture Atlas Packer 

Automatically packs textures into an atlas and applies the atlas to the world without having to manually make one. 

![atlas](https://github.com/LostPizzaMan/DesignDemolish-Public/blob/main/Images/Screenshot7.png)

### Multithreading

Chunk Generation, Chunk Mesh Generation and the Lighting Engine are all run on a separate thread so the main render thread does not stutter.

### Saving and Loading

Allows you to save and load your progress. Chunks which are in the same region are stored in the same region file. (i.e. r.0.0.sav for chunks [0, 0] to [31, 31])

![saving](https://raw.githubusercontent.com/LostPizzaMan/DesignDemolish-Public/main/Images/Screenshot6.png?token=AFJ43CHGIRWD5GMOWK7APUTB24YRQ)

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

**Good Vibes** by Acaitart Licensed under [CC BY 4.0](https://creativecommons.org/licenses/by/4.0/)
