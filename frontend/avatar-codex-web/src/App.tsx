import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import HomePage from './pages/Home/HomePage'

const queryClient = new QueryClient()

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <HomePage />
    </QueryClientProvider>
  )
}
