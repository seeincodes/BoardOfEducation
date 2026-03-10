# Build and Deploy to Board

Step-by-step directions to package the game and test on a physical Board console.

---

## Part 1: Build the APK

### Prerequisites

- Unity 2021.3 LTS with **Android Build Support** installed
- At least one scene added to Build Settings (File → Build Settings → Scenes In Build)
- Board piece set model configured (e.g., Arcade) — see [UNITY_SETUP_CHECKLIST.md](UNITY_SETUP_CHECKLIST.md)

### Build Steps

**Option A — Menu (easiest)**

1. Open the project in Unity
2. **Board of Education** → **Build Android APK**
3. APK is created at `BoardOfEducation/Build/BoardOfEducation.apk`

**Option B — Manual**

1. **File** → **Build Settings**
2. **Platform:** Android (switch if needed)
3. Ensure your game scene(s) are in **Scenes In Build** and enabled
4. Click **Build** (or **Build and Run**)
5. Save as `Build/BoardOfEducation.apk`

**Option C — Command line**

```bash
/Applications/Unity/Hub/Editor/2021.3.56f2/Unity.app/Contents/MacOS/Unity \
  -batchmode -quit \
  -projectPath /path/to/BoardOfEducation \
  -executeMethod BoardOfEducation.Editor.BoardBuild.BuildAndroid
```

---

## Part 2: Deploy to Board

### Prerequisites

- Physical Board console
- USB cable (Board to computer)
- **Board Developer Bridge (bdb)** — [Download](https://dev.board.fun/) (macOS Universal, Linux, or Windows)

### Install bdb

1. Go to [dev.board.fun](https://dev.board.fun/)
2. Accept Developer Terms
3. Download **Board Developer Bridge (bdb)** for your OS
4. **macOS:** Make executable: `chmod +x bdb`
5. Move to a folder in your PATH, or use full path

### Deploy via USB

1. **Connect Board to computer** with USB cable
2. **Enable USB debugging** on Board (if prompted — check Board docs)
3. Run:

```bash
./bdb install Build/BoardOfEducation.apk
```

Or with full path:

```bash
./bdb install /path/to/BoardOfEducation/Build/BoardOfEducation.apk
```

4. Wait for install to complete
5. Launch **Board of Education** from the Board home screen

### Alternative: ADB (if bdb is unavailable)

Board runs Android. If you have Android Debug Bridge (adb) installed:

```bash
adb devices                    # Verify Board is connected
adb install Build/BoardOfEducation.apk
```

---

## Part 3: Test on Board

1. **Launch** the game from the Board home screen
2. **Place physical pieces** (Arcade or configured piece set) on the Board
3. **Play** — place pieces in correct sequence, check feedback
4. **Logs** — interaction data is written to the Board’s storage (see [INTERACTION_LOG.md](INTERACTION_LOG.md) for schema)

### Retrieving Logs from Board

If you need to pull the interaction log CSV from the Board:

```bash
adb shell "run-as com.DefaultCompany.BoardOfEducation cat /data/data/com.DefaultCompany.BoardOfEducation/files/interaction_logs/session_*.csv"
```

Or use `adb pull` after locating the log path (Board may use `getExternalFilesDir` or similar).

---

## Troubleshooting

| Issue | Fix |
|-------|-----|
| **Unity shuts down on build** | Fixed in BoardBuild.cs — Exit only runs in batch mode. If still happening: try File → Build Settings → Build (manual) instead of menu; check Console for errors before crash. |
| **[Board SDK] Project settings not configured** | Run **Board → Configure Unity Project...** — sets Min API 33, ARM64, IL2CPP |
| **[Board SDK] No Piece Set Model configured** | Edit → Project Settings → Board → Input Settings → set Piece Set Model to `arcade_v1.3.7.tflite`. Ensure file exists in `Assets/StreamingAssets/`. |
| Build fails: "No scenes" | **Board of Education → Create Starter Scene** (creates `Assets/Scenes/Game.unity` and adds to build), or save your scene and use **Add Open Scene to Build Settings** |
| Build fails: Android module missing | Unity Hub → Add Modules → Android Build Support |
| `AndroidJavaClass` / Android JNI not found | `com.unity.modules.androidjni` added to Packages/manifest.json — reload project |
| bdb: "device not found" | Check USB connection; enable USB debugging on Board |
| Game doesn’t recognize pieces | Ensure piece set model (e.g., arcade_v1.3.7.tflite) is in StreamingAssets and configured in Board Input Settings |
| Black screen on launch | Check Board docs; verify Android API level and build settings |

---

## References

- [Board Developer Portal](https://dev.board.fun/) — SDK, bdb, piece set models
- [Board Docs](https://docs.dev.board.fun/) — SDK documentation
- [UNITY_SETUP_CHECKLIST.md](UNITY_SETUP_CHECKLIST.md) — Project setup
