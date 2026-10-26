import { QueryClientProvider } from '@tanstack/react-query'
import { RouterProvider } from 'react-router'
import AuthProvider from './contexts/AuthProvider'
import LevelUpProvider from './contexts/LevelUpProvider'
import ToastProvider from './contexts/ToastProvider'
import { createQueryClient } from './lib/queryClient'
import { router } from './router'

const queryClient = createQueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <ToastProvider>
        <LevelUpProvider>
          <AuthProvider>
            <RouterProvider router={router} />
          </AuthProvider>
        </LevelUpProvider>
      </ToastProvider>
    </QueryClientProvider>
  )
}
