# AquaGuard Sprint 2 test plans

PR coordinates the plans, fixture handoff, and evidence review. Module owners review cases for their components. Chintan supplies or reviews the system fixtures for integration tests. The requirement IDs below refer to the final Se7en SRS. Use the same build and fixture revision when comparing results.

## Shared setup and evidence

- Use Windows and the .NET SDK specified by CI. Run `dotnet test AquaGuard.sln --configuration Release --logger trx` from the repository root for the automated baseline.
- Prepare two test habitats with distinct names and recognizable readings. Use `temperature.txt`, `oxygen.txt`, `ph.txt`, `flow.txt`, and `salinity.txt` where applicable. Include looping, malformed and empty records, and a valid recovery record. Set `AQUAGUARD_SENSOR_DATA_DIRECTORY` to the fixture directory before launch.
- Use a test-only staff code and a fresh SQLite file per test run. Set `AQUAGUARD_STAFF_CODE_SHA256` to the code's SHA-256 hex digest and `AQUAGUARD_DATABASE_PATH` to the test database path. Use a controllable clock for feeding integration cases.
- Record case ID, requirement IDs, commit, environment, fixture revision, steps, expected and actual results, status (pass, fail, blocked, or unrun), evidence, and any defect ID. Use synthetic inputs throughout. Module gaps are blockers for the relevant case.

## AG-42 Unit test plan

| ID | Requirement or risk | Setup and expected result | Existing or planned tests |
| --- | --- | --- | --- |
| U01 | REQ-FUN-005, REQ-NFR-007, Rsk-002 | Read a two-record file three times. Expect first, second, first. Empty, invalid and missing records produce unavailable readings as appropriate. Valid data after an invalid record is processed. | `FileDataProviderTests` and individual sensor tests |
| U02 | REQ-FUN-012–015, REQ-NFR-007, Rsk-003 | Configure separate thresholds for two habitats. Exercise values on both sides of every boundary, including equality. Expect the configured Normal, Warning, or Critical class. Exercise Warning → Critical → Normal and repeated severity. Expect one active alarm, updated severity and resolution. Missing data does not create a valid environmental classification. | Planned for threshold and alarm modules |
| U03 | REQ-FUN-006–011, REQ-NFR-007 | Try both endpoints, values immediately outside the range, and unsupported fractional values. Assert accepted values and unchanged state on rejection. Exercise both on/off states for boolean devices. | `CirculationPumpTests`, `FilterSystemTests`, `AeratorTests`, `HeaterChillerTests`, `FoodDispenserTests` |
| U04 | REQ-FUN-020–023, REQ-NFR-006 | Reject disabled or non-$2.00 feeding, accept one portion, deny an immediate repeat and a request one tick before two hours, then accept at two hours. Exercise independent habitat histories. Test the rolling cap with seeded SQLite history, then cross the 24-hour cutoff. | `FoodDispenserTests` and `EquipmentWorkflowTests` |
| U05 | REQ-FUN-023–027, REQ-NFR-006 | Check blank, malformed, wrong, and valid staff credentials. Call command APIs before sign-in and after returning to mode selection. Expect denials without a configuration change. | `AuthenticationServiceTests`, `EquipmentControlServiceTests`, `HubTests`, `EquipmentWorkflowTests` |
| U06 | REQ-FUN-016–017, REQ-NFR-005/007, Rsk-004 | Write an accepted setting change and query a fresh connection. Expect matching habitat, device, setting, value and UTC timestamp. Reject invalid changes without writing. Simulate write failure and verify unchanged state. When other event types are wired, reopen the database and verify all types plus habitat filters. | Heater write, reopen and rejection in `EquipmentWorkflowTests`; feeding persistence also covered there. Remaining event types and history API are planned |

The two-hour interval limits successful feedings to at most 12 in a rolling 24-hour window with ordinary chronological operation. The implementation also checks the explicit 20-portion limit.

## AG-43 Integration test plan

