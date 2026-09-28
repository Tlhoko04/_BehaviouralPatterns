using System;
using System.Collections.Generic;

namespace MediCoreUI
{
    #region 1. CHAIN OF RESPONSIBILITY
    public class TriageRequest
    {
        public string PatientName { get; }
        public int AcuityLevel { get; }

        public TriageRequest(string patientName, int acuityLevel)
        {
            PatientName = patientName;
            AcuityLevel = acuityLevel;
        }
    }

    public abstract class MedicalOfficer
    {
        protected MedicalOfficer Next;

        public void SetNext(MedicalOfficer nextOfficer)
        {
            Next = nextOfficer;
        }

        public abstract string EvaluateTriage(TriageRequest req);
    }

    public class TriageNurse : MedicalOfficer
    {
        public override string EvaluateTriage(TriageRequest req)
        {
            if (req.AcuityLevel <= 2)
                return $"[Triage Nurse] Treated {req.PatientName} at Outpatient Care (Acuity {req.AcuityLevel}).";
            return Next != null ? Next.EvaluateTriage(req) : "Unhandled triage request.";
        }
    }

    public class AttendingPhysician : MedicalOfficer
    {
        public override string EvaluateTriage(TriageRequest req)
        {
            if (req.AcuityLevel <= 4)
                return $"[Physician] Admitted {req.PatientName} to Urgent Observation (Acuity {req.AcuityLevel}).";
            return Next != null ? Next.EvaluateTriage(req) : "Unhandled triage request.";
        }
    }

    public class ChiefSurgeon : MedicalOfficer
    {
        public override string EvaluateTriage(TriageRequest req)
        {
            if (req.AcuityLevel == 5)
                return $"[Chief Surgeon] Emergency OR Activation! Escalated {req.PatientName} to Surgery.";
            return Next != null ? Next.EvaluateTriage(req) : "Unhandled triage request.";
        }
    }
    #endregion

    #region 2. COMMAND
    public interface IMedicalCommand
    {
        string Execute();
        string Undo();
    }

    public class PatientChartReceiver
    {
        public string PatientId { get; }
        public double ActiveMedicationMg { get; set; }

        public PatientChartReceiver(string id, double initialMg = 0.0)
        {
            PatientId = id;
            ActiveMedicationMg = initialMg;
        }
    }

    public class AdministerMedicationCommand : IMedicalCommand
    {
        private readonly PatientChartReceiver _patient;
        private readonly string _drugName;
        private readonly double _dosageMg;

        public AdministerMedicationCommand(PatientChartReceiver patient, string drugName, double dose)
        {
            _patient = patient;
            _drugName = drugName;
            _dosageMg = dose;
        }

        public string Execute()
        {
            _patient.ActiveMedicationMg += _dosageMg;
            return $"Injected {_dosageMg}mg {_drugName} to {_patient.PatientId}. Active load: {_patient.ActiveMedicationMg}mg.";
        }

        public string Undo()
        {
            _patient.ActiveMedicationMg = Math.Max(0.0, _patient.ActiveMedicationMg - _dosageMg);
            return $"Adverse Reaction Rollback: Antidote neutralized {_dosageMg}mg. Active load restored to: {_patient.ActiveMedicationMg}mg.";
        }
    }

    public class MedicationDeliveryInvoker
    {
        private readonly Stack<IMedicalCommand> _history = new Stack<IMedicalCommand>();

        public string RunCommand(IMedicalCommand command)
        {
            string result = command.Execute();
            _history.Push(command);
            return result;
        }

        public string RollbackLast()
        {
            if (_history.Count > 0)
            {
                IMedicalCommand lastCmd = _history.Pop();
                return lastCmd.Undo();
            }
            return "No administered doses in buffer to reverse.";
        }
    }
    #endregion

    #region 3. ITERATOR
    public class VitalReading
    {
        public DateTime Timestamp { get; } = DateTime.Now;
        public int HeartRateBpm { get; }
        public string BloodPressure { get; }

        public VitalReading(int hr, string bp)
        {
            HeartRateBpm = hr;
            BloodPressure = bp;
        }

        public override string ToString()
        {
            return $"{Timestamp:HH:mm:ss} | HR: {HeartRateBpm,3} BPM | BP: {BloodPressure}";
        }
    }

    public interface IIterator<T>
    {
        bool HasNext();
        T Next();
    }

    public interface IAggregate<T>
    {
        IIterator<T> CreateIterator();
    }

    public class TelemetryMonitor : IAggregate<VitalReading>
    {
        private readonly List<VitalReading> _readings = new List<VitalReading>();

        public void Log(int hr, string bp) => _readings.Add(new VitalReading(hr, bp));

        public IIterator<VitalReading> CreateIterator() => new VitalsIterator(this);

        private class VitalsIterator : IIterator<VitalReading>
        {
            private readonly TelemetryMonitor _monitor;
            private int _index = 0;

            public VitalsIterator(TelemetryMonitor m)
            {
                _monitor = m;
            }

