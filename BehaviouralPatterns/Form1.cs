using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MediCoreUI
{
    public partial class Form1 : Form
    {
        private Panel pnlNavigation;
        private Panel pnlWorkbench;
        private TextBox txtAuditLog;

        private readonly PatientChartReceiver _commandPatient = new PatientChartReceiver("PAT-701");
        private readonly MedicationDeliveryInvoker _medicationInvoker = new MedicationDeliveryInvoker();

        private readonly AnesthesiaProfile _mementoPatient = new AnesthesiaProfile("David Malan", 99.0, 120);
        private readonly AnesthesiologistCaretaker _anesthesiaCaretaker = new AnesthesiologistCaretaker();

        private readonly PatientContext _stateContext = new PatientContext();

        public Form1()
        {
            InitializeLayout();
            LoadChainOfResponsibilityWorkbench();
        }

        private void InitializeLayout()
        {
            Text = "MediCore - Behavioral Design Patterns Operations Center";
            Size = new Size(1100, 720);
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9.5f, FontStyle.Regular);

            // Left Navigation Panel
            pnlNavigation = new Panel { Dock = DockStyle.Left, Width = 230, BackColor = Color.FromArgb(24, 30, 42) };

            Label lblNavTitle = new Label
            {
                Text = "BEHAVIORAL SUITE",
                ForeColor = Color.WhiteSmoke,
                Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            pnlNavigation.Controls.Add(lblNavTitle);

            string[] patterns = {
                "1. Chain of Responsibility", "2. Command (Undo)", "3. Iterator",
                "4. Mediator", "5. Memento", "6. Observer",
                "7. State", "8. Strategy", "9. Template Method", "10. Visitor"
            };

            for (int i = patterns.Length - 1; i >= 0; i--)
            {
                Button btn = new Button
                {
                    Text = patterns[i],
                    Dock = DockStyle.Top,
                    Height = 42,
                    FlatStyle = FlatStyle.Flat,
                    ForeColor = Color.WhiteSmoke,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Tag = i + 1
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.Click += NavButton_Click;
                pnlNavigation.Controls.Add(btn);
            }

            // Lower Audit Log
            GroupBox grpLog = new GroupBox
            {
                Text = "Pattern Event Execution & Architectural Audit Feed",
                Dock = DockStyle.Bottom,
                Height = 220
            };
            txtAuditLog = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Dock = DockStyle.Fill,
                Font = new Font("Consolas", 9f),
                BackColor = Color.FromArgb(18, 22, 28),
                ForeColor = Color.LightGreen
            };
            grpLog.Controls.Add(txtAuditLog);

            // Center Dynamic Workbench Panel
            pnlWorkbench = new Panel { Dock = DockStyle.Fill, Padding = new Padding(20) };

            Controls.Add(pnlWorkbench);
            Controls.Add(grpLog);
            Controls.Add(pnlNavigation);
        }

        private void NavButton_Click(object sender, EventArgs e)
        {
            Button btn = sender as Button;
            if (btn != null && btn.Tag is int)
            {
                int patternId = (int)btn.Tag;
                pnlWorkbench.Controls.Clear();
                switch (patternId)
                {
                    case 1: LoadChainOfResponsibilityWorkbench(); break;
                    case 2: LoadCommandWorkbench(); break;
                    case 3: LoadIteratorWorkbench(); break;
                    case 4: LoadMediatorWorkbench(); break;
                    case 5: LoadMementoWorkbench(); break;
                    case 6: LoadObserverWorkbench(); break;
                    case 7: LoadStateWorkbench(); break;
                    case 8: LoadStrategyWorkbench(); break;
                    case 9: LoadTemplateMethodWorkbench(); break;
                    case 10: LoadVisitorWorkbench(); break;
                }
            }
        }

        private void Log(string message)
        {
            txtAuditLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\r\n");
        }

        #region WORKBENCH BUILDERS

        private void LoadChainOfResponsibilityWorkbench()
        {
            Label lbl = new Label { Text = "Chain of Responsibility: Triage Acuity Escalation Ladder", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblName = new Label { Text = "Patient Name:", Location = new Point(25, 70), AutoSize = true };
            TextBox txtName = new TextBox { Text = "Sarah Connor", Location = new Point(130, 68), Width = 160 };

            Label lblAcuity = new Label { Text = "Acuity Level (1-5):", Location = new Point(25, 110), AutoSize = true };
            NumericUpDown numAcuity = new NumericUpDown { Minimum = 1, Maximum = 5, Value = 3, Location = new Point(160, 108), Width = 60 };

            Button btnSubmit = new Button { Text = "Submit to Triage Chain", Location = new Point(25, 150), Size = new Size(220, 35) };
            btnSubmit.Click += (s, e) =>
            {
                var nurse = new TriageNurse();
                var physician = new AttendingPhysician();
                var surgeon = new ChiefSurgeon();
                nurse.SetNext(physician);
                physician.SetNext(surgeon);

                string result = nurse.EvaluateTriage(new TriageRequest(txtName.Text, (int)numAcuity.Value));
                Log(result);
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblName, txtName, lblAcuity, numAcuity, btnSubmit });
        }

        private void LoadCommandWorkbench()
        {
            Label lbl = new Label { Text = "Command Pattern: Full Invoker Execution & Undo Pipeline", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblStatus = new Label { Text = $"Patient {_commandPatient.PatientId} Active Drug Load: {_commandPatient.ActiveMedicationMg} mg", Location = new Point(25, 60), AutoSize = true };

            Button btnAdminister = new Button { Text = "Invoker.RunCommand: Inject 15mg Morphine", Location = new Point(25, 100), Size = new Size(280, 35) };
            btnAdminister.Click += (s, e) =>
            {
                var cmd = new AdministerMedicationCommand(_commandPatient, "Morphine", 15.0);
                string res = _medicationInvoker.RunCommand(cmd);
                lblStatus.Text = $"Patient {_commandPatient.PatientId} Active Drug Load: {_commandPatient.ActiveMedicationMg} mg";
                Log(res);
            };

            Button btnUndo = new Button { Text = "Invoker.RollbackLast: Neutralize Antidote", Location = new Point(320, 100), Size = new Size(280, 35) };
            btnUndo.Click += (s, e) =>
            {
                string res = _medicationInvoker.RollbackLast();
                lblStatus.Text = $"Patient {_commandPatient.PatientId} Active Drug Load: {_commandPatient.ActiveMedicationMg} mg";
                Log(res);
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblStatus, btnAdminister, btnUndo });
        }

        private void LoadIteratorWorkbench()
        {
            Label lbl = new Label { Text = "Iterator: Traversal via IAggregate<T> Implementation", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Button btnRun = new Button { Text = "Iterate Next Telemetry Batch", Location = new Point(25, 70), Size = new Size(240, 35) };
            btnRun.Click += (s, e) =>
            {
                IAggregate<VitalReading> monitor = new TelemetryMonitor();
                ((TelemetryMonitor)monitor).Log(72, "120/80");
                ((TelemetryMonitor)monitor).Log(88, "130/85");
                ((TelemetryMonitor)monitor).Log(115, "150/95");

                IIterator<VitalReading> iterator = monitor.CreateIterator();
                Log("Starting Iterator traversal through private collection:");
                while (iterator.HasNext())
                {
                    Log("  " + iterator.Next().ToString());
                }
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, btnRun });
        }

        private void LoadMediatorWorkbench()
        {
            Label lbl = new Label { Text = "Mediator: Trauma Hub Coordinating Hospital Wings", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Button btnAlarm = new Button { Text = "Trigger Multi-Vehicle Collision Dispatch", Location = new Point(25, 70), Size = new Size(300, 40) };
            btnAlarm.Click += (s, e) =>
            {
                var hub = new TraumaControlMediator();
                var er = new EmergencyRoom(hub);
                var bloodBank = new BloodBankUnit(hub);
                hub.Register(er);
                hub.Register(bloodBank);

                string trace = er.TriggerTraumaAlarm();
                Log(trace);
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, btnAlarm });
        }

        private void LoadMementoWorkbench()
        {
            Label lbl = new Label { Text = "Memento: Encapsulated Snapshot with AnesthesiologistCaretaker", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblStatus = new Label { Text = $"Telemetry: {_mementoPatient.PatientName} | SpO2: {_mementoPatient.OxygenSaturation}% | BP: {_mementoPatient.SystolicPressure} mmHg", Location = new Point(25, 60), AutoSize = true };

            Button btnSnapshot = new Button { Text = "Caretaker.Save: Pre-Op Snapshot", Location = new Point(25, 100), Size = new Size(230, 35) };
            btnSnapshot.Click += (s, e) =>
            {
                _anesthesiaCaretaker.Save(_mementoPatient.Save());
                Log($"Caretaker archived Memento: Baseline saved at SpO2={_mementoPatient.OxygenSaturation}%");
            };

            Button btnCrash = new Button { Text = "Simulate Drop (SpO2=82%)", Location = new Point(270, 100), Size = new Size(200, 35) };
            btnCrash.Click += (s, e) =>
            {
                _mementoPatient.OxygenSaturation = 82.0;
                _mementoPatient.SystolicPressure = 75;
                lblStatus.Text = $"Telemetry: {_mementoPatient.PatientName} | SpO2: {_mementoPatient.OxygenSaturation}% | BP: {_mementoPatient.SystolicPressure} mmHg";
                Log("Patient dropped: Acute surgical hypoxia detected!");
            };

            Button btnRevert = new Button { Text = "Caretaker.Restore: Revert State", Location = new Point(485, 100), Size = new Size(230, 35) };
            btnRevert.Click += (s, e) =>
            {
                var snapshot = _anesthesiaCaretaker.GetSnapshot();
                if (snapshot != null)
                {
                    string res = _mementoPatient.Revert(snapshot);
                    lblStatus.Text = $"Telemetry: {_mementoPatient.PatientName} | SpO2: {_mementoPatient.OxygenSaturation}% | BP: {_mementoPatient.SystolicPressure} mmHg";
                    Log(res);
                }
                else
                {
                    Log("No baseline memento archived in Caretaker.");
                }
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblStatus, btnSnapshot, btnCrash, btnRevert });
        }

        private void LoadObserverWorkbench()
        {
            Label lbl = new Label { Text = "Observer: Real-Time Telemetry Pager & Alarm Broadcaster", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblHr = new Label { Text = "Simulate Heart Rate (BPM):", Location = new Point(25, 70), AutoSize = true };
            NumericUpDown numHr = new NumericUpDown { Minimum = 40, Maximum = 200, Value = 80, Location = new Point(210, 68), Width = 80 };

            Button btnBroadcast = new Button { Text = "Broadcast Heart Rate Update", Location = new Point(25, 110), Size = new Size(265, 35) };
            btnBroadcast.Click += (s, e) =>
            {
                var monitor = new BedsideTelemetryMonitor("BED-ICU-02");
                monitor.Attach(new NurseStationPager());
                monitor.Attach(new CodeBlueSystem());

                var alerts = monitor.SetHeartRate((int)numHr.Value);
                foreach (var a in alerts) Log(a);
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblHr, numHr, btnBroadcast });
        }

        private void LoadStateWorkbench()
        {
            Label lbl = new Label { Text = "State: Patient Clinical Acuity Lifecycle", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblState = new Label { Text = $"Current State: {_stateContext.State.GetType().Name}", Location = new Point(25, 60), AutoSize = true };

            Button btnStable = new Button { Text = "Vitals: Normal 75 BPM", Location = new Point(25, 100), Size = new Size(180, 35) };
            btnStable.Click += (s, e) =>
            {
                Log(_stateContext.Evaluate(75));
                lblState.Text = $"Current State: {_stateContext.State.GetType().Name}";
            };

            Button btnDeteriorate = new Button { Text = "Vitals: Elevate 125 BPM", Location = new Point(215, 100), Size = new Size(180, 35) };
            btnDeteriorate.Click += (s, e) =>
            {
                Log(_stateContext.Evaluate(125));
                lblState.Text = $"Current State: {_stateContext.State.GetType().Name}";
            };

            Button btnCodeBlue = new Button { Text = "Vitals: Critical 160 BPM", Location = new Point(405, 100), Size = new Size(180, 35) };
            btnCodeBlue.Click += (s, e) =>
            {
                Log(_stateContext.Evaluate(160));
                lblState.Text = $"Current State: {_stateContext.State.GetType().Name}";
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblState, btnStable, btnDeteriorate, btnCodeBlue });
        }

        private void LoadStrategyWorkbench()
        {
            Label lbl = new Label { Text = "Strategy: Dynamic Physiological Drug Dosing", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Label lblWeight = new Label { Text = "Patient Weight (kg):", Location = new Point(25, 65), AutoSize = true };
            NumericUpDown numWeight = new NumericUpDown { Value = 70, Minimum = 5, Maximum = 200, Location = new Point(170, 63), Width = 70 };

            ComboBox cmbStrategy = new ComboBox { Location = new Point(25, 105), Width = 215, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbStrategy.Items.AddRange(new object[] { "Standard Adult (1.0x)", "Pediatric (0.65x)", "Geriatric Renal (0.50x)" });
            cmbStrategy.SelectedIndex = 0;

            Button btnCalc = new Button { Text = "Compute Dose", Location = new Point(250, 103), Size = new Size(130, 28) };
            btnCalc.Click += (s, e) =>
            {
                IDosageStrategy strategy;
                switch (cmbStrategy.SelectedIndex)
                {
                    case 1:
                        strategy = new PediatricStrategy();
                        break;
                    case 2:
                        strategy = new GeriatricRenalStrategy();
                        break;
                    default:
                        strategy = new StandardAdultStrategy();
                        break;
                }

                var calculator = new PharmacyDosageCalculator(strategy);
                double dose = calculator.Compute(2.5, (double)numWeight.Value);
                Log($"[Strategy: {strategy.GetType().Name}] Calculated Dose: {dose:F1} mg (Base: 2.5mg/kg on {numWeight.Value}kg)");
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, lblWeight, numWeight, cmbStrategy, btnCalc });
        }

        private void LoadTemplateMethodWorkbench()
        {
            Label lbl = new Label { Text = "Template Method: Standard Invariant Admission Protocols", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Button btnCardio = new Button { Text = "Execute Cardiology Admission", Location = new Point(25, 70), Size = new Size(240, 35) };
            btnCardio.Click += (s, e) =>
            {
                Log("Running Cardiology Admission Protocol:");
                ClinicalAdmissionProtocol cardio = new CardiologyAdmission();
                foreach (var step in cardio.Execute()) Log("  " + step);
            };

            Button btnNeuro = new Button { Text = "Execute Neurology Admission", Location = new Point(280, 70), Size = new Size(240, 35) };
            btnNeuro.Click += (s, e) =>
            {
                Log("Running Neurology Admission Protocol:");
                ClinicalAdmissionProtocol neuro = new NeurologyAdmission();
                foreach (var step in neuro.Execute()) Log("  " + step);
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, btnCardio, btnNeuro });
        }

        private void LoadVisitorWorkbench()
        {
            Label lbl = new Label { Text = "Visitor: Inspections Over Hospital Rooms & Facilities", Font = new Font("Segoe UI", 12, FontStyle.Bold), AutoSize = true, Location = new Point(20, 20) };
            Button btnSterility = new Button { Text = "Run Sterilization Audit Visitor", Location = new Point(25, 70), Size = new Size(250, 35) };
            btnSterility.Click += (s, e) =>
            {
                var facilities = new List<IHospitalFacility> { new OperatingTheater(4), new GeneralWard(28, 30) };
                var visitor = new SterilizationAuditVisitor();
                Log("Running Sterility Visitor across facility portfolio:");
                foreach (var f in facilities) Log("  " + f.Accept(visitor));
            };

            Button btnCensus = new Button { Text = "Run Capacity Census Visitor", Location = new Point(290, 70), Size = new Size(250, 35) };
            btnCensus.Click += (s, e) =>
            {
                var facilities = new List<IHospitalFacility> { new OperatingTheater(4), new GeneralWard(28, 30) };
                var visitor = new CapacityCensusVisitor();
                Log("Running Census Visitor across facility portfolio:");
                foreach (var f in facilities) Log("  " + f.Accept(visitor));
            };

            pnlWorkbench.Controls.AddRange(new Control[] { lbl, btnSterility, btnCensus });
        }

        #endregion
    }
}