Use the Sprint 1 reference approach from AG-70: repeatable simulated inputs, traceable component handoffs, and expected results committed with the plan. Chintan provides fixtures for full application scenarios. Use a telemetry subscriber to inspect payloads and query SQLite from a separate connection. For WPF checks, compare the visible result with the event and database evidence.

| ID | SRS | Handoff and procedure | Expected result |
| --- | --- | --- | --- |
| I01 | REQ-FUN-002/003/005, REQ-INT-009, REQ-NFR-003/004 | Feed valid, invalid and recovery records from two habitat folders through FileDataProvider, Hub and DataTransitBus to WPF. Record a timestamp at receipt and at display. | Correct habitat/device IDs and UTC time. Record order loops, invalid readings show No Data, valid peers continue and valid recovery appears within 3 seconds. Existing partial automation in `HabitatFileIntegrationTests` and `HubTests`. |
| I02 | REQ-FUN-012–017, REQ-INT-005 | With configured thresholds, drive file readings through Warning, Critical, and Normal. Observe HMI and query SQLite after restart. | One alarm changes severity, resolves at Normal, and produces attributable history. Requires alarm and logger integration. |
| I03 | REQ-FUN-006–011/016/023, REQ-USR-005/006 | Switch habitats. Send a legal heater setting, an unsupported increment, and an unauthenticated direct command. Compare device state, telemetry, WPF feedback, and SQLite rows. Repeat for each control as it becomes available. | Correct device changes once. Rejected requests preserve prior state and produce no change row. WPF identifies the accepted setting or invalid input. Heater and Hub/SQLite checks exist in `EquipmentWorkflowTests`. |
| I04 | REQ-FUN-010/020–023, REQ-INT-008 | Enable the feeder in one habitat, return to login, enter Visitor Mode, and feed using the simulated donation. Attempt to feed the other habitat and immediately retry the first. | Eligibility follows the selected habitat, one portion succeeds, cooldown blocks repeats, and Visitor Mode cannot alter dispenser configuration. Hub, persisted cooldown, and rolling cap coverage in `EquipmentWorkflowTests`. WPF execution is manual. |
| I05 | REQ-FUN-016/017, REQ-NFR-005 | Reopen the database, query both habitats, and test write failure with an unusable path on an isolated fixture. | Committed records survive restart, queries isolate habitats, and failed writes give meaningful feedback. Full event coverage and staff retrieval require the corresponding logger features. |

## AG-44 System test plan and traceability matrix

Run the manual cases in the built application using test data. Where a requirement is architectural or directly concerns automated coverage, use the review or test evidence in the procedure.

| ID | Requirement IDs | Procedure and expected result |
| --- | --- | --- |
| S01 | REQ-FUN-024–027, REQ-INT-001/010/011, REQ-USR-007 | Launch. Verify separate Staff and Visitor choices. Select Staff, enter an invalid code and see an authentication error. Enter the valid code and reach Staff. Return and enter Visitor without a code. Apply UXR07 for timing. |
| S02 | REQ-FUN-001–005, REQ-INT-002/003, REQ-NFR-003/004, REQ-USR-001 | Switch among configured habitats. Verify sensor name, unit, reading and class, plus the latest equipment status. Observe cyclic data. Disable one data source and verify No Data while other devices continue. Restore it and time valid updates against the 3-second limit. Apply UXR01. |
| S03 | REQ-FUN-006–011, REQ-INT-004, REQ-USR-002/005/006 | Change pump power/speed, filter power, aerator output, heater target, and feeder enablement. Confirm the named device and accepted setting. Try out-of-range values and a 22.25 °C heater target. Confirm clear errors and unchanged state. Apply UXR02/05/06. |
| S04 | REQ-FUN-012–015, REQ-INT-005, REQ-USR-003 | Configure thresholds independently for two habitats. Inject values across boundaries to trigger Warning, Critical, and Normal. Verify classification, alarm creation, escalation and resolution. Verify habitat, sensor, severity and timestamp. Apply UXR03. |
| S05 | REQ-FUN-016/017, REQ-NFR-005, REQ-SYS-060 | Generate a sensor reading, control change, alarm and feed. Restart, select each habitat, retrieve its records and compare values, IDs and timestamps with SQLite. |
| S06 | REQ-FUN-018–022, REQ-INT-006–008, REQ-USR-004 | Choose two visitor habitats and verify their distinct simulated camera feeds. Test disabled, eligible, within-cooldown, and exactly-two-hours cases. Use seeded history for the 20-portion boundary. Expect one fixed portion after approved simulated CAD $2.00 donation and visible eligibility feedback. Apply UXR04. |
| S07 | REQ-FUN-023, REQ-NFR-006 | In Visitor Mode, check that staff controls are unavailable. Attempt an equipment change directly through the Hub without a staff session. Verify denial and unchanged equipment state. |
| S08 | REQ-INT-009, REQ-SYS-050 | Capture one sensor and one equipment message. Match the habitat ID, device ID, reading or state, and timestamp to the originating fixture and command. |
| S09 | REQ-SYS-030/040 | Inventory device IDs with observed sensor readings or equipment status. Count separately configured devices. Verify at least seven distinct monitored devices. Exercise five distinct controllable devices and observe the resulting changes. Record IDs and evidence. |
| S10 | REQ-NFR-001/002 | Review C# component responsibilities. Add a test sensor using the device contract and verify it can publish without editing existing sensor classes. |
| S11 | REQ-NFR-007/008 | Run the automated suite and review evidence for data, classification, controls and database operations. Review the manual Staff and Visitor cases for reproducible steps and expected results. |

