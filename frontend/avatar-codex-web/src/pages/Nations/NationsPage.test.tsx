import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getNations } from '../../services/contentService'
import { fire } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { Nation } from '../../types/content'

vi.mock('../../services/contentService')

const nations: Nation[] = [
  {
    id: 4,
    name: 'Nação do Fogo',
    description: 'Império do fogo.',
    symbolImage: null,
    element: fire,
    characterCount: 9,
  },
  {
    id: 5,
    name: 'Cidade República',
    description: 'Capital da República Unida.',
    symbolImage: null,
    element: null,
    characterCount: 1,
  },
]

describe('NationsPage', () => {
  it('lista as nações com descrição, link e quantidade de personagens', async () => {
    vi.mocked(getNations).mockResolvedValue(nations)
    renderRoute('/nations')

    const fireNation = await screen.findByRole('link', { name: 'Nação do Fogo' })
    expect(fireNation).toHaveAttribute('href', '/nations/4')
    const card = fireNation.closest('article')!
    expect(within(card).getByText('Império do fogo.')).toBeInTheDocument()
    expect(within(card).getByText('9 personagens')).toBeInTheDocument()
    // Singular quando há um só personagem.
    expect(screen.getByText('1 personagem')).toBeInTheDocument()
  })

  it('sem nações cadastradas mostra estado vazio', async () => {
    vi.mocked(getNations).mockResolvedValue([])
    renderRoute('/nations')

    expect(await screen.findByText('Nenhuma nação cadastrada ainda')).toBeInTheDocument()
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    vi.mocked(getNations).mockRejectedValueOnce(new Error('falhou')).mockResolvedValue(nations)
    const user = userEvent.setup()
    renderRoute('/nations')

    await user.click(await screen.findByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByRole('link', { name: 'Cidade República' })).toBeInTheDocument()
  })
})
