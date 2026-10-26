import { Link } from 'react-router'
import AuthCard from '../../components/auth/AuthCard'
import { Button } from '../../components/common/Button'
import TextField from '../../components/common/TextField'
import { useAuth } from '../../hooks/useAuth'
import { useAuthForm } from '../../hooks/useAuthForm'

export default function LoginPage() {
  const { login } = useAuth()
  const { bind, handleSubmit, formError, submitting, redirectParam } = useAuthForm(
    { email: '', password: '' },
    login,
    (user) => `Bem-vindo(a) de volta, ${user.username}!`,
  )

  const registerLink = redirectParam
    ? `/register?redirect=${encodeURIComponent(redirectParam)}`
    : '/register'

  return (
    <>
      <title>Entrar · Avatar Codex</title>
      <AuthCard
        title="Entrar"
        description="Continue sua jornada pelo mundo de Avatar."
        error={formError}
        onSubmit={handleSubmit}
        footer={
          <>
            Ainda não tem conta? <Link to={registerLink}>Criar conta</Link>
          </>
        }
      >
        <TextField label="Email" type="email" autoComplete="email" {...bind('email')} />
        <TextField
          label="Senha"
          type="password"
          autoComplete="current-password"
          {...bind('password')}
        />
        <Button type="submit" variant="primary" disabled={submitting}>
          {submitting ? 'Entrando…' : 'Entrar'}
        </Button>
      </AuthCard>
    </>
  )
}
