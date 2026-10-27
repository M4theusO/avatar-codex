import { screen, waitFor } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { describe, expect, it, vi } from 'vitest'
import { register } from '../../services/authService'
import { apiError, user } from '../../test/fixtures'
import { renderRoute } from '../../test/renderWithProviders'

async function fillForm() {
  const actions = userEvent.setup()
  await actions.type(await screen.findByLabelText('Nome de usuário'), 'toph')
  await actions.type(screen.getByLabelText('Email'), 'toph@example.com')
  await actions.type(screen.getByLabelText('Senha'), 'curta')
  await actions.type(screen.getByLabelText('Confirme a senha'), 'curta')
  await actions.click(screen.getByRole('button', { name: 'Criar conta' }))
}

describe('RegisterPage', () => {
  it('cria a conta e abre o painel', async () => {
    vi.mocked(register).mockResolvedValue({ user: user(), reward: null })
    const { router } = renderRoute('/register')

    await fillForm()

    expect(register).toHaveBeenCalledWith({
      username: 'toph',
      email: 'toph@example.com',
      password: 'curta',
      confirmPassword: 'curta',
    })
    await waitFor(() => expect(router.state.location.pathname).toBe('/dashboard'))
    expect(await screen.findByText('Conta criada! Boas-vindas, toph.')).toBeInTheDocument()
  })

  it('anuncia a conquista "Primeiro passo" devolvida pelo cadastro', async () => {
    vi.mocked(register).mockResolvedValue({
      user: user({ experiencePoints: 10, levelProgress: 10 }),
      reward: {
        xpGained: 10,
        totalXp: 10,
        level: 1,
        leveledUp: false,
        currentLevelXp: 0,
        nextLevelXp: 100,
        levelProgress: 10,
        achievementsUnlocked: [
          { id: 1, name: 'Primeiro passo', icon: 'footprints', experienceReward: 10 },
        ],
        challengesCompleted: [],
      },
    })
    renderRoute('/register')

    await fillForm()

    expect(
      await screen.findByText('Conquista desbloqueada — Primeiro passo! +10 XP'),
    ).toBeInTheDocument()
  })

  it('mostra os erros de validação da API ao lado de cada campo', async () => {
    vi.mocked(register).mockRejectedValue(
      apiError(400, 'Os dados enviados são inválidos.', [
        { field: 'password', message: 'A senha deve ter pelo menos 8 caracteres.' },
      ]),
    )
    renderRoute('/register')

    await fillForm()

    const password = screen.getByLabelText('Senha')
    expect(await screen.findByText('A senha deve ter pelo menos 8 caracteres.')).toBeInTheDocument()
    expect(password).toHaveAttribute('aria-invalid', 'true')
    expect(password).toHaveAccessibleDescription(
      expect.stringContaining('A senha deve ter pelo menos 8 caracteres.'),
    )
    expect(screen.getByLabelText('Email')).not.toHaveAttribute('aria-invalid')
  })

  it('mostra conflito de email já cadastrado', async () => {
    vi.mocked(register).mockRejectedValue(apiError(409, 'Este email já está cadastrado.'))
    renderRoute('/register')

    await fillForm()

    expect(await screen.findByRole('alert')).toHaveTextContent('Este email já está cadastrado.')
  })
})
