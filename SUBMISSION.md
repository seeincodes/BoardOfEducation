# Board of Education — Submission Checklist

## Deliverables (per SuperBuilders BoE)

| Deliverable | Status | Notes |
|-------------|--------|-------|
| Working prototype (sideload or simulator) | Ready | Build for Android, sideload to Board |
| Sample data log | Done | `BoardOfEducation/sample_interaction_log.csv` |
| 1–2 min demo video | Pending | Record collaborative gameplay |
| Comprehensive README | Done | `BoardOfEducation/README.md`, `SETUP.md` |

## Packaging for Submission

### Option A: GitHub Repo (Recommended)

1. Ensure all changes are committed and pushed to `git@github.com:seeincodes/BoardOfEducation.git`
2. Tag release: `git tag v1.0 && git push origin v1.0`
3. Share repo URL with SuperBuilders

### Option B: ZIP Archive

1. **Exclude:** `.git/`, `Library/`, `Temp/`, `Logs/`, `obj/`, `Build/`, `*.csproj`, `*.sln`
2. **Include:** All source, docs, `sample_interaction_log.csv`, `Packages/`
3. Create: `BoardOfEducation-SuperBuilders-YYYYMMDD.zip`

### Build Artifact (APK)

**Option A — Menu:** Board of Education → Build Android APK (creates `Build/BoardOfEducation.apk`)

**Option B — Command line:**
```bash
/path/to/Unity -batchmode -quit -projectPath /path/to/BoardOfEducation -executeMethod BoardOfEducation.Editor.BoardBuild.BuildAndroid
```

**Option C — Manual:** File → Build Settings → Android → Build

## Pre-Submission Verification

- [ ] Unity 2021.3 project opens without errors
- [ ] Board SDK packages resolve
- [ ] Game scene runs in Editor (Play mode)
- [ ] Interaction log is written to `persistentDataPath/interaction_logs/`
- [ ] README setup steps are accurate
- [ ] Sample log matches documented schema

## Demo Video Checklist

- [ ] Show 2 players at the Board (or simulator)
- [ ] Demonstrate piece placement (place, move, rotate)
- [ ] Show "Correct!" / "Try again!" feedback
- [ ] Show puzzle completion ("You won!")
- [ ] Mention interaction logging (optional: show log file)
- [ ] Keep to 1–2 minutes

**Script:** See [DEMO_VIDEO_SCRIPT.md](DEMO_VIDEO_SCRIPT.md)

## Contact

- **Technical:** Patrick Skinner (patrick.skinner@superbuilders.school)
- **Repo:** https://github.com/seeincodes/BoardOfEducation
