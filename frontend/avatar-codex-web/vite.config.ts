import react from '@vitejs/plugin-react'
import { defineConfig } from 'vitest/config'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    strictPort: true,
  },
  test: {
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    // tokens.css é processado para que tokens.test.ts leia os valores reais (?raw).
    css: { include: [/tokens\.css/], modules: { classNameStrategy: 'non-scoped' } },
  },
})
