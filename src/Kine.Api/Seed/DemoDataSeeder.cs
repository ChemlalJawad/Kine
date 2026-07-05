using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Billing.Application;
using Kine.Modules.Billing.Domain;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Domain;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Domain;
using Kine.Modules.Reimbursement.Application;
using Kine.Modules.Reimbursement.Domain;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Domain;

namespace Kine.Api.Seed;

/// <summary>
/// Seeds the in-memory stores with demo data for local development, so the
/// staff UI shows realistic content on first run instead of empty screens.
/// Skips seeding if data already exists; becomes a no-op once real
/// persistence (PostgreSQL) is wired in behind the same store interfaces.
/// </summary>
public static class DemoDataSeeder
{
    private const string TenantId = "tenant-demo";
    private const string SeedActorId = "staff-1";

    public static void Seed(
        IPatientStore patientStore,
        ISchedulingStore schedulingStore,
        IInvoiceStore invoiceStore,
        ISeanceStore seanceStore,
        IPrescriptionStore prescriptionStore,
        IReimbursementCaseStore reimbursementStore)
    {
        if (patientStore.GetAll(TenantId).Count > 0)
        {
            return;
        }

        var patients = SeedPatients(patientStore);
        var practitioners = SeedPractitioners(schedulingStore);
        SeedScheduling(schedulingStore, patients, practitioners);
        var invoices = SeedBilling(invoiceStore, patients);
        var prescriptions = SeedPrescriptions(prescriptionStore, patients);
        SeedClinical(seanceStore, patients, prescriptions);
        SeedReimbursement(reimbursementStore, invoices);
    }