Several S03–S06 checks depend on features beyond the shell and equipment work. Record the exact unavailable feature when blocking a case and rerun it once integrated.

## AG-45 Usability test plan

Recruit first-time participants for each mode and record the total for each task. Use a consistent laptop, window size, data fixtures and initial screen. Use the same neutral prompts. Provide no hints while timing. Restart each task in its defined initial state. Use synthetic accounts and participant labels. Record elapsed seconds, result, assistance, observations and any error. Count an assisted, timed-out or incorrect attempt as unsuccessful. Apply the 80% threshold separately to each timed task rather than averaging results.

| ID | Requirement | Participant task and success criteria |
| --- | --- | --- |
| UXR01 | REQ-USR-001 | With the staff dashboard open, select the named habitat and report its current temperature. At least 80% complete without assistance in 60 seconds. |
| UXR02 | REQ-USR-002 | With the staff dashboard open, set the named pump to the provided legal percentage and verify it. At least 80% complete without assistance in 90 seconds. |
| UXR03 | REQ-USR-003 | With a prepared active alarm, identify its habitat and severity. At least 80% complete without assistance in 60 seconds. |
| UXR04 | REQ-USR-004 | In Visitor Mode, find a named habitat feed, then switch to the second named feed. At least 80% complete without assistance in 60 seconds. |
| UXR05 | REQ-USR-005 | Submit a valid heater change and ask the participant to identify the affected device and resulting value from the confirmation. Verify that the confirmation names both. |
| UXR06 | REQ-USR-006 | Submit an unsupported heater increment and ask the participant to identify the rejected value and recover using the feedback. Verify that the message identifies the invalid setting. |
| UXR07 | REQ-USR-007 | Start at mode selection with the valid test code supplied. At least 80% enter Staff Mode without assistance in 60 seconds. |

Compute success per timed task as successful unassisted attempts divided by all attempts. Record incomplete features as blocked before starting the session. Report participant counts, completion rates, timing and observed issues, and repeat affected tasks with first-time participants after fixes.

## Local smoke-test results

Environment: Windows, Release build, local branch pr/sprint-2, synthetic reef
habitat with a 23.5 °C reading and a fresh SQLite database.

- S03 (heater cases): Passed. Staff applied 22.5 °C and saw confirmation.
  The app rejected 22.25 °C and showed the invalid input. On returning to
  Staff Mode, the displayed target was still 22.5 °C.
- I04 (Staff-to-Visitor path): Passed. Staff enabled the reef dispenser.
  Visitor Mode showed feeding available, dispensed one simulated portion,
  and disabled the feeding button immediately afterward.
- Release automation: 156 passed, 0 failed, 0 skipped.

