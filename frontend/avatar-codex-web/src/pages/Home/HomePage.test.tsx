import { screen, within } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { getCharacters, getElements } from '../../services/contentService'
import { character, fire, mockSession, page } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

describe('HomePage', () => {
  beforeEach(() => {
    vi.mocked(getCharacters).mockResolvedValue(
      page([character({ id: 1, name: 'Aang', isAvatar: true })]),
    )
    vi.mocked(getElements).mockResolvedValue([
      { ...fire, description: 'O elemento do poder.', bendingSubtypes: [] },
    ])
  })

  it('mostra os Avatares e os elementos, com atalhos para a listagem filtrada', async () => {
    renderRoute('/')

    expect(await screen.findByRole('link', { name: 'Aang' })).toHaveAttribute(
      'href',
      '/characters/1',
    )
    expect(getCharacters).toHaveBeenCalledWith({ type: 'avatar', pageSize: 8 })
    expect(screen.getByRole('link', { name: /Ver todos/ })).toHaveAttribute(
      'href',
      '/characters?type=avatar',
    )

    const element = (await screen.findByRole('heading', { name: 'Fogo' })).closest('li')!
    expect(within(element).getByText('O elemento do poder.')).toBeInTheDocument()
    expect(within(element).getByRole('link', { name: /Ver dobradores/ })).toHaveAttribute(
      'href',
      '/characters?elementId=3',
    )
    expect(screen.getByRole('link', { name: 'Comparar personagens' })).toHaveAttribute(
      'href',
      '/compare',
    )
  })

  it('convida o visitante a criar conta', async () => {
    renderRoute('/')

    expect(await screen.findByRole('heading', { name: 'Comece sua jornada' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Criar conta grátis' })).toHaveAttribute(
      'href',
      '/register',
    )
  })

  it('não mostra o convite para quem já está logado', async () => {
    mockSession()
    renderRoute('/')

    await screen.findByRole('link', { name: 'Aang' })
    expect(await screen.findByRole('link', { name: 'Painel' })).toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Comece sua jornada' })).not.toBeInTheDocument()
  })

  it('erro em uma seção não derruba a página', async () => {
    vi.mocked(getElements).mockRejectedValue(new Error('falhou'))
    renderRoute('/')

    expect(await screen.findByRole('link', { name: 'Aang' })).toBeInTheDocument()
    expect(await screen.findByRole('button', { name: 'Tentar novamente' })).toBeInTheDocument()
  })
})
