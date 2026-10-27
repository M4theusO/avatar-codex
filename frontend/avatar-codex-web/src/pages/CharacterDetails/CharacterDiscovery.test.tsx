import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getCharacter, getNation } from '../../services/contentService'
import { addFavorite, discoverCharacter, discoverNation } from '../../services/userService'
import { apiError, character, characterDetails, mockSession } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

const reward = {
  xpGained: 10,
  totalXp: 110,
  level: 2,
  leveledUp: true,
  currentLevelXp: 100,
  nextLevelXp: 250,
  levelProgress: 6,
  achievementsUnlocked: [],
  challengesCompleted: [],
}

describe('descoberta (RN-12)', () => {
  it('registra a descoberta uma única vez, depois que o personagem aparece', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    vi.mocked(discoverCharacter).mockResolvedValue({ isNew: true, reward })
    renderRoute('/characters/13')

    expect(await screen.findByRole('heading', { level: 1, name: 'Zuko' })).toBeInTheDocument()
    expect(
      await screen.findByText('✨ Nova descoberta — Você descobriu Zuko! +10 XP'),
    ).toBeInTheDocument()
    expect(screen.getByRole('dialog', { name: 'Novo nível!' })).toHaveTextContent(
      'Você alcançou o nível 2.',
    )

    // O refetch depois da descoberta não dispara outra chamada.
    await waitFor(() => expect(getCharacter).toHaveBeenCalledTimes(2))
    expect(discoverCharacter).toHaveBeenCalledTimes(1)
    expect(discoverCharacter).toHaveBeenCalledWith(13)
  })

  it('descoberta repetida não mostra toast', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails({ isDiscovered: true }))
    renderRoute('/characters/13')

    await screen.findByRole('heading', { level: 1, name: 'Zuko' })
    await waitFor(() => expect(discoverCharacter).toHaveBeenCalledOnce())
    expect(screen.queryByText(/Nova descoberta/)).not.toBeInTheDocument()
    expect(screen.getByText('Descoberto')).toBeInTheDocument()
  })

  it('visitante não registra descoberta e vê o convite para entrar', async () => {
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    renderRoute('/characters/13')

    await screen.findByRole('heading', { level: 1, name: 'Zuko' })
    expect(await screen.findByRole('link', { name: 'Entre para favoritar' })).toHaveAttribute(
      'href',
      '/login?redirect=%2Fcharacters%2F13',
    )
    expect(discoverCharacter).not.toHaveBeenCalled()
  })

  it('não registra descoberta quando o personagem não existe', async () => {
    mockSession()
    vi.mocked(getCharacter).mockRejectedValue(apiError(404, 'Personagem não encontrado.'))
    renderRoute('/characters/999')

    expect(await screen.findByText('Personagem não encontrado')).toBeInTheDocument()
    expect(discoverCharacter).not.toHaveBeenCalled()
  })

  it('registra a descoberta da nação (DA-01)', async () => {
    mockSession()
    vi.mocked(getNation).mockResolvedValue({
      id: 4,
      name: 'Nação do Fogo',
      description: 'Nação insular.',
      symbolImage: null,
      element: null,
      characters: [character()],
    })
    vi.mocked(discoverNation).mockResolvedValue({
      isNew: true,
      reward: { ...reward, leveledUp: false },
    })
    renderRoute('/nations/4')

    expect(
      await screen.findByText('✨ Nova descoberta — Você descobriu Nação do Fogo! +10 XP'),
    ).toBeInTheDocument()
    expect(discoverNation).toHaveBeenCalledWith(4)
  })

  it('anuncia a conquista desbloqueada separada do XP da descoberta', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    vi.mocked(discoverCharacter).mockResolvedValue({
      isNew: true,
      reward: {
        ...reward,
        xpGained: 60,
        totalXp: 160,
        leveledUp: false,
        achievementsUnlocked: [{ id: 2, name: 'Curioso', icon: 'eye', experienceReward: 50 }],
      },
    })
    renderRoute('/characters/13')

    expect(
      await screen.findByText('✨ Nova descoberta — Você descobriu Zuko! +10 XP'),
    ).toBeInTheDocument()
    expect(screen.getByText('Conquista desbloqueada — Curioso! +50 XP')).toBeInTheDocument()
  })
})

describe('favoritar', () => {
  it('marca na hora (otimista) e confirma com toast', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    const actions = userEvent.setup()
    renderRoute('/characters/13')

    const button = await screen.findByRole('button', { name: 'Favoritar' })
    expect(button).toHaveAttribute('aria-pressed', 'false')

    await actions.click(button)

    expect(screen.getByRole('button', { name: 'Favoritado' })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
    expect(addFavorite).toHaveBeenCalledWith(13)
    expect(await screen.findByText('✓ Personagem adicionado aos favoritos.')).toBeInTheDocument()
  })

  it('desfaz a marcação se a API falhar', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    vi.mocked(addFavorite).mockRejectedValue(new Error('Network Error'))
    const actions = userEvent.setup()
    renderRoute('/characters/13')

    await actions.click(await screen.findByRole('button', { name: 'Favoritar' }))

    expect(
      await screen.findByText('Não foi possível atualizar seus favoritos. Tente novamente.'),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Favoritar' })).toHaveAttribute(
      'aria-pressed',
      'false',
    )
  })

  it('cards da listagem mostram favorito e descoberto só para quem está logado', async () => {
    mockSession()
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    const { getCharacters } = await import('../../services/contentService')
    vi.mocked(getCharacters).mockResolvedValue({
      items: [character({ isFavorite: true, isDiscovered: true })],
      page: 1,
      pageSize: 12,
      totalItems: 1,
      totalPages: 1,
    })
    renderRoute('/characters')

    const card = (await screen.findByRole('link', { name: 'Zuko' })).closest('article')!
    expect(within(card).getByRole('button', { name: 'Favoritar Zuko' })).toHaveAttribute(
      'aria-pressed',
      'true',
    )
    expect(within(card).getByText('Descoberto')).toBeInTheDocument()
  })
})
