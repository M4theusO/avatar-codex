import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render } from '@testing-library/react'
import { createMemoryRouter, RouterProvider } from 'react-router'
import AuthProvider from '../contexts/AuthProvider'
import LevelUpProvider from '../contexts/LevelUpProvider'
import ToastProvider from '../contexts/ToastProvider'
import { routes } from '../router'

/**
 * Renderiza o app completo (providers, layout e rotas reais) em uma URL inicial.
 * A sessão vem do mock de `authService` (ver setup.ts e `mockSession`).
 */
export function renderRoute(url: string) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  const router = createMemoryRouter(routes, { initialEntries: [url] })

  render(
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <LevelUpProvider>
          <AuthProvider>
            <RouterProvider router={router} />
          </AuthProvider>
        </LevelUpProvider>
      </ToastProvider>
    </QueryClientProvider>,
  )

  return { router }
}
