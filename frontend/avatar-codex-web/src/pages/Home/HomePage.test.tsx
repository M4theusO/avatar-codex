import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import { getHealth } from '../../services/healthService'
import HomePage from './HomePage'

vi.mock('../../services/healthService')

function renderPage() {
  const client = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={client}>
      <HomePage />
    </QueryClientProvider>,
  )
}

describe('HomePage', () => {
  it('mostra que a API e o banco estão conectados', async () => {
    vi.mocked(getHealth).mockResolvedValue({ status: 'Healthy', checks: { database: 'Healthy' } })
    renderPage()
    expect(await screen.findByText('API e banco de dados conectados')).toBeInTheDocument()
  })

  it('mostra quando o banco está indisponível', async () => {
    vi.mocked(getHealth).mockResolvedValue({
      status: 'Unhealthy',
      checks: { database: 'Unhealthy' },
    })
    renderPage()
    expect(await screen.findByText('API no ar, banco de dados indisponível')).toBeInTheDocument()
  })

  it('mostra quando a API não responde', async () => {
    vi.mocked(getHealth).mockRejectedValue(new Error('Network Error'))
    renderPage()
    expect(await screen.findByText('API indisponível')).toBeInTheDocument()
  })
})