            public bool HasNext() => _index < _monitor._readings.Count;

            public VitalReading Next() => _monitor._readings[_index++];
        }
    }
    #endregion

    #region 4. MEDIATOR
    public interface IHospitalMediator
    {
        string BroadcastNotice(string notice, HospitalUnit sender);
    }

    public abstract class HospitalUnit
    {
        protected IHospitalMediator Mediator;
        public string UnitName { get; }

        protected HospitalUnit(IHospitalMediator m, string name)
        {
            Mediator = m;
            UnitName = name;
        }

        public abstract string Receive(string notice, string fromUnit);
    }

    public class TraumaControlMediator : IHospitalMediator
    {
        private readonly List<HospitalUnit> _units = new List<HospitalUnit>();

        public void Register(HospitalUnit u) => _units.Add(u);

        public string BroadcastNotice(string notice, HospitalUnit sender)
        {
            string log = $"[Dispatch Broadcast by {sender.UnitName}]: {notice}\r\n";
            foreach (var unit in _units)
            {
                if (unit != sender)
                    log += "  " + unit.Receive(notice, sender.UnitName) + "\r\n";
            }
            return log.TrimEnd();
        }
    }

    public class EmergencyRoom : HospitalUnit
    {
        public EmergencyRoom(IHospitalMediator m) : base(m, "Trauma-ER") { }

        public string TriggerTraumaAlarm() => Mediator.BroadcastNotice("Critical inbound collision trauma!", this);

        public override string Receive(string notice, string from) => $"[ER Unit] Acknowledged '{notice}' from {from}.";
    }

    public class BloodBankUnit : HospitalUnit
    {
        public BloodBankUnit(IHospitalMediator m) : base(m, "BloodBank") { }

        public override string Receive(string notice, string from) => $"[Blood Bank] Preparing 4 units of O-Negative universal plasma for {from}.";
    }
    #endregion

    #region 5. MEMENTO
    public class PatientBaselineMemento
    {
        public double OxygenSaturation { get; }
        public int SystolicPressure { get; }

        public PatientBaselineMemento(double o2, int sys)
        {
            OxygenSaturation = o2;
            SystolicPressure = sys;
        }
    }

    public class AnesthesiaProfile
    {
        public string PatientName { get; }
        public double OxygenSaturation { get; set; }
        public int SystolicPressure { get; set; }

        public AnesthesiaProfile(string name, double o2, int sys)
        {
            PatientName = name;
            OxygenSaturation = o2;
            SystolicPressure = sys;
        }

        public PatientBaselineMemento Save() => new PatientBaselineMemento(OxygenSaturation, SystolicPressure);

        public string Revert(PatientBaselineMemento m)
        {
            OxygenSaturation = m.OxygenSaturation;
            SystolicPressure = m.SystolicPressure;
            return $"Reverted to pre-op baseline: SpO2={OxygenSaturation}% | BP={SystolicPressure} mmHg";
        }
    }

    public class AnesthesiologistCaretaker
    {
        private PatientBaselineMemento _snapshot;
        public void Save(PatientBaselineMemento memento) => _snapshot = memento;
        public PatientBaselineMemento GetSnapshot() => _snapshot;
    }
    #endregion

    #region 6. OBSERVER
    public interface IBedsideObserver
    {
        string NotifyVital(string bedId, int hr);
    }

    public class BedsideTelemetryMonitor
    {
        private readonly List<IBedsideObserver> _observers = new List<IBedsideObserver>();
        public string BedId { get; }

        public BedsideTelemetryMonitor(string id) => BedId = id;

        public void Attach(IBedsideObserver obs) => _observers.Add(obs);

        public List<string> SetHeartRate(int hr)
        {
            var logs = new List<string>();
            foreach (var obs in _observers)
            {
                logs.Add(obs.NotifyVital(BedId, hr));
            }
            return logs;
        }
    }

    public class NurseStationPager : IBedsideObserver
    {
        public string NotifyVital(string bed, int hr) => $"[Nurse Pager] {bed} pulse at {hr} BPM.";
    }

    public class CodeBlueSystem : IBedsideObserver
    {
        public string NotifyVital(string bed, int hr) =>
            hr > 140 ? $"[CRASH ALARM] Severe Tachycardia ({hr} BPM) on {bed}! Code Blue activated!" : $"[Telemetry] {bed} status nominal.";
    }
    #endregion

    #region 7. STATE
    public interface IClinicalState
    {
        string EvaluateHeartRate(PatientContext ctx, int hr);
    }

    public class PatientContext
    {
        public IClinicalState State { get; set; } = new StableState();
        public string Evaluate(int hr) => State.EvaluateHeartRate(this, hr);
    }

    public class StableState : IClinicalState
    {
        public string EvaluateHeartRate(PatientContext ctx, int hr)
        {
            if (hr > 115)
            {
                ctx.State = new DeterioratingState();
                return $"HR: {hr} BPM -> Condition shifted! Transitioned to [DeterioratingState].";
            }
            return $"HR: {hr} BPM -> Patient remains in [StableState].";
        }
    }

