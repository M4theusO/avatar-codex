import { screen, waitFor, within } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getCharacters, getElements, getNations, getSeries } from '../../services/contentService'
import { character, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

beforeEach(() => {
  vi.mocked(getNations).mockResolvedValue([
    {
      id: 4,
      name: 'Nação do Fogo',
      description: '',
      symbolImage: null,
      element: null,
      characterCount: 9,
    },
  ])
  vi.mocked(getElements).mockResolvedValue([])
  vi.mocked(getSeries).mockResolvedValue([])
})

describe('CharactersPage', () => {
  it('lista personagens com nome, nação e elemento', async () => {
    vi.mocked(getCharacters).mockResolvedValue(
      page([character(), character({ id: 2, name: 'Asami Sato', element: null, isBender: false })]),
    )
    renderRoute('/characters')

    const zuko = await screen.findByRole('link', { name: 'Zuko' })
    expect(zuko).toHaveAttribute('href', '/characters/13')
    expect(screen.getByText('2 personagens encontrados')).toBeInTheDocument()

    const zukoCard = zuko.closest('article')!
    expect(within(zukoCard).getByText('Fogo')).toBeInTheDocument()
    expect(within(zukoCard).getByText('Nação do Fogo')).toBeInTheDocument()
    expect(screen.getByText('Não dobrador')).toBeInTheDocument()
  })

  it('envia os filtros da URL para a API', async () => {
    vi.mocked(getCharacters).mockResolvedValue(page([character()]))
    renderRoute('/characters?type=bender&nationId=4&page=2&sort=element')

    await screen.findByRole('link', { name: 'Zuko' })
    expect(getCharacters).toHaveBeenCalledWith(
      expect.objectContaining({ type: 'bender', nationId: 4, page: 2, sort: 'element' }),
    )
  })

  it('busca com debounce, atualiza a URL e volta para a página 1', async () => {
    vi.mocked(getCharacters).mockResolvedValue(page([character()]))
    const user = userEvent.setup()
    const { router } = renderRoute('/characters?page=3')

    await user.type(await screen.findByLabelText('Buscar personagem'), 'zuko')

    await waitFor(() => expect(router.state.location.search).toBe('?search=zuko'))
    await waitFor(() =>
      expect(getCharacters).toHaveBeenLastCalledWith(
        expect.objectContaining({ search: 'zuko', page: 1 }),
      ),
    )
    // Uma chamada para a carga inicial e uma para o termo final, não uma por tecla.
    expect(vi.mocked(getCharacters).mock.calls.filter(([q]) => q.search)).toHaveLength(1)
  })

  it('filtra por nação pelo select', async () => {
    vi.mocked(getCharacters).mockResolvedValue(page([character()]))
    const user = userEvent.setup()
    const { router } = renderRoute('/characters')

    const nation = await screen.findByLabelText('Nação')
    await screen.findByRole('option', { name: 'Nação do Fogo' })
    await user.selectOptions(nation, '4')

    expect(router.state.location.search).toBe('?nationId=4')
  })

  it('mostra estado vazio e permite limpar os filtros', async () => {
    vi.mocked(getCharacters).mockResolvedValue(page([]))
    const user = userEvent.setup()
    const { router } = renderRoute('/characters?search=xyz')

    expect(await screen.findByText('Nenhum personagem encontrado')).toBeInTheDocument()
    expect(screen.getByLabelText('Buscar personagem')).toHaveValue('xyz')

    await user.click(screen.getAllByRole('button', { name: 'Limpar filtros' })[0])

    expect(router.state.location.search).toBe('')
    expect(screen.getByLabelText('Buscar personagem')).toHaveValue('')
  })

  it('mostra erro amigável e tenta de novo', async () => {
    vi.mocked(getCharacters).mockRejectedValueOnce(new Error('Network Error'))
    vi.mocked(getCharacters).mockResolvedValueOnce(page([character()]))
    const user = userEvent.setup()
    renderRoute('/characters')

    expect(await screen.findByRole('alert')).toHaveTextContent('Não foi possível carregar')
    await user.click(screen.getByRole('button', { name: 'Tentar novamente' }))

    expect(await screen.findByRole('link', { name: 'Zuko' })).toBeInTheDocument()
  })

  it('pagina pela navegação numérica', async () => {
    vi.mocked(getCharacters).mockResolvedValue(
      page([character()], { totalItems: 30, totalPages: 3 }),
    )
    const user = userEvent.setup()
    const { router } = renderRoute('/characters')

    await user.click(await screen.findByRole('button', { name: 'Página 2' }))

    expect(router.state.location.search).toBe('?page=2')
    expect(screen.getByRole('button', { name: 'Página anterior' })).toBeDisabled()
  })
})
