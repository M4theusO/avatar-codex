import { defineConfig } from 'cypress'

// Os testes E2E rodam contra o app real: Vite em 5173 e API em 5080, com um banco próprio
// (avatar_codex_e2e). Passo a passo no README, seção "Testes E2E".
export default defineConfig({
  e2e: {
    baseUrl: 'http://localhost:5173',
    expose: { apiUrl: 'http://localhost:5080' },
    viewportWidth: 1280,
    viewportHeight: 900,
    defaultCommandTimeout: 8000,
    video: false,
    screenshotOnRunFailure: true,
  },
})
