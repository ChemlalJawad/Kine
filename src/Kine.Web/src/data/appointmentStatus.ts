/**
 * Shared label/badge mapping for Appointment.status (0=Scheduled, 1=Cancelled,
 * 2=NoShow, 3=Completed), used by both DashboardPage and AgendaPage so the
 * wording/colors stay in sync in one place. Labels match the mockup's
 * terminology (Confirmé/Annulé/Terminé) rather than a literal enum-name
 * translation.
 */
export const appointmentStatusLabels: Record<number, string> = {
  0: 'Confirmé',
  1: 'Annulé',
  2: 'No-show',
  3: 'Terminé'
};

export const appointmentStatusBadgeClass: Record<number, string> = {
  0: 'badge badge-success',
  1: 'badge badge-danger',
  2: 'badge badge-warning',
  3: 'badge badge-neutral'
};
