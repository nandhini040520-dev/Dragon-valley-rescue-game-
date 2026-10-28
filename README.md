# Adventure Quest: The Dragon Valley Rescue

This repository contains the Unity source project, game assets, preview gallery,
package settings, project settings, and build/run scripts. Generated Windows
players, videos, QA captures, and hackathon submission exports are kept outside
Git so the project stays straightforward to clone and build. The Phase 2 task
card and information panel use the Phase 1 visual language while showing live
mechanism state.

## Phase 2 gameplay upgrade

The current source now uses player-operated mechanisms across all seven regions. Launch **Run-Phase2.bat** for the updated Windows build. Read [Phase 2 mechanism handover](HackathonDocs/Phase2-Mechanism-Handover.md) for controls, the jury demo route, architecture, replay and verification. `Build/Windows`, the original video, and the older release ZIP below document the earlier robot-action prototype.

A peaceful third-person STEM adventure, built for **Discovery World** with Unity **6000.6.0f1** and the built-in renderer. All seven regions connect in `AdventureWorld`. The environment retains its storybook style; the four Funobotz now follow the participant-provided real-kit screenshots, with white card panels, orange seams, printed labels and their correct physical mechanisms.

## Gameplay video

The [90-second gameplay video](Videos/Adventure-Quest-Full-Gameplay-90s.mp4) shows all seven regions, the dragon rescue and victory, with game audio. It uses an automated route through the actual Unity player, edited to fit 90 seconds. See [video details and chapter times](Videos/README.md).

## Assets and images

Quacky's 18 September refinement adds the attached folded head, a two-tier octagonal body, rounded crawling connectors and depth-tested kit printing from the supplied reference. Its updated prefab, PNG and Windows player are included in the normal deliverables. See `QA/Quacky-Refinement.md` for verification.

Open **`AssetPreviews/index.html`** for the complete local image gallery. It includes actual Unity model portraits, the character lineup, views of all seven regions, a tall overhead render of the connected world, the illustrated journal map, and four supplied kit references. The earlier invented robot concept is superseded and excluded from the current gallery and image ZIP.

Reusable model prefabs live in `Assets/Art/Prefabs`; their meshes and materials are saved under `Assets/Art/Models` and `Assets/Art/Materials`. The four Funobotz, explorer, dragon, delivery crate, signal crystals, lantern, forest tree and fairy cottage have individual PNG previews and prefab assets. The game's model factories and regional scenery share these authored definitions in `StorybookAssets.cs` and `StorybookEnvironment.cs`.

`Release/Dragon-Valley-Storybook-Assets.unitypackage` contains the 11 prefabs, 97 saved mesh assets, painted materials, their dependencies, and the illustrated map. Import it into a Unity 6 project using the built-in renderer through **Assets > Import Package > Custom Package**. The custom paint shader targets the built-in renderer. `Release/Dragon-Valley-Asset-Images.zip` contains 25 current images, including the four supplied references, and the offline gallery. `Release/Adventure-Quest-Windows.zip` contains the playable Windows build; extract it before launching.

The corrected kits are defined in `FunobotzModels.cs`; `FunobotzMotion.cs` animates the crawling joints, tollgate and manipulating arm. `TikoController` carries an object at the segmented arm's end socket. Photographs supply visual identity, not exact measurements; unseen geometry is simplified.

## Hackathon documents

`HackathonDocs` contains the Phase 2 submission following the supplied eight-section format, a demo/technical handover, a blank external playtest record, ten live evidence images, and twelve supplied Unity Editor screenshots with date/time captions. Use `Crust-Innovators-Phase-2-Submission-With-Dated-Evidence.docx` for the current 24-page submission. Team: **Crust Innovators** — **Devanand S, Karthiga S, Dharsana K, Nandhini S**, **AVS Engineering College**. Automated implementation checks are documented separately from the external player session that remains to be recorded.

Use **Adventure Quest → Export Storybook Models and Images** to regenerate the prefabs and model portraits. Launch the Windows player with `-assetTour` to export actual regional and world-overview images. The tour uses separate QA saves. The illustrated map is loaded from `Assets/Resources/Storybook/ExpeditionMap.png`.

## Play

