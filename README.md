# AquaGuard

AquaGuard is a SCADA/HMI aquarium management system developed for our Project III: Software Development Life Cycle course at Conestoga College.

The system is designed to monitor aquarium habitat conditions, manage life-support equipment, alert staff to unsafe conditions, and log important system data.

## Features

AquaGuard is planned to include:

- Real-time habitat monitoring
- Graphical HMI for aquarium staff
- Warning and alarm system
- Equipment monitoring and control
- Simulated device data
- SQLite data logging
- Multiple aquarium habitats
- Visitor Mode for viewing aquarium habitats

## Monitoring

The system will monitor devices and conditions such as:

- Temperature
- Dissolved oxygen
- pH
- Salinity
- Water flow
- Circulation pumps
- Filtration systems
- Heater/Chiller
- Aerators
- Food dispensers

## Staff Controls

Through the AquaGuard HMI, staff will be able to control and configure aquarium equipment, including:

- Circulation pumps
- Filtration systems
- Heater/Chiller
- Aerators
- Food dispensers

## System Design

AquaGuard uses a central Hub to coordinate communication between the HMI, aquarium habitats, monitoring devices, controllable equipment, alarm system, and data logger.

Each habitat is composed of the sensors and equipment required to monitor and maintain its environment.

## Data Logging

AquaGuard will use SQLite to store important system information such as:

- Sensor readings
- Warnings and alarms
- Equipment activity
- Control changes
- System events

## Testing

The project will use both automated and manual testing.

Automated testing will focus on device logic, simulated data processing, alarms, equipment controls, Hub communication, and data logging.

Manual testing will focus on the HMI, navigation, staff controls, warning displays, Visitor Mode, and overall usability.

## Technologies

- C#
- .NET
- SQLite
- Git / GitHub
- Visual Studio

## System Architecture

The following diagram illustrates AquaGuard's system architecture, including the central Hub, shared Data Transit Bus, aquarium habitats, simulated devices, alarm management, and SQLite data logging.

```mermaid
flowchart TB
    flowchart TB
    Staff["Staff HMI"]

    Visitor["Visitor Mode"]

    Alarm["Alarm Manager"]
    Hub["HUB<br/>(Central SCADA)"]
    Logger["Data Logger"]
    DB[("SQLite Database<br/>(Alarms, Events & Data)")]

    Bus["SHARED DATA TRANSIT BUS"]

    H1["Habitat 1<br/>(Shark Tank)"]
    H2["Habitat 2<br/>(Coral Reef)"]
    H3["Habitat 3<br/>(Jellyfish Tank)"]

    S1["Sensors<br/>Temperature, pH, Oxygen,<br/>Salinity, Flow"]
    E1["Equipment<br/>Pumps, Filter, Heater/Chiller,<br/>Aerator, Food Dispenser"]

    S2["Sensors<br/>Temperature, pH, Oxygen,<br/>Salinity, Flow"]
    E2["Equipment<br/>Pumps, Filter, Heater/Chiller,<br/>Aerator, Food Dispenser"]

    S3["Sensors<br/>Temperature, pH, Oxygen,<br/>Salinity, Flow"]
    E3["Equipment<br/>Pumps, Filter, Heater/Chiller,<br/>Aerator, Food Dispenser"]

    F1[("Simulated Data File<br/>(Sensor Readings)")]
    F2[("Simulated Data File<br/>(Equipment Status)")]
    F3[("Simulated Data File<br/>(Sensor Readings)")]
    F4[("Simulated Data File<br/>(Equipment Status)")]
    F5[("Simulated Data File<br/>(Sensor Readings)")]
    F6[("Simulated Data File<br/>(Equipment Status)")]

    Staff <--> Hub
    Visitor <--> Hub

    Alarm <--> Hub
    Hub --> Logger
    Logger <--> DB

    Hub <--> Bus

    Bus <--> H1
    Bus <--> H2
    Bus <--> H3

    H1 <--> S1
    H1 <--> E1
    H2 <--> S2
    H2 <--> E2
    H3 <--> S3
    H3 <--> E3

    S1 --> F1
    E1 --> F2
    S2 --> F3
    E2 --> F4
    S3 --> F5
    E3 --> F6

    classDef central fill:#247bc0,stroke:#125487,color:#fff,stroke-width:2px
    classDef interface fill:#dceeff,stroke:#247bc0,color:#173e5b
    classDef habitat fill:#dff3e8,stroke:#358464,color:#174b36
    classDef device fill:#eef3f8,stroke:#64748b,color:#243447
    classDef storage fill:#fff0d9,stroke:#b77b29,color:#604017

    class Hub,Bus central
    class Staff,Visitor,Alarm,Logger interface
    class H1,H2,H3 habitat
    class S1,E1,S2,E2,S3,E3 device
    class DB,F1,F2,F3,F4,F5,F6 storage
```
## Project Team

**Group 7**  
CSCN72030 - Project III: Software Development Life Cycle  
Conestoga College
