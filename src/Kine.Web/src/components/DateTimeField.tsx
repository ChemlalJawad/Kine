import DatePicker, { registerLocale } from 'react-datepicker';
import { fr } from 'date-fns/locale';
import 'react-datepicker/dist/react-datepicker.css';

registerLocale('fr', fr);

type DateTimeFieldProps = {
  value: Date | null;
  onChange: (value: Date | null) => void;
  /** true = date + heure (creneaux, seances); false = date seule (naissance). */
  showTime?: boolean;
  placeholder?: string;
  /** Pas de temps en minutes pour la selection d'heure (defaut 15, aligne sur les creneaux kine). */
  timeIntervals?: number;
  minDate?: Date;
  maxDate?: Date;
};

/**
 * Selecteur de date/heure unifie (remplace les <input type="date|datetime-local">
 * natifs, heterogenes selon les navigateurs) : calendrier localise fr, selection
 * d'heure par pas de 15 min, saisie clavier possible, style aligne sur le theme
 * (voir overrides .react-datepicker dans styles.css).
 * Retourne un vrai objet Date local -- la conversion UTC (toISOString) se fait
 * au moment de l'appel API, ce qui corrige au passage le decalage d'heure de
 * l'ancien format "YYYY-MM-DDTHH:mm" + suffixe "Z" naif.
 */
export function DateTimeField({
  value,
  onChange,
  showTime = false,
  placeholder,
  timeIntervals = 15,
  minDate,
  maxDate
}: DateTimeFieldProps) {
  return (
    <DatePicker
      selected={value}
      onChange={onChange}
      locale="fr"
      showTimeSelect={showTime}
      timeIntervals={timeIntervals}
      timeCaption="Heure"
      dateFormat={showTime ? 'dd/MM/yyyy HH:mm' : 'dd/MM/yyyy'}
      placeholderText={placeholder ?? (showTime ? 'jj/mm/aaaa hh:mm' : 'jj/mm/aaaa')}
      minDate={minDate}
      maxDate={maxDate}
      showMonthDropdown
      showYearDropdown
      dropdownMode="select"
      isClearable
      autoComplete="off"
      className="datefield-input"
      popperPlacement="bottom-start"
    />
  );
}