Double-click `Run-Adventure.bat` in the project folder, or extract `Release/Adventure-Quest-Windows.zip` and run the included `Run-Adventure.bat`. Unity Editor is not required to play. Keep the executable, `Adventure Quest_Data`, `UnityPlayer.dll`, and `MonoBleedingEdge` together. Select **START ADVENTURE** for a fresh expedition, or **CONTINUE** to resume the last unlocked region.

New adventures begin with a three-page tutorial covering keyboard/mouse controls, the four Funobotz and the first mission. Use **Enter** to advance, **Backspace** to go back, or **Esc** to close the guide. On the robot page, **1 / 2 / 3 / 4** or the card buttons preview each selection. Gameplay waits while the guide is open. Revisit it through **Esc > TUTORIAL & CONTROLS**; closing it returns to the pause menu. Continue resumes directly into the saved expedition.

The playable world now has worn path borders, region-colored entrance pennants, compass inlays and small flower clusters. The illustrated Q-key map is unchanged.

| Control | Action |
|---|---|
| WASD | Move |
| Mouse | Orbit the third-person camera |
| Space | Jump |
| E | Interact with a nearby object |
| F | Inspect the nearby object |
| 1 / 2 / 3 / 4 | Petalo / Quacky / Tolly / Tiko |
| Q | Show the objective journal and hint |
| Esc | Pause, resume, reset, or return to menu |

Approach the glowing task marker. The HUD names the nearby interaction. Some objects must be inspected with **F** before **E** works. Cyan pads are safe placement areas; pink plant beds are protected. A wrong placement resets the object, which can then be lifted again. The dragon is a friend, never an enemy.

## Short demo route

1. **Grand Gateway:** inspect either route sign, choose it, then select Tolly and activate the gateway checkpoint. Both introductory routes join the same expedition.
2. **Hidden Forest:** inspect the branch, lift it with Tiko, place it on the cyan clearing, and restore the route sign. Try the protected plant bed to demonstrate recoverable environmental feedback.
3. **Rainbow Bridge:** inspect the survey post, lift the support with Tiko, place it at the near-bank socket, deliver the supply with Quacky, and walk across to the checkpoint.
4. **Castle Place:** Tolly opens the courtyard; collect the crystal, load the container, lift it with Tiko, place it on the lift, and activate the tower receiver with Petalo.
5. **Mystery Cave:** inspect and illuminate the marker stone with Petalo, light beacon I, send Quacky through the narrow passage, then use Petalo for beacons II and III.
6. **Fairy Garden:** Quacky delivers the path kit. Inspect and lift the heavy stone with Tiko, place it on the cyan foundation, and activate the flower marker.
7. **Dragon Valley:** Quacky delivers energy, Tiko aligns the mechanical block, Petalo lights the beacon, and Tolly operates access. Only then can the restoration altar rescue the dragon and open the final portal.

## Open and rebuild in Unity

Open this folder using Unity Hub with **6000.6.0f1**. Open `Assets/Scenes/AdventureWorld.unity` and press Play. The scene contains the game director; its modular world builder creates the authored storybook environment at runtime. This keeps the prototype reproducible. Puzzle definitions are in `MissionManager.cs`, scenery and object bindings in `AdventureWorld.cs`, and model definitions in `StorybookAssets.cs` and `StorybookEnvironment.cs`.

Use **Adventure Quest → Build Windows Prototype** or run `Build-Windows.ps1`. The Windows build uses the Mono scripting backend. No third-party assets, account services, networking, or external packages are required beyond Unity's built-in modules.

## Persistence

PlayerPrefs stores the unlocked region, explorer tokens, and completed-adventure state. Saving at region boundaries replays an unfinished region from its start and avoids partially restored physics mechanisms. **RESET ADVENTURE**, **PLAY AGAIN**, and starting a new adventure clear this game's progress only.

## Verification

`Run-QA.ps1` launches the standalone player with `-questQA`. The acceptance driver uses the real CharacterController and interaction manager, visits every region, checks wrong robots and inspection requirements, tests protected-plant recovery, completes the rescue, checks victory, and restarts. Screenshots and the detailed results are written to `QA/`. It is opt-in and does not run during normal play. Read `QA/acceptance-results.txt` for the actual result; the existence of the harness alone does not establish a pass.

Models combine custom meshes and shaped components in a storybook style. This is a playable hackathon prototype, with authored direct robot movement and generated placeholder sound effects. The illustrated reference sheet is art direction; the PNG model previews show the implemented geometry.
