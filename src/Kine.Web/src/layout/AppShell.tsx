import { NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

const navItems = [
  { to: '/app', label: 'Tableau de bord', end: true },
  { to: '/app/agenda', label: 'Agenda' },
  { to: '/app/patients', label: 'Patients' },
  { to: '/app/facturation', label: 'Facturation' },
  { to: '/app/reporting', label: 'Reporting' }
];

export function AppShell() {
  const { user, signOut } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const titles: Record<string, { eyebrow: string; title: string }> = {
    '/app': { eyebrow: 'Vue d’ensemble', title: `Bonjour, ${user?.displayName ?? 'staff'}` },
    '/app/agenda': { eyebrow: 'Module Agenda', title: 'Rendez-vous' },
    '/app/patients': { eyebrow: 'Module Patients', title: 'Dossiers patients' },
    '/app/facturation': { eyebrow: 'Module Facturation', title: 'Remboursements' },
    '/app/reporting': { eyebrow: 'Module Reporting', title: 'Activite du cabinet' }
  };
  const page = titles[location.pathname] ?? titles['/app'];

  const handleSignOut = () => {
    signOut();
    navigate('/login', { replace: true });
  };

  return (
    <div className="app-shell">
      <aside className="sidebar">
        <div className="sidebar-top">
          <div>
            <p className="eyebrow">Q-INE</p>
            <h1 className="brand">Cabinet staff</h1>
          </div>

          <nav className="nav">
            {navItems.map((item) => (
              <NavLink
                key={item.to}
                to={item.to}
                end={item.end}
                className={({ isActive }) => (isActive ? 'nav-link active' : 'nav-link')}
              >
                {item.label}
              </NavLink>
            ))}
          </nav>
        </div>

        <div className="staff-card">
          <span className="staff-card-name">{user?.displayName ?? 'Staff Demo'}</span>
          <span className="staff-card-meta">
            {user?.tenantId} &middot; {user?.actorId}
          </span>
        </div>
      </aside>

      <div className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">{page.eyebrow}</p>
            <h2 className="page-title">{page.title}</h2>
          </div>

          <button type="button" className="ghost-button" onClick={handleSignOut}>
            Se deconnecter
          </button>
        </header>

        <main className="content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
