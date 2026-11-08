import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { getElements } from '../../services/contentService'
import { getRanking } from '../../services/rankingService'
import { fire, mockSession, page, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { Element } from '../../types/content'
import type { Ranking, RankingEntry } from '../../types/ranking'
import { monthLabel, parseRankingQuery, rankingQueryToParams } from '../../utils/ranking'

vi.mock('../../services/contentService')
vi.mock('../../services/rankingService')

const elements: Element[] = [
  { id: 1, name: 'Água', color: '#3186B5', icon: 'droplets', description: '', bendingSubtypes: [] },
  { ...fire, description: '', bendingSubtypes: [] },
]

function entry(overrides: Partial<RankingEntry> = {}): RankingEntry {
  return {
    position: 1,
    username: 'katara',
    level: 7,
    experience: 1240,
    element: null,
    isCurrentUser: false,
    ...overrides,
  }
}

function ranking(overrides: Partial<Ranking> = {}): Ranking {
  return {
    period: 'all',
    periodStart: null,
    periodEnd: null,
    elementId: null,
    entries: page([
      entry({ position: 1, username: 'katara' }),
      entry({ position: 1, username: 'zuko', element: fire }),
      entry({ position: 3, username: 'toph', experience: 900, isCurrentUser: true }),
    ]),
    currentUser: { position: 3, experience: 900 },
    ...overrides,
  }
}

describe('parâmetros do ranking na URL', () => {
  it('lê e escreve só o que muda em relação ao padrão', () => {
    expect(parseRankingQuery(new URLSearchParams('period=month&element=3&page=2'))).toEqual({
      period: 'month',
      elementId: 3,
      page: 2,
    })
    expect(parseRankingQuery(new URLSearchParams('period=semana&element=x&page=-1'))).toEqual({
      period: 'all',
      page: 1,
    })
    expect(rankingQueryToParams({ period: 'all', page: 1 })).toEqual({})
    expect(rankingQueryToParams({ period: 'month', elementId: 3, page: 2 })).toEqual({
      period: 'month',
      element: '3',
      page: '2',
    })
  })

  it('descreve o mês em UTC', () => {
    expect(monthLabel('2026-11-01T00:00:00Z', '2026-12-01T00:00:00Z')).toBe(
      'novembro de 2026 · recomeça em 1 de dezembro',
    )
  })
})

describe('RankingPage', () => {
  it('exige login', async () => {
    const { router } = renderRoute('/ranking')

    await waitFor(() => expect(router.state.location.pathname).toBe('/login'))
  })

  it('mostra as posições vindas do backend, com empate e o usuário destacado', async () => {
    mockSession()
    vi.mocked(getElements).mockResolvedValue(elements)
    vi.mocked(getRanking).mockResolvedValue(ranking())
    renderRoute('/ranking')

    expect(await screen.findByText(/Sua posição:/)).toHaveTextContent(
      'Sua posição: 3º de 3 · 900 XP',
    )
    expect(getRanking).toHaveBeenCalledWith({ period: 'all', page: 1 })
    const rows = within(screen.getByRole('list', { name: 'Ranking' })).getAllByRole('listitem')
    expect(rows[0]).toHaveTextContent('1º lugar')
    expect(rows[1]).toHaveTextContent('1º lugar')
    expect(rows[2]).toHaveTextContent('3º lugar')
    expect(within(rows[1]).getByText('Nv. 7 · Fogo')).toBeInTheDocument()
    expect(rows[2]).toHaveAttribute('aria-current', 'true')
    expect(within(rows[2]).getByText('Você')).toBeInTheDocument()
    expect(within(rows[0]).getByText('1.240 XP')).toBeInTheDocument()
  })

  it('troca para o mês e para um elemento pela URL', async () => {
    mockSession(user({ element: fire }))
    vi.mocked(getElements).mockResolvedValue(elements)
    vi.mocked(getRanking).mockImplementation(async (query) =>
      query.period === 'month'
        ? ranking({
            period: 'month',
            periodStart: '2026-11-01T00:00:00Z',
            periodEnd: '2026-12-01T00:00:00Z',
          })
        : ranking(),
    )
    const u = userEvent.setup()
    const { router } = renderRoute('/ranking')

    await u.click(await screen.findByRole('button', { name: 'Este mês' }))
    expect(router.state.location.search).toBe('?period=month')
    expect(
      await screen.findByText('novembro de 2026 · recomeça em 1 de dezembro'),
    ).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Este mês' })).toHaveAttribute('aria-pressed', 'true')

    await u.click(screen.getByRole('button', { name: 'Água' }))
    expect(router.state.location.search).toBe('?period=month&element=1')
    expect(getRanking).toHaveBeenLastCalledWith({ period: 'month', elementId: 1, page: 1 })
  })

  it('fora do ranking de um elemento: explica e convida para o teste', async () => {
    mockSession()
    vi.mocked(getElements).mockResolvedValue(elements)
    vi.mocked(getRanking).mockResolvedValue(
      ranking({ elementId: 3, entries: page([entry({ element: fire })]), currentUser: null }),
    )
    renderRoute('/ranking?element=3')

    expect(await screen.findByRole('link', { name: 'Faça o teste de elemento' })).toHaveAttribute(
      'href',
      '/element-test',
    )
    expect(screen.getByText(/para entrar no ranking de Fogo/)).toBeInTheDocument()
  })

  it('ranking vazio e erro', async () => {
    mockSession()
    vi.mocked(getElements).mockResolvedValue(elements)
    vi.mocked(getRanking)
      .mockRejectedValueOnce(new Error('falhou'))
      .mockResolvedValue(ranking({ entries: page([]), currentUser: null }))
    const u = userEvent.setup()
    renderRoute('/ranking')

    await u.click(await screen.findByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByText('Ninguém neste ranking ainda')).toBeInTheDocument()
  })
})
