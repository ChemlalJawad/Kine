import { ReactNode, useEffect } from 'react';

type ModalProps = {
  title: string;
  onClose: () => void;
  children: ReactNode;
};

/**
 * Lightweight popup used for creation forms (patient, creneau, rendez-vous)
 * so they no longer sit inline against the panel/list they belong to.
 * Closes on overlay click or Escape.
 */
export function Modal({ title, onClose, children }: ModalProps) {
  useEffect(() => {
    const handleKeyDown = (event: KeyboardEvent) => {
      if (event.key === 'Escape') {
        onClose();
      }
    };
    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  return (
    <div className="modal-overlay" onClick={onClose}>
      <div className="modal-panel panel" onClick={(event) => event.stopPropagation()}>
        <div className="dossier-header">
          <h3 style={{ margin: 0 }}>{title}</h3>
          <button className="ghost-button" type="button" onClick={onClose} aria-label="Fermer">
            Fermer
          </button>
        </div>
        {children}
      </div>
    </div>
  );
}
