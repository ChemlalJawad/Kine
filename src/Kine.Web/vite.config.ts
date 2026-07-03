import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// Default target for the backend API in local dev (Kine.Api's Kestrel default
// when no launchSettings.json/ASPNETCORE_URLS override port 5000). Override by
// setting VITE_API_PROXY_TARGET in a .env.local if your Kine.Api runs elsewhere.
const defaultApiProxyTarget = 'http://localhost:5000';

export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), '');

  return {
    plugins: [react()],
    server: {
      proxy: {
        // Without this, relative fetch('/api/...') calls resolve against the
        // Vite dev server itself (whatever port it landed on, e.g. 5174 when
        // 5173 was taken) instead of the backend, which 404s and returns
        // Vite's HTML shell -- causing "Unexpected token '<'" JSON parse errors.
        '/api': {
          target: env.VITE_API_PROXY_TARGET || defaultApiProxyTarget,
          changeOrigin: true
        }
      }
    }
  };
});