    public class DeterioratingState : IClinicalState
    {
        public string EvaluateHeartRate(PatientContext ctx, int hr)
        {
            if (hr > 150)
            {
                ctx.State = new CodeBlueState();
                return $"HR: {hr} BPM -> Collapse! Transitioned to [CodeBlueState]!";
            }
            if (hr <= 90)
            {
                ctx.State = new StableState();
                return $"HR: {hr} BPM -> Patient restabilized. Transitioned to [StableState].";
            }
            return $"HR: {hr} BPM -> Ongoing deterioration. Rapid response at bedside.";
        }
    }

    public class CodeBlueState : IClinicalState
    {
        public string EvaluateHeartRate(PatientContext ctx, int hr) =>
            $"HR: {hr} BPM -> Patient is in [CodeBlueState]. Immediate resuscitation in progress.";
    }
    #endregion

    #region 8. STRATEGY
    public interface IDosageStrategy
    {
        double CalculateDose(double baseMg, double weightKg);
    }

    public class StandardAdultStrategy : IDosageStrategy
    {
        public double CalculateDose(double baseMg, double weight) => baseMg * weight;
    }

    public class PediatricStrategy : IDosageStrategy
    {
        public double CalculateDose(double baseMg, double weight) => (baseMg * weight) * 0.65;
    }

    public class GeriatricRenalStrategy : IDosageStrategy
    {
        public double CalculateDose(double baseMg, double weight) => (baseMg * weight) * 0.50;
    }

    public class PharmacyDosageCalculator
    {
        private IDosageStrategy _strategy;

        public PharmacyDosageCalculator(IDosageStrategy strategy) => _strategy = strategy;

        public void SetStrategy(IDosageStrategy strategy) => _strategy = strategy;

        public double Compute(double baseMg, double weight) => _strategy.CalculateDose(baseMg, weight);
    }
    #endregion

    #region 9. TEMPLATE METHOD
    public abstract class ClinicalAdmissionProtocol
    {
        public List<string> Execute()
        {
            var steps = new List<string>
            {
                "Step 1: Patient legal identity and informed consent recorded.",
                "Step 2: Baseline vital signs recorded."
            };
            steps.Add(TargetedDiagnostics());
            steps.Add(AssignBed());
            return steps;
        }

        protected abstract string TargetedDiagnostics();
        protected abstract string AssignBed();
    }

    public class CardiologyAdmission : ClinicalAdmissionProtocol
    {
        protected override string TargetedDiagnostics() => "Step 3 (Cardiology): 12-lead ECG and Troponin blood draw performed.";
        protected override string AssignBed() => "Step 4 (Cardiology): Bed allocated in Cardiac Care Unit (CCU).";
    }

    public class NeurologyAdmission : ClinicalAdmissionProtocol
    {
        protected override string TargetedDiagnostics() => "Step 3 (Neurology): Non-contrast emergency brain CT scan.";
        protected override string AssignBed() => "Step 4 (Neurology): Bed allocated in Acute Stroke Ward.";
    }
    #endregion

    #region 10. VISITOR
    public interface IHospitalVisitor
    {
        string Visit(OperatingTheater theater);
        string Visit(GeneralWard ward);
    }

    public interface IHospitalFacility
    {
        string Accept(IHospitalVisitor visitor);
    }

    public class OperatingTheater : IHospitalFacility
    {
        public int SterilizationHours { get; }
        public OperatingTheater(int hours) => SterilizationHours = hours;
        public string Accept(IHospitalVisitor visitor) => visitor.Visit(this);
    }

    public class GeneralWard : IHospitalFacility
    {
        public int OccupiedBeds { get; }
        public int TotalBeds { get; }
        public GeneralWard(int occupied, int total)
        {
            OccupiedBeds = occupied;
            TotalBeds = total;
        }
        public string Accept(IHospitalVisitor visitor) => visitor.Visit(this);
    }

    public class SterilizationAuditVisitor : IHospitalVisitor
    {
        public string Visit(OperatingTheater t) =>
            t.SterilizationHours <= 12 ? "[Sterility Audit] Theater is sterile and cleared for surgery." : "[Sterility Audit] VIOLATION: Sterilization expired!";

        public string Visit(GeneralWard w) =>
            $"[Sterility Audit] Routine surface checks verified for {w.TotalBeds} beds.";
    }

    public class CapacityCensusVisitor : IHospitalVisitor
    {
        public string Visit(OperatingTheater t) => "[Census Audit] Operating theater is open for acute surgical cases.";
        public string Visit(GeneralWard w) =>
            $"[Census Audit] General Ward Capacity: {(double)w.OccupiedBeds / w.TotalBeds * 100:F0}% ({w.OccupiedBeds}/{w.TotalBeds} beds).";
    }
    #endregion
}