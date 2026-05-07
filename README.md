# Tomat-Oh No!

A trivia + boss-fight hybrid built in Unity 6. Answer questions correctly to damage the boss; answer wrong and the boss heals while bullets come flying at you. Built for CISC 226 (Game Development).

## Play it

Grab the latest Windows build from the [Releases page](../../releases/latest), unzip, and double-click `My project.exe`.

## Controls

- **Move** — WASD / Arrow keys
- **Select answer** — walk onto the answer tile
- (Update this section once you've confirmed the controls in `Assets/Scripts/PlayerController.cs`)

## How it works

- Trivia questions are loaded each round; the player picks an answer by stepping on a tile.
- Correct answer → boss HP drops.
- Wrong answer → boss HP heals + the bullet spawner activates and fires at the player.
- Win when the boss is defeated, lose when player HP hits zero.

Core scripts live in [Assets/Scripts/](Assets/Scripts/):

| Script | Role |
| --- | --- |
| `GameManager.cs` | Round flow, boss HP, win/lose conditions |
| `PlayerController.cs` | Player movement and HP |
| `BulletSpawner.cs` | Spawns bullets when the player answers wrong |
| `Bullet.cs` | Bullet behavior |
| `AnswerTile.cs` | Floor tile that registers an answer when stepped on |
| `GridRenderer.cs` | Renders the play grid |
| `TriviaQuestion.cs` | Question data model |
| `UILayoutManager.cs` | Adapts UI to screen size |
| `ModeSwitcher.cs` | Toggles between game modes |
| `SplatEffect.cs` | Visual splat / hit feedback |

## Build from source

1. Install [Unity Hub](https://unity.com/download) and Unity Editor **6000.3.9f1** (the version this project was built with — see `ProjectSettings/ProjectVersion.txt`).
2. Clone this repo:
   ```
   git clone https://github.com/davidyang02/Tomat-Oh-No.git
   ```
3. In Unity Hub, **Add project from disk** and pick the cloned folder.
4. Open `Assets/Scenes/SampleScene.unity` and press Play.

## Credits

- Code, design, and game logic: David Yang
- Background music / SFX: third-party assets (see `Assets/Resources/`) — replace this section with proper attribution before sharing widely.

## License

[MIT](LICENSE) — code is free to use, modify, and redistribute. Note that bundled audio/art assets may be under different licenses; check `Assets/Resources/` and the asset source before reusing them.
