# MediCore — Clinical Behavioral Design Patterns Suite

**MediCore Operations Center** is an interactive C# desktop suite built on .NET Framework 4.7.2 that models hospital clinical workflows to demonstrate all **10 Gang of Four (GoF) Behavioral Design Patterns**.

Rather than abstract or generic examples, each pattern is mapped to a critical healthcare responsibility—ranging from emergency room triage and cardiac telemetry traversal to adverse medication rollbacks, dynamic anesthesia snapshots, and operating theater sterility audits.

---

## Interactive Pattern Showcases

The application features a dynamic workbench panel paired with a real-time console audit feed. Selecting any pattern from the navigation bar loads its dedicated clinical workbench.

| Pattern | Clinical Domain Scenario | Key Components | Observable Behavior |
| :--- | :--- | :--- | :--- |
| **1. Chain of Responsibility** | Emergency Triage Escalation | `MedicalOfficer`, `TriageNurse`, `AttendingPhysician`, `ChiefSurgeon` | Patients are routed down an escalation ladder based on acuity levels (1–5). Level 1–2 is handled by outpatient nurses, 3–4 by observation physicians, and Level 5 escalates directly to surgical activation. |
| **2. Command** | Medication Injection & Adverse Rollback | `IMedicalCommand`, `AdministerMedicationCommand`, `PatientChartReceiver`, `MedicationDeliveryInvoker` | Encapsulates medication doses as executable command objects. The `MedicationDeliveryInvoker` maintains a history stack, allowing clinicians to reverse administered doses via antidote rollback. |
| **3. Iterator** | Sequential Vitals Traversal | `VitalReading`, `IIterator<T>`, `IAggregate<T>`, `TelemetryMonitor` | Sequentially traverses a private internal collection of timestamped heart rate and blood pressure logs without exposing the underlying storage structure. |
| **4. Mediator** | Inter-Departmental Trauma Alarm | `IHospitalMediator`, `TraumaControlMediator`, `EmergencyRoom`, `BloodBankUnit` | Dispatches critical trauma alarms across hospital departments without tight coupling. An alarm from the ER triggers immediate blood bank preparation through a centralized hub. |
| **5. Memento** | Intra-Operative Baseline Restoration | `PatientBaselineMemento`, `AnesthesiaProfile`, `AnesthesiologistCaretaker` | Captures an internal snapshot of patient oxygenation ($\text{SpO}_2$) and blood pressure prior to surgery, enabling rapid restoration if an acute hypoxic event occurs. |
| **6. Observer** | Real-Time Bedside Telemetry & Crash Alarms | `IBedsideObserver`, `BedsideTelemetryMonitor`, `NurseStationPager`, `CodeBlueSystem` | Subscribers listen for heart rate changes on bedside monitors. Normal vitals send routine pager updates, while severe tachycardia (>140 BPM) triggers automatic Code Blue crash alarms. |
| **7. State** | Clinical Acuity Lifecycle | `IClinicalState`, `PatientContext`, `StableState`, `DeterioratingState`, `CodeBlueState` | Automatically transitions patient state based on vital signs: normal pulse keeps the patient in `StableState`, elevated rates trigger `DeterioratingState`, and sustained critical rates transition to `CodeBlueState`. |
| **8. Strategy** | Pharmacokinetic Weight Dosing | `IDosageStrategy`, `StandardAdultStrategy`, `PediatricStrategy`, `GeriatricRenalStrategy`, `PharmacyDosageCalculator` | Dynamically swaps dosing calculation algorithms at runtime to account for metabolic differences across adult (1.0x), pediatric (0.65x), and geriatric renal-impaired (0.50x) cohorts. |
| **9. Template Method** | Invariant Admission Ingestion | `ClinicalAdmissionProtocol`, `CardiologyAdmission`, `NeurologyAdmission` | Enforces a standardized clinical admission sequence (identity validation, consent, baseline vitals) while allowing specific departments to implement custom diagnostics and bed allocations. |
| **10. Visitor** | Hospital Sterility & Census Auditing | `IHospitalFacility`, `IHospitalVisitor`, `SterilizationAuditVisitor`, `CapacityCensusVisitor`, `OperatingTheater`, `GeneralWard` | Decouples compliance audits from facility classes. Auditors can inspect sterilization expiration and bed occupancy percentages without modifying facility structures. |

---

## Architecture Overview

```text
BehaviouralPatterns/
├── App.config                      # Application configuration
├── BehaviouralPatterns.csproj      # .NET Framework 4.7.2 project file
├── Form1.cs                        # Workbench GUI & event dispatchers
├── Form1.Designer.cs               # UI controls and layout definitions
├── Patterns.cs                     # Domain models and pattern implementations
└── Program.cs                      # WinForms runtime bootstrapper
