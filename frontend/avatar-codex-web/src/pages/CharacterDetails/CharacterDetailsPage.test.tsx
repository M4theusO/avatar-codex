import { screen } from '@testing-library/react'
import { AxiosError, AxiosHeaders } from 'axios'
import { describe, expect, it, vi } from 'vitest'
import { getCharacter } from '../../services/contentService'
import { characterDetails } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

function notFound() {
  const config = { headers: new AxiosHeaders() }
  return new AxiosError('Not Found', 'ERR_BAD_REQUEST', config, null, {
    status: 404,
    statusText: 'Not Found',
    data: { status: 404, message: 'Personagem não encontrado.' },
    headers: {},
    config,
  })
}

describe('CharacterDetailsPage', () => {
  it('mostra os dados completos do personagem', async () => {
    vi.mocked(getCharacter).mockResolvedValue(characterDetails())
    renderRoute('/characters/13')

    expect(await screen.findByRole('heading', { level: 1, name: 'Zuko' })).toBeInTheDocument()
    expect(getCharacter).toHaveBeenCalledWith(13)
    expect(screen.getByText('Também conhecido como Lee, Espírito Azul')).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Nação do Fogo' })).toHaveAttribute(
      'href',
      '/nations/4',
    )
    expect(screen.getByText('Depois se tornou Senhor do Fogo.')).toBeInTheDocument()
    expect(screen.getByText('Dobra de relâmpago')).toBeInTheDocument()
    expect(screen.getByRole('heading', { name: 'Redirecionar relâmpago' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: /Iroh/ })).toHaveAttribute('href', '/characters/14')
    expect(screen.getByText('Avatar: A Lenda de Aang')).toBeInTheDocument()
  })

  it('esconde seções sem conteúdo', async () => {
    vi.mocked(getCharacter).mockResolvedValue(
      characterDetails({ abilities: [], relationships: [], bendingSubtypes: [] }),
    )
    renderRoute('/characters/13')

    await screen.findByRole('heading', { level: 1, name: 'Zuko' })
    expect(screen.queryByRole('heading', { name: 'Habilidades' })).not.toBeInTheDocument()
    expect(screen.queryByRole('heading', { name: 'Relações' })).not.toBeInTheDocument()
  })

  it('mostra "não encontrado" quando a API responde 404', async () => {
    vi.mocked(getCharacter).mockRejectedValue(notFound())
    renderRoute('/characters/999')

    expect(await screen.findByText('Personagem não encontrado')).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: 'Tentar novamente' })).not.toBeInTheDocument()
  })

  it('não chama a API com id inválido', async () => {
    renderRoute('/characters/abc')

    expect(await screen.findByText('Personagem não encontrado')).toBeInTheDocument()
    expect(getCharacter).not.toHaveBeenCalled()
  })
})
