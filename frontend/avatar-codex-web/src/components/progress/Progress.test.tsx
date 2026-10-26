import { render, screen, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { useEffect } from 'react'
import { describe, expect, it, vi } from 'vitest'
import LevelUpProvider from '../../contexts/LevelUpProvider'
import { useLevelUp } from '../../hooks/useLevelUp'
import { getCharacter } from '../../services/contentService'
import { discoverCharacter, getExperience } from '../../services/userService'
import { characterDetails, mockSession, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import XpBar from './XpBar'

vi.mock('../../services/contentService')

describe('XpBar', () => {
  it('mostra os valores vindos do backend, sem calcular nada', () => {
    render(
      <XpBar
        user={user({
          level: 2,
          experiencePoints: 175,
          currentLevelXp: 100,
          nextLevelXp: 250,
          levelProgress: 50,
        })}
      />,
    )

    const bar = screen.getByRole('progressbar', { name: 'Experiência' })
    expect(bar).toHaveAttribute('aria-valuemin', '100')
    expect(bar).toHaveAttribute('aria-valuemax', '250')
    expect(bar).toHaveAttribute('aria-valuenow', '175')
    expect(bar).toHaveAttribute('aria-valuetext', 'Nível 2: 175 de 250 XP')
    expect(screen.getByText('Nível 2')).toBeInTheDocument()
    expect(bar.firstElementChild).toHaveStyle({ width: '50%' })
  })
})

function TriggerLevels({ levels }: { levels: number[] }) {
  const { showLevelUp } = useLevelUp()
  useEffect(() => {
    levels.forEach(showLevelUp)
  }, [levels, showLevelUp])
  return null
}

describe('LevelUpModal', () => {
  it('mostra vários níveis um de cada vez', async () => {
    const actions = userEvent.setup()
    render(
      <LevelUpProvider>
        <TriggerLevels levels={[2, 3]} />
      </LevelUpProvider>,
    )

    const dialog = screen.getByRole('dialog', { name: 'Novo nível!' })
    expect(dialog).toHaveTextContent('Você alcançou o nível 2.')
    expect(within(dialog).getByRole('button', { name: 'Continuar' })).toHaveFocus()

    await actions.click(within(dialog).getByRole('button', { name: 'Continuar' }))
    expect(screen.getByRole('dialog', { name: 'Novo nível!' })).toHaveTextContent(
      'Você alcançou o nível 3.',
    )

    await actions.click(screen.getByRole('button', { name: 'Continuar' }))
    expect(screen.queryByRole('dialog')).not.toBeInTheDocument()
  })
})

describe('XP na interface', () => {
  it('a barra da navbar acompanha o reward da descoberta', async () => {
    mockSession(user({ experiencePoints: 90, levelProgress: 90 }))
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    vi.mocked(discoverCharacter).mockResolvedValue({
      isNew: true,
      reward: {
        xpGained: 10,
        totalXp: 100,
        level: 2,
        leveledUp: true,
        currentLevelXp: 100,
        nextLevelXp: 250,
        levelProgress: 0,
        achievementsUnlocked: [],
        challengesCompleted: [],
      },
    })
    renderRoute('/characters/13')

    const nav = await screen.findByRole('navigation', { name: 'Principal' })
    expect(await within(nav).findByText('Nv. 2')).toBeInTheDocument()
    expect(within(nav).getByRole('progressbar', { name: 'Experiência' })).toHaveAttribute(
      'aria-valuenow',
      '100',
    )
  })

  it('perfil mostra o histórico de XP', async () => {
    mockSession(user({ experiencePoints: 20 }))
    vi.mocked(getExperience).mockResolvedValue({
      items: [
        {
          id: 2,
          amount: 10,
          type: 'NationDiscovered',
          description: 'Descobriu Nação do Fogo',
          createdAt: '2026-10-26T15:00:00Z',
        },
        {
          id: 1,
          amount: 10,
          type: 'CharacterDiscovered',
          description: 'Descobriu Zuko',
          createdAt: '2026-10-26T14:00:00Z',
        },
      ],
      page: 1,
      pageSize: 10,
      totalItems: 2,
      totalPages: 1,
    })
    renderRoute('/profile')

    expect(await screen.findByText('Descobriu Nação do Fogo')).toBeInTheDocument()
    expect(screen.getByText('Descobriu Zuko')).toBeInTheDocument()
    expect(screen.getAllByText('+10 XP')).toHaveLength(2)
  })
})
