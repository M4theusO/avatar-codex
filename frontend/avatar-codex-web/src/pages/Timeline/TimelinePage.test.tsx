import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getEras, getTimeline } from '../../services/contentService'
import { fire } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { TimelineEra } from '../../types/content'

vi.mock('../../services/contentService')

const zuko = { id: 13, name: 'Zuko', image: null, element: fire }
const aang = { id: 1, name: 'Aang', image: null, element: null }
const korra = { id: 22, name: 'Korra', image: null, element: null }

const war: TimelineEra = {
  id: 3,
  name: 'A Guerra dos Cem Anos',
  description: 'Cem anos de conflito.',
  events: [
    {
      id: 10,
      title: 'O Dia do Sol Negro',
      description: 'A invasão falha durante o eclipse.',
      dateLabel: '100 AG',
      image: null,
      characters: [aang, zuko],
    },
  ],
}

const korraEra: TimelineEra = {
  id: 4,
  name: 'A Era de Korra',
  description: 'A República Unida.',
  events: [
    {
      id: 20,
      title: 'A Revolução Igualitária',
      description: 'Amon tenta tomar a cidade.',
      dateLabel: '170 AG',
      image: null,
      characters: [korra],
    },
  ],
}

describe('TimelinePage', () => {
  beforeEach(() => {
    vi.mocked(getEras).mockResolvedValue([
      { id: 3, name: 'A Guerra dos Cem Anos', description: '-', eventCount: 1 },
      { id: 4, name: 'A Era de Korra', description: '-', eventCount: 1 },
    ])
    vi.mocked(getTimeline).mockImplementation(async (filters) =>
      filters.characterId === 13 ? [war] : [war, korraEra],
    )
  })

  it('mostra as eras em ordem, com eventos, datas e personagens', async () => {
    renderRoute('/timeline')

    const eras = await screen.findAllByRole('heading', { level: 2 })
    expect(eras.map((h) => h.textContent)).toEqual(['A Guerra dos Cem Anos', 'A Era de Korra'])

    const event = screen
      .getByRole('heading', { level: 3, name: 'O Dia do Sol Negro' })
      .closest('li')!
    expect(within(event).getByText('100 AG')).toBeInTheDocument()
    expect(within(event).getByRole('link', { name: 'Zuko' })).toHaveAttribute(
      'href',
      '/characters/13',
    )
  })

  it('filtra por personagem pela URL, com as opções vindas da linha do tempo', async () => {
    const user = userEvent.setup()
    const { router } = renderRoute('/timeline')

    const select = await screen.findByLabelText('Personagem')
    await waitFor(() =>
      expect(
        within(select)
          .getAllByRole('option')
          .map((o) => o.textContent),
      ).toEqual(['Todos', 'Aang', 'Korra', 'Zuko']),
    )

    await user.selectOptions(select, '13')

    await waitFor(() => expect(router.state.location.search).toBe('?characterId=13'))
    expect(getTimeline).toHaveBeenCalledWith({ characterId: 13 })
    await waitFor(() =>
      expect(screen.queryByRole('heading', { name: 'A Era de Korra' })).not.toBeInTheDocument(),
    )
  })

  it('filtro sem resultado mostra estado vazio e limpa os filtros', async () => {
    vi.mocked(getTimeline).mockImplementation(async (filters) =>
      filters.eraId === 4 && filters.characterId === 13 ? [] : [war, korraEra],
    )
    const user = userEvent.setup()
    const { router } = renderRoute('/timeline?eraId=4&characterId=13')

    expect(await screen.findByText('Nenhum evento encontrado')).toBeInTheDocument()

    await user.click(screen.getAllByRole('button', { name: 'Limpar filtros' })[0])

    await waitFor(() => expect(router.state.location.search).toBe(''))
    expect(await screen.findByRole('heading', { name: 'A Era de Korra' })).toBeInTheDocument()
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    vi.mocked(getTimeline).mockRejectedValueOnce(new Error('falhou'))
    const user = userEvent.setup()
    renderRoute('/timeline?eraId=3')

    expect(
      await screen.findByText('Não foi possível carregar a linha do tempo'),
    ).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Tentar novamente' }))
    expect(await screen.findByRole('heading', { name: 'O Dia do Sol Negro' })).toBeInTheDocument()
  })
})
