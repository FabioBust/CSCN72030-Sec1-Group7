# AG-72 Architecture and SOLID Review

Reviewed against master commit 03fd59d on 25 November 2026.
Traceability covers REQ-SYS-001/003/030/040 and REQ-NFR-001/002.
QA reviewer is Pratyasa Roy. Integration reviewer is Chintan Patel.

## Responsibilities and extension points

| Component | Responsibility and current evidence |
| --- | --- |
| WPF windows | Display selected habitat data and feedback, collect user actions, and schedule refreshes. They delegate device operations to the Hub. |
| FileDataProvider | Read and loop external records, returning unavailable data on file access failure. It has no UI, database, or alarm responsibilities. |
| HabitatFileSources | Construct habitats and adapt files into sensor callbacks. Its generic file sensor accepts additional numeric device files. |
| DataTransitBus | Deliver telemetry and route accepted equipment commands. Authorization occurs before routing through Hub and EquipmentControlService. |
| Hub | Coordinate registered habitats, sensor callbacks, environmental classification, control commands, and history persistence. |
| AlertManager | Maintain environmental alert state and emit raised, updated, and resolved transitions. The Alarms/AlarmManager class is an unused placeholder. The running implementation is Core/AlertManager. |
| DataLogger | Own SQLite commands, transactions, timestamp normalization, and history queries. It reports failed writes through exceptions. Callers provide feedback or retry according to the operation. |

Habitat depends on IDevice. A new sensor can implement IDevice and register a
reading callback through Hub.RegisterSensorInput. Existing sensor classes do
not require modification. ArchitectureAcceptanceTests exercises this extension
with an independent sensor implementation and two additional numeric files.

Hub and equipment code currently depend on the concrete internal DataLogger.
This is a coupling to revisit when a second storage implementation is required.
Hub polling is synchronous on the WPF refresh path. SQLite lock waits are now
bounded to one second per write operation. A poll with both sensor and alarm
writes can wait for both operations, so overall refresh latency under contention
still needs a timed integration check.

## Capacity checks

ArchitectureAcceptanceTests creates seven monitored devices in one habitat
using temperature, oxygen, pH, flow, salinity, water level, and turbidity files.
All seven produce numeric readings. The last two exercise generic numeric
extension support and have no additional unit or physical range contract.

The same fixture registers five distinct controllable equipment objects.
It exercises pump power/speed, filter power, aerator output, heater target,
and dispenser enablement. Device-specific behavior remains covered by the
existing equipment and workflow tests.

## Contribution evidence

The repository history demonstrates at least one implemented object for each
of its four named contributors. This records contribution evidence. Formal
ongoing ownership assignments remain subject to the team's agreement.

| Team member | Implemented object | Commit evidence |
| --- | --- | --- |
| Daksh Bajaj | Hub, DataTransitBus and SalinitySensor | e4e0fbe and 4bbdf6b |
| Fabio | TemperatureSensor and habitat file integration | d84bf4b and 2e31f67 |
| Chintan Patel | CirculationPump and AlertManager | 1bf2102 and fb6783b |
| Pratyasa Roy | SQLite DataLogger integration and FoodDispenser workflows | b08c5ee and 555be11 |

## Acceptance status

Automated logger coverage checks all four event kinds with IDs, values and UTC
timestamps after reopening storage. It checks batch rollback, retained committed
rows, lock failure and retry, missing storage recovery, and rejection of
non-finite values. Existing integration tests cover equipment state preservation
on failed writes, sensor polling during unavailable storage, and alarm retries.

Pratyasa's QA review, Chintan's integration review, confirmation of the formal
ownership assignments, and the refresh timing check under storage contention
remain pending. Keep AG-72 open until those applicable checks pass.
