import { screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getAbilities, getAbility } from '../../services/contentService'
import { apiError, character, fire } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { AbilityDetails, AbilityListItem } from '../../types/content'
import { groupAbilitiesByElement } from '../../utils/abilities'

vi.mock('../../services/contentService')

const water = { id: 1, name: 'Água', color: '#3186B5', icon: 'droplet' }

const abilities: AbilityListItem[] = [
  {
    id: 2,
    name: 'Cura',
    description: 'Fecha ferimentos com água.',
    element: water,
    image: null,
    characterCount: 1,
  },
  {
    id: 1,
    name: 'Redirecionamento de relâmpago',
    description: 'Desvia raios.',
    element: fire,
    image: null,
    characterCount: 2,
  },
  {
    id: 3,
    name: 'Bloqueio de chi',
    description: 'Golpes em pontos de pressão.',
    element: null,
    image: null,
    characterCount: 3,
  },
]

const lightning: AbilityDetails = {
  id: 1,
  name: 'Redirecionamento de relâmpago',
  description: 'Desvia raios.',
  element: fire,
  image: null,
  characters: [character(), character({ id: 14, name: 'Iroh' })],
}

describe('groupAbilitiesByElement', () => {
  it('agrupa na ordem da API, com as sem elemento num grupo próprio', () => {
    const groups = groupAbilitiesByElement([...abilities, { ...abilities[0], id: 9 }])

    expect(groups.map((g) => g.element?.name ?? null)).toEqual(['Água', 'Fogo', null])
    expect(groups[0].abilities.map((a) => a.id)).toEqual([2, 9])
  })
})

describe('AbilitiesPage', () => {
  it('lista as habilidades por elemento, com link e quantidade de personagens', async () => {
    vi.mocked(getAbilities).mockResolvedValue(abilities)
    renderRoute('/abilities')

    const fireSection = await screen.findByRole('region', { name: 'Fogo' })
    const link = within(fireSection).getByRole('link', { name: 'Redirecionamento de relâmpago' })
    expect(link).toHaveAttribute('href', '/abilities/1')
    expect(within(fireSection).getByText('2 personagens')).toBeInTheDocument()

    expect(screen.getAllByRole('heading', { level: 2 }).map((h) => h.textContent)).toEqual([
      'Água',
      'Fogo',
      'Sem elemento',
    ])
    // Singular quando há um só personagem.
    expect(
      within(screen.getByRole('region', { name: 'Água' })).getByText('1 personagem'),
    ).toBeInTheDocument()
  })

  it('sem habilidades cadastradas mostra estado vazio', async () => {
    vi.mocked(getAbilities).mockResolvedValue([])
    renderRoute('/abilities')

    expect(await screen.findByText('Nenhuma habilidade cadastrada ainda')).toBeInTheDocument()
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    vi.mocked(getAbilities).mockRejectedValueOnce(new Error('falhou')).mockResolvedValue(abilities)
    const user = userEvent.setup()
    renderRoute('/abilities')

    await user.click(await screen.findByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByRole('link', { name: 'Bloqueio de chi' })).toBeInTheDocument()
  })
})

describe('AbilityDetailsPage', () => {
  it('mostra a habilidade e os personagens que a dominam', async () => {
    vi.mocked(getAbility).mockResolvedValue(lightning)
    renderRoute('/abilities/1')

    expect(
      await screen.findByRole('heading', { level: 1, name: 'Redirecionamento de relâmpago' }),
    ).toBeInTheDocument()
    expect(getAbility).toHaveBeenCalledWith(1)
    expect(screen.getByRole('heading', { name: 'Quem domina (2 personagens)' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Iroh/ })).toHaveAttribute('href', '/characters/14')
    expect(screen.getByRole('link', { name: /Todas as habilidades/ })).toHaveAttribute(
      'href',
      '/abilities',
    )
  })

  it('habilidade inexistente: mensagem e link para a lista', async () => {
    vi.mocked(getAbility).mockRejectedValue(apiError(404, 'Habilidade não encontrada.'))
    renderRoute('/abilities/999')

    expect(await screen.findByText('Habilidade não encontrada')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Ver habilidades' })).toHaveAttribute(
      'href',
      '/abilities',
    )
  })

  it('id inválido não chama a API', async () => {
    renderRoute('/abilities/abc')

    expect(await screen.findByText('Habilidade não encontrada')).toBeInTheDocument()
    expect(getAbility).not.toHaveBeenCalled()
  })
})
