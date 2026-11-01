# Godot Projects

Small projects for learning game development with Godot and C#. Each game has its own Godot project; the collection progresses from engine fundamentals to complete, deliberately small games.

The companion repository, [godot-learning](https://github.com/janno-p/godot-learning), contains the coaching agreement, learning plans, project decisions, and progress notes. This repository holds the actual Godot source projects.

## Projects

| Project | Description | Status |
| --- | --- | --- |
| [Basic Structure](basic-structure/) | A button-driven counter exploring scenes, nodes, signals, C# scripts, and responsive UI layout. | Foundations sandbox complete |
| [Lights Out](lights-out/) | A playable 3×3 puzzle with state-driven visuals, victory detection, and restart. | Version one complete; Windows export tested by the author |

### Lights Out

Click a light to toggle it and its immediate horizontal and vertical neighbors. Turn every light off to win.

- The starting pattern is fixed and solvable.
- ON/OFF labels and colors show each light's state.
- The solved board stays visible, with further moves disabled.
- **Restart** restores the original puzzle at any time.

## Requirements

The development environment is Windows 11 with:

- **Godot 4.7.2 .NET** — use the .NET edition, not the standard editor.
- **.NET 10 SDK** — required to load and compile the C# projects in Godot; install alongside .NET 11.
- **.NET 11 SDK `11.0.100-rc.3.26522.148`** — provides the C# 15 preview language features used by these projects.

Both projects target `net8.0` for desktop and enable C# 15 preview features, including union types. The target framework alone does not describe the SDK requirements: install both SDKs listed above.

Shared C# configuration lives in [`Directory.Build.props`](Directory.Build.props): preview language features, implicit usings, nullable reference checks, and SDK preview-message suppression. Both projects inherit these settings. No `global.json` SDK pin is required for this setup.

The union support types, `IUnion` and `UnionAttribute`, live in [`shared/System.Runtime.CompilerServices/`](shared/System.Runtime.CompilerServices/). `Directory.Build.props` automatically includes them as linked C# source in each project inheriting the shared configuration, including future projects. They compile into each game's own assembly; no separate library is needed. Keep the shared folder and root props file when copying project source.

## Run a project

1. Clone this repository.
2. Open the Godot .NET Project Manager and choose **Import**.
3. Select either [`basic-structure/project.godot`](basic-structure/project.godot) or [`lights-out/project.godot`](lights-out/project.godot).
4. Open the project, build the C# solution, and press **F6** to run the open scene or **F5** to run the project.

There is no shared launcher or root solution; open each project separately.

## Exports

Lights Out includes a Windows Desktop export preset. Export templates matching the Godot editor version are required to create a build.

Generated `dist/` folders are ignored by Git; exported binaries are not included in a clone. Keep these items together when distributing the Windows build:

```text
Lights Out.exe
Lights Out.pck
data_Lights Out_windows_x86_64/
```

The author tested the Windows export and a relocated copy on the same machine; compatibility with a clean PC has not been verified.