    private static IReadOnlyList<Practitioner> SeedPractitioners(ISchedulingStore store)
    {
        // F-B2 multi-praticiens : deux kines de demonstration; les slots/rdv du
        // jour sont repartis entre eux pour que le filtre Agenda ait du contenu.
        var now = DateTime.UtcNow;
        var practitioners = new[]
        {
            new Practitioner
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                FirstName = "Julie",
                LastName = "Peeters",
                InamiNumber = "5-12345-67-890",
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                CreatedBy = SeedActorId
            },
            new Practitioner
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                FirstName = "Thomas",
                LastName = "Dujardin",
                InamiNumber = null,
                CreatedAtUtc = now,
                UpdatedAtUtc = now,
                CreatedBy = SeedActorId
            }
        };

        foreach (var practitioner in practitioners)
        {
            store.AddPractitioner(practitioner);
        }

        return practitioners;
    }

    private static IReadOnlyList<Patient> SeedPatients(IPatientStore store)
    {
        var now = DateTime.UtcNow;

        var sophie = NewPatient(
            store, "Sophie", "Vandenberghe", new DateOnly(1985, 3, 14), PatientStatus.Active, now.AddDays(-120),
            mutuelle: "Mutualite Chretienne", diagnosis: "Lombalgie chronique", sessionsPrescribed: 18, sessionsDone: 8);
        var marc = NewPatient(
            store, "Marc", "Lemmens", new DateOnly(1972, 11, 2), PatientStatus.Active, now.AddDays(-90),
            mutuelle: "Solidaris", diagnosis: "Reeducation post-operatoire genou", sessionsPrescribed: 20, sessionsDone: 4);
        var amina = NewPatient(
            store, "Amina", "El Amrani", new DateOnly(1990, 7, 19), PatientStatus.Active, now.AddDays(-40),
            mutuelle: "Partena", diagnosis: "Kinesitherapie respiratoire", sessionsPrescribed: 9, sessionsDone: 2);
        var louis = NewPatient(
            store, "Louis", "Dupont", new DateOnly(1958, 1, 5), PatientStatus.Archived, now.AddDays(-400),
            mutuelle: "Helan", diagnosis: "Reeducation epaule", sessionsPrescribed: 18, sessionsDone: 18);

        AddContact(store, sophie, PatientContactType.Phone, "0472 55 12 34", isPrimary: true);
        AddContact(store, sophie, PatientContactType.Email, "sophie.vandenberghe@example.be", isPrimary: false);
        AddContact(store, marc, PatientContactType.Phone, "0485 09 88 21", isPrimary: true);
        AddContact(store, amina, PatientContactType.Phone, "0493 44 76 02", isPrimary: true);
        AddContact(store, louis, PatientContactType.Phone, "0471 20 65 90", isPrimary: true);

        AddConsent(store, sophie, ConsentType.TraitementDonnees);
        AddConsent(store, marc, ConsentType.TraitementDonnees);
        AddConsent(store, amina, ConsentType.TraitementDonnees);
        AddConsent(store, amina, ConsentType.Communication);
        AddConsent(store, louis, ConsentType.TraitementDonnees);

        return new[] { sophie, marc, amina, louis };
    }

    private static void SeedScheduling(ISchedulingStore store, IReadOnlyList<Patient> patients, IReadOnlyList<Practitioner> practitioners)
    {
        var sophie = patients[0];
        var marc = patients[1];
        var amina = patients[2];
        var louis = patients[3];
        var julie = practitioners[0].Id.ToString();
        var thomas = practitioners[1].Id.ToString();

        var today = DateTime.UtcNow.Date;

        // Today's schedule: a mix of booked/free slots and appointment statuses,
        // so the dashboard's "Planning du jour" and the Agenda both have content
        // immediately, without waiting on a first manual booking.
        var slot0900 = NewSlot(store, julie, today.AddHours(9), today.AddHours(9.5), isBooked: true);
        var slot0930 = NewSlot(store, thomas, today.AddHours(9.5), today.AddHours(10), isBooked: true);
        NewSlot(store, julie, today.AddHours(10.25), today.AddHours(10.75), isBooked: false);
        var slot1045 = NewSlot(store, thomas, today.AddHours(10.75), today.AddHours(11.25), isBooked: true);
        var slot1400 = NewSlot(store, julie, today.AddHours(14), today.AddHours(14.5), isBooked: true);
        NewSlot(store, thomas, today.AddHours(15), today.AddHours(15.5), isBooked: false);

        NewAppointment(store, sophie, slot0900, AppointmentStatus.Scheduled);
        NewAppointment(store, marc, slot0930, AppointmentStatus.Scheduled);
        NewAppointment(store, amina, slot1045, AppointmentStatus.Scheduled);
        NewAppointment(store, louis, slot1400, AppointmentStatus.Cancelled);

        // Completed sessions in the past, so every seeded patient has a real "derniere seance"
        // (Kine.Web/PatientsPage derives this from Completed appointments, it is not a stored
        // field on Patient -- see SPEC/11-open-questions.md Q-B20). Dates loosely match the
        // design mockup's example values (Sophie/Marc/Amina/Louis).
        var slotSophie = NewSlot(store, julie, today.AddDays(-1).AddHours(9), today.AddDays(-1).AddHours(9.5), isBooked: true);
        NewAppointment(store, sophie, slotSophie, AppointmentStatus.Completed);

        var slotMarc = NewSlot(store, thomas, today.AddDays(-2).AddHours(11), today.AddDays(-2).AddHours(11.5), isBooked: true);
        NewAppointment(store, marc, slotMarc, AppointmentStatus.Completed);

        var slotAmina = NewSlot(store, julie, today.AddDays(-7).AddHours(15), today.AddDays(-7).AddHours(15.5), isBooked: true);
        NewAppointment(store, amina, slotAmina, AppointmentStatus.Completed);

        var slotLouis = NewSlot(store, julie, today.AddDays(-51).AddHours(10), today.AddDays(-51).AddHours(10.5), isBooked: true);
        NewAppointment(store, louis, slotLouis, AppointmentStatus.Completed);
    }

    private static IReadOnlyList<Invoice> SeedBilling(IInvoiceStore store, IReadOnlyList<Patient> patients)
    {
        var sophie = patients[0];
        var marc = patients[1];
        var amina = patients[2];
        var louis = patients[3];

        // Mirrors the Q-INE Redesign mockup rows so the Facturation page and the
        // dashboard KPIs (CA du mois, remboursements en attente) show realistic
        // content on first run. Codes/montants come from ActeInamiCatalog.
        return new[]
        {
            NewInvoice(store, sophie, "558014", InvoiceStatus.Reimbursed),
            NewInvoice(store, marc, "558310", InvoiceStatus.Pending),
            NewInvoice(store, amina, "558891", InvoiceStatus.Pending),
            NewInvoice(store, louis, "558014", InvoiceStatus.Rejected)
        };
    }

    private static IReadOnlyDictionary<Guid, Prescription> SeedPrescriptions(IPrescriptionStore store, IReadOnlyList<Patient> patients)
    {
        // F-A4 : une prescription active par patient actif, alignee sur le nombre
        // de seances prescrites du dossier, pour que la section Prescriptions et
        // les alertes (expiration/quota) aient du contenu de demonstration.
        var now = DateTime.UtcNow;
        var byPatient = new Dictionary<Guid, Prescription>();
        var prescribers = new[] { "Dr Anne Willems", "Dr Pierre Marchal", "Dr Sofia Rossi" };
        var index = 0;

        foreach (var patient in patients)
        {
            if (patient.Status != PatientStatus.Active || patient.SessionsPrescribed <= 0)
            {
                continue;
            }

            var prescribedAt = now.AddDays(-30);
            var prescription = new Prescription
            {
                Id = Guid.NewGuid(),
                TenantId = TenantId,
                PatientId = patient.Id,
                PrescriberName = prescribers[index++ % prescribers.Length],
                PrescriberInami = null,
                PrescribedAtUtc = prescribedAt,
                ValidUntilUtc = prescribedAt.AddMonths(2),
                Diagnosis = patient.Diagnosis,
                SessionsPrescribed = patient.SessionsPrescribed,
                CreatedAtUtc = now,
                CreatedBy = SeedActorId
            };

            store.Add(prescription);
            byPatient[patient.Id] = prescription;
        }

        return byPatient;
    }

    private static void SeedClinical(ISeanceStore store, IReadOnlyList<Patient> patients, IReadOnlyDictionary<Guid, Prescription> prescriptions)
    {
        // Q-B20 resolu: les seances sont des enregistrements reels. Le seed cree
        // autant de seances que l'ancien compteur SessionsDone de chaque patient
        // (espacees d'une semaine en remontant depuis hier), pour que la barre de
        // progression du dossier patient (derivee du nombre de seances) affiche
        // les memes valeurs que le mockup.
        var now = DateTime.UtcNow;
        foreach (var patient in patients)
        {
            for (var i = 0; i < patient.SessionsDone; i++)
            {
                store.Add(new SeanceClinique
                {
                    Id = Guid.NewGuid(),
                    TenantId = TenantId,
                    PatientId = patient.Id,
                    AppointmentId = null,
                    PrescriptionId = prescriptions.TryGetValue(patient.Id, out var prescription) ? prescription.Id : null,
                    DateSeanceUtc = now.AddDays(-1).AddDays(-7 * i),
                    Note = i == 0 ? "Seance de suivi (demo)" : null,
                    CreatedAtUtc = now,
                    CreatedBy = SeedActorId
                });
            }
        }
    }

    private static void SeedReimbursement(IReimbursementCaseStore store, IReadOnlyList<Invoice> invoices)
    {
        // Un dossier de demonstration au statut Draft regroupant les factures en
        // attente, pour que la section "Dossiers de remboursement" ne soit pas vide.
        var pendingInvoiceIds = invoices
            .Where(invoice => invoice.Status == InvoiceStatus.Pending)
            .Select(invoice => invoice.Id)
            .ToList();

        if (pendingInvoiceIds.Count == 0)
        {
            return;
        }

        var now = DateTime.UtcNow;
        store.Add(new ReimbursementCase
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            InvoiceIds = pendingInvoiceIds,
            Status = ReimbursementCaseStatus.Draft,
            SubmissionRef = null,
            InamiResponse = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        });
    }

    private static Invoice NewInvoice(IInvoiceStore store, Patient patient, string codeInami, InvoiceStatus status)
    {
        var acte = ActeInamiCatalog.Find(codeInami)
            ?? throw new InvalidOperationException($"Seed references unknown INAMI code '{codeInami}'.");

        var now = DateTime.UtcNow;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PatientId = patient.Id,
            CodeInami = acte.Code,
            Label = acte.Label,
            Amount = acte.Amount,
            Mutuelle = patient.Mutuelle,
            Status = status,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        };

        store.Add(invoice);
        return invoice;
    }

    private static Patient NewPatient(
        IPatientStore store,
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        PatientStatus status,
        DateTime createdAtUtc,
        string? mutuelle = null,
        string? diagnosis = null,
        int sessionsPrescribed = 0,
        int sessionsDone = 0)
    {
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Status = status,
            Mutuelle = mutuelle,
            Diagnosis = diagnosis,
            SessionsPrescribed = sessionsPrescribed,
            SessionsDone = sessionsDone,
            CreatedAtUtc = createdAtUtc,
            UpdatedAtUtc = createdAtUtc,
            CreatedBy = SeedActorId
        };

        store.Add(patient);
        return patient;
    }

    private static void AddContact(IPatientStore store, Patient patient, PatientContactType type, string value, bool isPrimary)
    {
        var now = DateTime.UtcNow;
        store.AddContact(new PatientContact
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PatientId = patient.Id,
            Type = type,
            Value = value,
            IsPrimary = isPrimary,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        });
    }

    private static void AddConsent(IPatientStore store, Patient patient, ConsentType type)
    {
        var now = DateTime.UtcNow;
        store.AddConsent(new PatientConsent
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PatientId = patient.Id,
            Type = type,
            Granted = true,
            GrantedAtUtc = now,
            RevokedAtUtc = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        });
    }

    private static PractitionerSlot NewSlot(ISchedulingStore store, string practitionerId, DateTime startAtUtc, DateTime endAtUtc, bool isBooked)
    {
        var now = DateTime.UtcNow;
        var slot = new PractitionerSlot
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PractitionerId = practitionerId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            IsBooked = isBooked,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        };

        store.AddSlot(slot);
        return slot;
    }

    private static void NewAppointment(ISchedulingStore store, Patient patient, PractitionerSlot slot, AppointmentStatus status)
    {
        var now = DateTime.UtcNow;
        store.AddAppointment(new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = TenantId,
            PatientId = patient.Id,
            PractitionerId = slot.PractitionerId,
            SlotId = slot.Id,
            StartAtUtc = slot.StartAtUtc,
            EndAtUtc = slot.EndAtUtc,
            Status = status,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SeedActorId
        });
    }
}
