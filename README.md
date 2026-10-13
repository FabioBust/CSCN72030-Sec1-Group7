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

## Class Diagram

The UML class diagram presents AquaGuard’s main classes, attributes, methods, and relationships. It illustrates how the login system, Staff and Visitor Modes, and central Hub interact with the monitoring, control, alarm management, and data logging components, following a modular design based on SOLID principles.

```mermaid
classDiagram
    direction TB

class LoginWindow {
    -AuthenticationService authService
    +SelectStaffMode() void
    +SelectVisitorMode() void
    +SubmitStaffCode(code: string) void
    +OpenStaffDashboard() void
    +OpenVisitorMode() void
    +DisplayLoginError() void
}

class AuthenticationService {
    -string staffCodeHash
    +ValidateStaffCode(code: string) bool
}

    class StaffMode {
        -Hub hub
        +DisplaySensorData() void
        +DisplayEquipmentStatus() void
        +DisplayAlarms() void
        +SendControlCommand() void
    }


    class VisitorMode {
    -Hub hub
    +SelectHabitat(habitatId: string) void
    +ViewCameraFeed() void
    +RequestFeeding(habitatId: string) bool
    +DisplayFeedingAvailability() void
}

    class Hub {
        -List~Habitat~ habitats
        -DataTransitBus bus
        -AlarmManager alarmManager
        -DataLogger dataLogger
        +GetHabitat(id: string) Habitat
        +ProcessMessage(msg: SystemMessage) void
        +UpdateEquipment(id: string, settings: object) bool
        +RequestFeeding(habitatId: string) bool
    }

    class DataLogger {
        -string databasePath
        +LogReading(reading: SensorReading) void
        +LogEquipmentStatus(status: EquipmentStatus) void
        +LogAlarm(alarm: Alarm) void
        +LogEvent(message: SystemMessage) void
        +GetHistory(habitatId: string) void
    }

    class DataTransitBus {
        +Publish(message: SystemMessage) void
        +SendCommand(message: SystemMessage) void
    }

    class AlarmManager {
        -List~Alarm~ activeAlarms
        +Evaluate(reading: SensorReading) void
        +ResolveAlarm(alarmId: string) void
        +GetActiveAlarms() List~Alarm~
    }

    class Habitat {
        +string habitatId
        +string name
        -List~Sensor~ sensors
        -List~Equipment~ equipment
        +GetSensors() List~Sensor~
        +GetEquipment() List~Equipment~
    }

    class Alarm {
        +string alarmId
        +string habitatId
        +string deviceId
        +string message
        +string severity
        +DateTime timestamp
        +bool isActive
    }

    class Sensor {
        +string deviceId
        +string habitatId
        +string parameter
        +string unit
        +ReadNextValue() SensorReading
    }

    class Equipment {
        +string deviceId
        +string habitatId
        +string equipmentType
        +bool isEnabled
        +GetStatus() EquipmentStatus
        +UpdateSettings(settings: object) bool
    }

    class FileDataProvider {
        -string filePath
        -int currentIndex
        +ReadNextRecord() string
        +Reset() void
    }

    class SensorReading {
        +string deviceId
        +string habitatId
        +double value
        +DateTime timestamp
    }

    class SystemMessage {
        +string habitatId
        +string deviceId
        +string messageType
        +string payload
        +DateTime timestamp
    }

    class EquipmentStatus {
        +string deviceId
        +string habitatId
        +bool isEnabled
        +DateTime timestamp
    }

    %% LOGIN AND AUTHENTICATION
LoginWindow --> AuthenticationService : validates staff code
LoginWindow --> StaffMode : opens if authorized
LoginWindow --> VisitorMode : opens without code

%% USER INTERFACES
StaffMode --> Hub : staff monitoring and control
VisitorMode --> Hub : visitor data and feeding

    %% CENTRAL SYSTEM COORDINATION
    Hub --> DataLogger : requests logging
    Hub --> DataTransitBus : communicates via
    Hub --> AlarmManager : requests evaluation

    %% SYSTEM SERVICES
    DataTransitBus --> Habitat : routes messages
    DataTransitBus ..> SystemMessage : transports
    AlarmManager "1" *-- "0..*" Alarm : manages

    %% HABITAT COMPOSITION
    Habitat "1" *-- "1..*" Sensor : contains
    Habitat "1" *-- "1..*" Equipment : contains

    %% SIMULATED DEVICE DATA
    Sensor ..> FileDataProvider : reads data
    Equipment ..> FileDataProvider : reads status

    %% DEVICE DATA MODELS
    Sensor ..> SensorReading : produces
    Equipment ..> EquipmentStatus : produces

    %% MONOCHROME UML STYLING
    classDef default fill:#ffffff,stroke:#000000,color:#000000,stroke-width:1.5px
```
## Project Team

**Group 7**  
CSCN72030 - Project III: Software Development Life Cycle  
Conestoga College
