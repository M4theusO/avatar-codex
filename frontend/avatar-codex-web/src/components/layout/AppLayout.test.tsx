import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getNations } from '../../services/contentService'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

describe('AppLayout', () => {
  it('abre o menu mobile e navega pelos links', async () => {
    vi.mocked(getNations).mockResolvedValue([])
    const user = userEvent.setup()
    const { router } = renderRoute('/timeline')

    const menuButton = await screen.findByRole('button', { name: 'Abrir menu' })
    expect(menuButton).toHaveAttribute('aria-expanded', 'false')

    await user.click(menuButton)
    expect(screen.getByRole('button', { name: 'Fechar menu' })).toHaveAttribute(
      'aria-expanded',
      'true',
    )

    await user.click(screen.getByRole('link', { name: 'Nações' }))
    await waitFor(() => expect(router.state.location.pathname).toBe('/nations'))
    expect(screen.getByRole('button', { name: 'Abrir menu' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Nações' })).toHaveAttribute('aria-current', 'page')
  })

  it('mostra a página 404 em rotas desconhecidas', async () => {
    renderRoute('/rota-que-nao-existe')
    expect(
      await screen.findByRole('heading', { name: 'Página não encontrada' }),
    ).toBeInTheDocument()
  })
})
