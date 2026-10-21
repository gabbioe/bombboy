# What is Bombboy
This is a "Bomberman" inspired game for the KTU module programme T120B516 "Object-Oriented Programme Design". This project is here to learn design patterns. Multiple players on different devices can connect using LAN.

# How to play
## Initializing Bombboy
Install the game files from Github, and launch "Program.cs" in the "backend" folder. Ignore certificate warnings and allow them all. Then play on the launched site.

## Connecting additional players to a session
One game session can support 1 to 4 players. After Bombboy is launched, players on the same network as the host can connect using the same localhost link.

## Game controls
Players move around with the arrow or wasd keys, and can place bombs with the space key, or use picked up items with e or shift+space keys.

## How to start a match
The host (first joined player) can choose the amount of rounds played and the duration of a round (default is 5 rounds and 180 seconds). There are 2 types of maps (normal or ice), and players vote on the map they want. If there is a tie, the map is chosen randomly. All players choose to mark themselves as ready. If all are ready, the game starts automatically, but the host can force start a game once half are ready.

# How to win
Remain the last player alive by avoiding blast radius of bombs, weapons or stepping into water tiles, and exploding all other players.

# Map items
## Tiles

There are 4 kinds of tiles:

- Unbreakable tile

- Breakable tile

- Open tile

- Water tile (if stepped on, player will drown)

The appearance of the first 3 changes depending on the map type, but the logic remains the same.

## Weapons

There are 3 kinds of weapons: close range, throwable and long range. Appearance and function depend on the map.

## Power-ups

There are 4 kinds of power-ups: shield, extra life, and 2 status (extra bomb or longer bomb range). All but shields spawn in the ice map, and all spawn in the normal map.

## Bomb strategies
There are 5 different bomb explosion strategies. Each player can choose which one they want, and the same strategy can be used across multiple players.

# Notice
This game is in early development, and still has a lot of bugs to fix and features to implement. This is not the final version.
