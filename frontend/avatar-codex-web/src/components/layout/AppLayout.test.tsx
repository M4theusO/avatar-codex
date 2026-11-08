import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getNations } from '../../services/contentService'
import { mockSession } from '../../test/fixtures'
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
    // O link ativo só muda quando a página lazy termina de carregar (lento com a suíte em paralelo).
    await waitFor(() =>
      expect(screen.getByRole('link', { name: 'Nações' })).toHaveAttribute('aria-current', 'page'),
    )
    expect(screen.getByRole('button', { name: 'Abrir menu' })).toBeInTheDocument()
  })

  it('os atalhos só com ícone têm o nome numa dica, sem repetir para leitores de tela', async () => {
    mockSession()
    vi.mocked(getNations).mockResolvedValue([])
    renderRoute('/nations')

    for (const label of ['Favoritos', 'Desafios diários', 'Conquistas', 'Ranking']) {
      const link = await screen.findByRole('link', { name: label })
      const tip = link.querySelector('[aria-hidden="true"]:not(svg)')
      expect(tip).toHaveTextContent(label)
    }
    const logout = screen.getByRole('button', { name: 'Sair' })
    expect(logout.querySelector('span[aria-hidden="true"]')).toHaveTextContent('Sair')
  })

  it('mostra a página 404 em rotas desconhecidas', async () => {
    renderRoute('/rota-que-nao-existe')
    expect(
      await screen.findByRole('heading', { name: 'Página não encontrada' }),
    ).toBeInTheDocument()
  })
})
