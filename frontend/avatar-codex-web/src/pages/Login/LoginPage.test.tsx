import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { login } from '../../services/authService'
import { apiError, mockSession, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

vi.mock('../../services/contentService')

async function fillAndSubmit(email: string, password: string) {
  const userActions = userEvent.setup()
  await userActions.type(await screen.findByLabelText('Email'), email)
  await userActions.type(screen.getByLabelText('Senha'), password)
  await userActions.click(screen.getByRole('button', { name: 'Entrar' }))
}

describe('LoginPage', () => {
  it('entra, mostra boas-vindas e volta para a página de origem', async () => {
    vi.mocked(login).mockResolvedValue({ user: user(), reward: null })
    const { router } = renderRoute('/login?redirect=%2Fprofile')

    await fillAndSubmit('toph@example.com', 'Metal2024')

    expect(login).toHaveBeenCalledWith({ email: 'toph@example.com', password: 'Metal2024' })
    await waitFor(() => expect(router.state.location.pathname).toBe('/profile'))
    expect(await screen.findByText('Bem-vindo(a) de volta, toph!')).toBeInTheDocument()
    expect(await screen.findByRole('heading', { level: 1, name: 'toph' })).toBeInTheDocument()
  })

  it('mostra a mensagem da API quando as credenciais são inválidas', async () => {
    vi.mocked(login).mockRejectedValue(apiError(401, 'Email ou senha inválidos.'))
    const { router } = renderRoute('/login')

    await fillAndSubmit('toph@example.com', 'errada123')

    expect(await screen.findByRole('alert')).toHaveTextContent('Email ou senha inválidos.')
    expect(router.state.location.pathname).toBe('/login')
    expect(screen.getByRole('button', { name: 'Entrar' })).toBeEnabled()
  })

  it('mostra erro amigável quando o servidor não responde', async () => {
    vi.mocked(login).mockRejectedValue(new Error('Network Error'))
    renderRoute('/login')

    await fillAndSubmit('toph@example.com', 'Metal2024')

    expect(await screen.findByRole('alert')).toHaveTextContent(
      'Não foi possível conectar ao servidor',
    )
  })

  it('ignora redirect para outro site', async () => {
    vi.mocked(login).mockResolvedValue({ user: user(), reward: null })
    const { router } = renderRoute('/login?redirect=%2F%2Fevil.example.com')

    await fillAndSubmit('toph@example.com', 'Metal2024')

    await waitFor(() => expect(router.state.location.pathname).toBe('/profile'))
  })

  it('quem já está logado é levado direto ao destino', async () => {
    mockSession()
    const { router } = renderRoute('/login?redirect=%2Fcharacters')

    await waitFor(() => expect(router.state.location.pathname).toBe('/characters'))
  })
})
