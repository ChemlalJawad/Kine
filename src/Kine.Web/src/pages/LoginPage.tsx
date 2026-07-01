import { FormEvent, useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../auth/AuthContext';

type LocationState = {
  from?: string;
};

export function LoginPage() {
  const { isAuthenticated, signIn } = useAuth();
  const location = useLocation();
  const navigate = useNavigate();
  const [email, setEmail] = useState('staff@q-ine.local');
  const [password, setPassword] = useState('');

  const state = location.state as LocationState | null;
  const from = state?.from ?? '/app';

  if (isAuthenticated) {
    return <Navigate to={from} replace />;
  }

  const handleSubmit = (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    signIn(email, password);
    navigate(from, { replace: true });
  };

  return (
    <main className="login-screen">
      <section className="login-card" aria-label="Connexion Q-INE">
        <p className="eyebrow">Q-INE</p>
        <h1>Connexion staff</h1>
        <p className="muted">Acces demo du shell React TypeScript.</p>

        <form className="login-form" onSubmit={handleSubmit}>
          <label>
            Email
            <input value={email} onChange={(event) => setEmail(event.target.value)} type="email" />
          </label>

          <label>
            Mot de passe
            <input
              value={password}
              onChange={(event) => setPassword(event.target.value)}
              type="password"
            />
          </label>

          <button type="submit" className="primary-button">
            Ouvrir la session
          </button>
        </form>
      </section>
    </main>
  );
}
