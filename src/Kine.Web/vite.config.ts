import { defineConfig, loadEnv } from 'vite';
import react from '@vitejs/plugin-react';

// Default target for the backend API in local dev. Must match the port used by
// Kine.Api: src/Kine.Api/Properties/launchSettings.json and run-dev.ps1 both
// bind http://localhost:5080. A mismatch here makes every /api call fail with
// ECONNREFUSED, which Vite surfaces to the browser as a 500 on each request.
// Override via VITE_API_PROXY_TARGET in a .env.local if your API runs elsewhere.
const defaultApiProxyTarget = 'http://localhost:5080';

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
