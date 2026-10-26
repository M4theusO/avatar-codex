import '@testing-library/jest-dom/vitest'
import { cleanup } from '@testing-library/react'
import { afterEach, vi } from 'vitest'

// jsdom não implementa rolagem.
window.scrollTo = vi.fn() as typeof window.scrollTo

afterEach(() => {
  cleanup()
  vi.clearAllMocks()
})
