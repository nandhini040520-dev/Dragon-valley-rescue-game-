# Phase 2: player-operated world mechanisms

Team: Crust Innovators — Devanand S, Karthiga S, Dharsana K, Nandhini S.

This implementation responds to the attached jury brief. The player directly controls machines; selecting a Funobotz is never a requirement for completing a mechanism. The original movement controller, third-person camera, character models and seven-region environment remain in use.

## Run and rebuild

- Launch `Run-Phase2.bat` or `Build/Phase2/Adventure Quest Phase 2.exe`.
- Open `Assets/Scenes/AdventureWorld.unity` in Unity 6000.6.0f1 for the updated source.
- `Build-Phase2.ps1` builds an isolated copy under `QA/Phase2/BuildWorkspace` so the main project may stay open in Unity. Its executable goes to `Build/Phase2`.
- `Run-Phase2-QA.ps1` runs the new opt-in acceptance driver; inspect `QA/Phase2/acceptance-results.txt` for actual results.
- The older `Build/Windows` executable and release ZIP are the previous prototype. They are deliberately retained. The new launcher identifies the Phase 2 build.

## Explain the code to the jury

`MechanismComponent` has one upstream input, an enabled constraint, a boolean output and a change event. A changed output invokes the next component's evaluation. The chain is source → switch → cable/router → transmission → shaft/sensor → actuator.

`MechanismController.Refresh` supplies the constraints from player controls. Power removal immediately removes downstream outputs. Gear alignment, coupling pin, route, valves and reflectors are conditions, not objective flags. `Update` integrates actuator travel over five seconds (nine for the final core). A working output physically moves a collider, lift or sluice; the output checkpoint cannot complete until the dependency chain and safety requirements are satisfied.

`MechanismWorld` authors the locations, components, controls and physical output for each region. `InteractionManager` checks proximity and line of sight, then passes the operation to the machine. Robot selection is not consulted. `MissionManager` accepts completion only from a completed, safe machine and saves region boundaries with a separate Phase 2 key namespace.

## Demonstration route

1. **Grand Gateway:** F to inspect. Pick up the loose crystal, install it, switch power on, rotate the gear twice to II, collect the coupling pin, select a route, release the brake, drive and observe motion. Pass through the selected route and verify.
2. **Hidden Forest:** install power, switch on, select route 1, remove intake debris, open the restoration valve, release brake and drive. Moving water reaches the plants as the sluice opens.
3. **Rainbow Bridge:** permanent repair uses power, route 1, gear II and the pin. Release brake and drive; engage the brake after travel completes, then cross and verify. Alternatively use the hydraulic bypass, which does not require the gear or pin but expires after 25 seconds. Expiry retracts the deck and safely returns anyone on the span to the near bank.
4. **Castle Place:** configure power, route 1, gear II, pin, UP and floor 1. Release the brake, board the platform, then use its onboard drive. The counterweight descends as the platform carries the player up. Walk onto the upper landing and verify. If you fall off, select DOWN and floor 0 and operate the ground drive to recall the platform; then select UP / 1 and board again.
5. **Mystery Cave:** install power, select route 1, set reflector I to 1 and reflector II to 2, release brake and drive. Wrong upstream alignment blocks subsequent links. Moving light pulses show the connected path.
6. **Fairy Garden:** restore intake and valve, choose the irrigation route rather than the pink drainage channel, then operate the sluice. Wrong settings stop flow and can be corrected in place.
7. **Dragon Valley:** combine power routing, gear II, coupling pin, reflectors 1/2 and beacon enable. Release brake and start the restoration drive. Observe the machinery, staged valley lights and moving barrier. Verify at the final console to release Aster.

## Replay, saves and evidence

The bridge challenge is available from the main menu, pause menu and victory screen. It resets challenge state but does not delete or replace the campaign save. Permanent and bypass times have separate best records; operations, incorrect attempts, repeated one-shot actions, elapsed play time and optional service-clue discovery are tracked. Pause and tutorial time do not count. Partially completed machines reload from the region's initial state, which avoids loading unsafe moving mechanisms.

QA uses teleports only to arrange interaction fixtures. Critical bridge traversal and lift transport use the actual CharacterController. This is automated implementation verification, not a claim of external human playtesting or educational effectiveness. Original scripts are backed up under `QA/Phase2/Backup`.

These are deliberately simplified deterministic machines, not electrical or fluid physics simulations. The bridge is the principal alternative-solution puzzle; the lift has one implemented destination. Optional service inscriptions provide clue credit, and a woodland alcove beyond the gateway rewards a hidden-area discovery.
