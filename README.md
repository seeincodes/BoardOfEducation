# Board of Education

A 2-player collaborative coding logic puzzle for the [Board.fun](https://board.fun) physical-digital console. Built for the SuperBuilders 1-week challenge.

Students (age 6+) work together to place physical game pieces in the correct sequence, learning **sequencing**—the foundational idea that order matters in code. Every piece placement and rotation is logged to CSV for learning analytics.

---

## Quick Setup

### 1. Install Unity

- **Version:** Unity 2021.3 LTS (2021.3.56f2 recommended — security-patched)
- **Download:** [unity.com/download](https://unity.com/download)
- **Modules:** Install **Android Build Support** (for Board sideload)
- **Platform:** Works on Apple Silicon (M1/M2/M3) and Intel Macs

### 2. Open the Project

1. Open **Unity Hub**
2. **Add** → select the `BoardOfEducation` folder (inside this repo)
3. Open with Unity 2021.3

### 3. First-Time Setup in Unity

1. **Wait for packages** — Unity resolves Board SDK, Input System, UI
2. **Import Board Input sample** — Window → Package Manager → fun.board → Samples → Import "Input"
3. **Add scenes to build** — File → Build Settings → Add your game scene(s)
4. **Press Play** — Run in Editor with Board Simulator

See **[BoardOfEducation/UNITY_SETUP_CHECKLIST.md](BoardOfEducation/UNITY_SETUP_CHECKLIST.md)** for the full checklist.

---

## Project Structure

```
SuperBuilders/
├── README.md                 ← You are here
├── BoardOfEducation/         ← Unity project (open this folder in Unity Hub)
│   ├── Assets/
│   │   ├── Scripts/          # Game logic, validators, UI
│   │   └── Editor/           # Build scripts
│   ├── Packages/
│   │   └── fun.board-3.2.1.tgz
│   ├── README.md
│   ├── SETUP.md
│   ├── UNITY_SETUP_CHECKLIST.md
│   └── INTERACTION_LOG.md
├── GAME_DESIGN.md            # Order Up! puzzle concept
├── PRD.md                    # Product requirements
├── TASK_LIST.md              # Task breakdown
├── TECH_STACK.md             # Architecture
├── SUBMISSION.md             # Submission checklist
└── EVALUATION.md             # PRD self-review
```

---

## Running the Game

| Mode               | How                                                                                  |
| ------------------ | ------------------------------------------------------------------------------------ |
| **Simulator**      | Press Play in Unity Editor — Board Simulator runs automatically                      |
| **Physical Board** | Build Android APK → sideload to Board (menu: Board of Education → Build Android APK) |

---

## Key Documentation

| Doc                                                                                    | Purpose                           |
| -------------------------------------------------------------------------------------- | --------------------------------- |
| [BoardOfEducation/README.md](BoardOfEducation/README.md)                               | Unity project overview            |
| [BoardOfEducation/SETUP.md](BoardOfEducation/SETUP.md)                                 | Detailed setup and scene creation |
| [BoardOfEducation/UNITY_SETUP_CHECKLIST.md](BoardOfEducation/UNITY_SETUP_CHECKLIST.md) | First-time Unity checklist        |
| [GAME_DESIGN.md](GAME_DESIGN.md)                                                       | Puzzle concept (Order Up!)        |
| [SUBMISSION.md](SUBMISSION.md)                                                         | Submission and packaging          |
| [docs.dev.board.fun](https://docs.dev.board.fun/)                                      | Board SDK documentation           |

---

## Requirements

- Unity 2021.3 LTS
- Board SDK 3.2.1 (included)
- Board.fun console or Simulator (no physical device needed for development)

---

## References

- **Board SDK:** [docs.dev.board.fun](https://docs.dev.board.fun/) | [Discord](https://discord.gg/KccHAYgykD)