The smoke test covered these specific cases. The remaining manual system and
participant usability cases still require execution.

## Additional manual checks

Used the Release app with synthetic kelp and reef habitats and a fresh SQLite
database. Kelp reported 17.5 °C. Reef reported 23.5 °C with an empty oxygen file.

| Case | Status | Evidence or remaining work |
| --- | --- | --- |
| S01 | Functional checks passed | Invalid Staff code displayed an error. The valid code opened the Staff dashboard. Visitor Mode opened without a code. The UXR07 participant timing check remains unrun. |
| S02 | Partial | Switching from kelp to reef replaced the sensor rows. Reef temperature was 23.5 °C and oxygen displayed No Data while temperature remained visible. Units, classification, equipment status, live outage/recovery and timing were not verified here. |
| I04 | Passed for the two-habitat workflow | Only reef was enabled by Staff. Visitor feeding was unavailable in kelp. Reef dispensed one portion and disabled the button. After closing the app, a separate SQLite connection found exactly one event for reef/food-dispenser. |
| S05 | Partial | The persisted feeding row was verified independently. Full event-type coverage and Staff history retrieval remain blocked on integration. |
| UXR01–UXR07 | Participant sessions unrun | The functional smoke checks supply no participant completion-rate or timing evidence. |

## US-4 Habitat Selection acceptance

| Acceptance criterion | Evidence | Result |
| --- | --- | --- |
| Staff can select a configured habitat using a Hub-backed WPF selector. | `MainWindow.xaml` defines `HabitatSelection`. `MainWindow.xaml.cs` binds its items to `Hub.Habitats`. In the Release app, the configured kelp and reef habitats were available in the selector. | Pass |
| Selecting a different habitat updates the dashboard to that habitat. | With kelp selected, Staff saw temperature 17.5 °C. Switching to reef replaced the kelp rows with reef temperature 23.5 °C and reef oxygen No Data. | Pass |

Traceability: REQ-FUN-001 and REQ-INT-002. The manual evidence comes from the
two-habitat Release check recorded above. The implementation was delivered in
GitHub PR #20, which was merged at `aa812d6`. This acceptance check covers
habitat selection. The separate UXR01 participant timing case remains unrun.

## Sensor recovery and database failure checks

- S02/I01 recovery subset passed. With the Staff dashboard open, removed the
  synthetic reef oxygen file. Oxygen changed from 7.2 to No Data while
  temperature remained 23.5. Restored the file and observed oxygen return to 7.2
  without restarting the app. The three-second threshold was not timed.
- I05 heater write-failure subset passed. Pointed the SQLite path at a directory
  and attempted to apply a valid 22.5 °C heater target. The app displayed a
  logging error. Returning to Staff Mode confirmed that the target remained unset.
- `HeaterWriteFailurePreservesTargetAndDoesNotPublishChange` also checks the
  failure with a previously set target. It verifies that the previous value is
  preserved and that the Hub publishes no equipment change. The Release suite
  with this regression test passed 157 tests.
- `FeedingWriteFailureDoesNotConsumePortionOrStartCooldown` verifies that a
  failed feeding write leaves the dispenser eligible after storage recovers.
  The retry records one portion and starts the cooldown. The Release suite
  with both database failure tests passed 158 tests.

## US-5 sensor refresh acceptance

Tested the Release dashboard matching merged commit `34242d3` with a synthetic
reef temperature file. The dashboard initially displayed 21.5 °C. For each
trial, recorded UTC immediately before writing a new single-record file value
and again after the dashboard accessibility tree first showed that value with
unit °C. The interval includes file write and observation overhead.

| New temperature | Observed interval | REQ-NFR-003 |
| --- | ---: | --- |
| 22.5 °C | 1,134 ms | Pass |
| 23.5 °C | 1,324 ms | Pass |
| 24.5 °C | 1,255 ms | Pass |

The run verifies temperature value and unit display and the three-second
refresh limit for this fixture. Sequential file looping and configured unit
metadata are covered by `HabitatFileIntegrationTests`. Environmental
classification remains pending.
