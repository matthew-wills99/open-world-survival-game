# University of Windsor Computer Science Capstone
### Matthew Wills & Yusuf Naebkhil

***

We decided to do an open world survival game, set in a randomly generated world, with building and crafting
The perspective is top-down 2D.

***

### Map Generation

Multiple iterations of map generation scripts came to a map generated using cellular automata.

For the initial random terrain and water tiles, the ones towards the centre of the map are more likely to be land.
The smoothing algorithm passes over the map several times.
Biomes are randomly placed a certain distance from each other and are spread to a random size.
Then environment obstacles, trees, cacti, and ores are placed based on their rarity and the biome they are placed on, they are also placed in clusters of random size.

<a href="http://www.youtube.com/watch?feature=player_embedded&v=b2tfX3FlJeY
" target="_blank"><img src="http://img.youtube.com/vi/b2tfX3FlJeY/0.jpg" 
alt="Map Generation Timelapse Video" width="240" height="180" border="10" /></a>

