# Chipster8

## A Chip8 emulator in C# using MonoGame. 

![Chipster8 Screenshot 1](/Screenshots/Chipster-1.png?raw=true)
![Chipster8 Screenshot 2](/Screenshots/Chipster-2.png?raw=true)
![Chipster8 Screenshot 3](/Screenshots/Chipster-3.png?raw=true)

## About
Chipster8 was built using MonoGame 3.8 with support for OpenGL and DirectX desktop clients.

It has the following features:
- Increase/decrease emulation speed
- Multiple color schemes
- Pause/Resume

The Chip8 emulator and application are separated such that one could pull out the Chip8 and re-use it in another way.

*Note: Chip8 implementations by nature have a decent amount of flickering due to the way draw calls are implemented, and without writing against the original spec it is an unfortunate side effect of playing Chip8 games.*

## Controls

### Game Controls
| Chip8  | Chipster8 |
| ------------- | ------------- |
| 1 | 1 |
| 2 | 2 |
| 3 | 3 |
| C | 4 |
| 4 | Q |
| 5 | W |
| 6 | E |
| D | R |
| 7 | A |
| 8 | S |
| 9 | D |
| E | F |
| A | Z |
| 0 | X |
| B | C |
| F | V |

### Menu Controls
| Feature  | Key |
| ------------- | ------------- |
| Move Selection | Up/Down |
| Load Selection | Enter |

### Extra Controls
| Feature  | Key |
| ------------- | ------------- |
| Pause/Resume | Space |
| Mute/Unmute | Backspace |
| Dump Memory To Console | F1 |
| Dump Video To Console | F2 |
| Dump Registers To Console | F3 |
| Decrease Speed | F4 |
| Increase Speed | F5 |
| Cycle Color Scheme | F6 |
| Toggle Fullscreen | F11 |
