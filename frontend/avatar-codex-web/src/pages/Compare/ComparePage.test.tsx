import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { compareCharacters, getCharacter, getCharacters } from '../../services/contentService'
import { apiError, character, characterDetails, fire, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'
import type { CharacterComparison, ComparedCharacter } from '../../types/content'

vi.mock('../../services/contentService')

const zuko = character()
const sokka = character({
  id: 3,
  name: 'Sokka',
  element: null,
  isBender: false,
  nation: { id: 1, name: 'Tribo da Água do Sul' },
})

function compared(base: typeof zuko): ComparedCharacter {
  return {
    id: base.id,
    name: base.name,
    description: base.description,
    image: null,
    nation: base.nation,
    element: base.element,
    isBender: base.isBender,
    isAvatar: false,
    status: 'Alive',
    firstAppearance: null,
    bendingSubtypes: [],
    abilities: base.element
      ? [{ id: 1, name: 'Redirecionar relâmpago', description: '-', element: fire, image: null }]
      : [],
    series: [{ id: 1, code: 'ATLA', name: 'Avatar: A Lenda de Aang' }],
  }
}

const comparison: CharacterComparison = {
  characters: [compared(zuko), compared(sokka)],
  attributes: [
    {
      id: 1,
      name: 'Domínio elemental',
      description: 'Controle da dobra.',
      values: [82, 0],
      leaderId: 13,
    },
    { id: 2, name: 'Combate', description: 'Luta.', values: [85, 72], leaderId: 13 },
    { id: 3, name: 'Estratégia', description: 'Planos.', values: [65, 92], leaderId: 3 },
  ],
}

describe('ComparePage', () => {
  beforeEach(() => {
    vi.mocked(getCharacters).mockImplementation(async (query) =>
      page([sokka, zuko].filter((c) => !query.search || c.name.includes(query.search))),
    )
    vi.mocked(getCharacter).mockImplementation(async (id) =>
      id === 3 ? characterDetails({ ...sokka }) : characterDetails(),
    )
    vi.mocked(compareCharacters).mockResolvedValue(comparison)
  })

  it('escolhe os dois personagens pela busca e mostra atributos, maior valor e aviso', async () => {
    const user = userEvent.setup()
    const { router } = renderRoute('/compare')

    expect(await screen.findByText('Escolha dois personagens')).toBeInTheDocument()
    expect(screen.getByRole('note')).toHaveTextContent('não informações canônicas')

    const first = screen.getByRole('group', { name: 'Primeiro personagem' })
    await user.click(await within(first).findByRole('button', { name: /Zuko/ }))
    await waitFor(() => expect(router.state.location.search).toBe('?a=13'))

    // O personagem já escolhido não aparece do outro lado.
    const second = screen.getByRole('group', { name: 'Segundo personagem' })
    await user.type(within(second).getByRole('searchbox'), 'Sok')
    const result = await within(second).findByRole('button', { name: /Sokka/ })
    expect(within(second).queryByRole('button', { name: /Zuko/ })).not.toBeInTheDocument()
    await user.click(result)

    await waitFor(() => expect(router.state.location.search).toBe('?a=13&b=3'))
    expect(compareCharacters).toHaveBeenCalledWith(13, 3)

    const strategy = (await screen.findByRole('heading', { name: 'Estratégia' })).closest('li')!
    expect(within(strategy).getByText('92').parentElement).toHaveTextContent('92 (maior valor)')
    expect(within(strategy).getByText('65')).toBeInTheDocument()
    expect(
      screen.getByRole('img', { name: /Radar dos atributos de Zuko e Sokka/ }),
    ).toBeInTheDocument()

    const table = screen.getByRole('table', { name: 'Informações dos personagens lado a lado' })
    const kind = within(table).getByRole('row', { name: /Tipo/ })
    expect(kind).toHaveTextContent('Dobrador')
    expect(kind).toHaveTextContent('Não dobrador')
  })

  it('aberto pelo link do personagem, já traz o primeiro lado e permite trocar e inverter', async () => {
    const user = userEvent.setup()
    const { router } = renderRoute('/compare?a=13&b=3')

    expect(await screen.findByRole('heading', { name: 'Domínio elemental' })).toBeInTheDocument()

    await user.click(screen.getByRole('button', { name: 'Inverter lados' }))
    await waitFor(() => expect(router.state.location.search).toBe('?a=3&b=13'))

    await user.click(screen.getByRole('button', { name: 'Trocar segundo personagem' }))
    await waitFor(() => expect(router.state.location.search).toBe('?a=3'))
    expect(screen.getByRole('searchbox', { name: 'Buscar segundo personagem' })).toHaveFocus()
    expect(screen.getByText('Escolha dois personagens')).toBeInTheDocument()
  })

  it('personagem inexistente no link: mensagem própria', async () => {
    vi.mocked(compareCharacters).mockRejectedValue(apiError(404, 'Personagem não encontrado.'))
    renderRoute('/compare?a=13&b=999')

    expect(await screen.findByText('Personagem não encontrado')).toBeInTheDocument()
  })

  it('erro ao carregar: mensagem amigável e tentar de novo', async () => {
    vi.mocked(compareCharacters).mockRejectedValueOnce(apiError(500, 'Erro interno.'))
    const user = userEvent.setup()
    renderRoute('/compare?a=13&b=3')

    expect(await screen.findByText('Não foi possível carregar a comparação')).toBeInTheDocument()
    await user.click(screen.getByRole('button', { name: 'Tentar novamente' }))
    expect(await screen.findByRole('heading', { name: 'Estratégia' })).toBeInTheDocument()
  })
})
