import { Link } from 'react-router'
import AuthCard from '../../components/auth/AuthCard'
import { Button } from '../../components/common/Button'
import TextField from '../../components/common/TextField'
import { useAuth } from '../../hooks/useAuth'
import { useAuthForm } from '../../hooks/useAuthForm'

export default function RegisterPage() {
  const { register } = useAuth()
  const { bind, handleSubmit, formError, submitting, redirectParam } = useAuthForm(
    { username: '', email: '', password: '', confirmPassword: '' },
    register,
    (user) => `Conta criada! Boas-vindas, ${user.username}.`,
  )

  const loginLink = redirectParam
    ? `/login?redirect=${encodeURIComponent(redirectParam)}`
    : '/login'

  return (
    <>
      <title>Criar conta · Avatar Codex</title>
      <AuthCard
        title="Criar conta"
        description="Salve favoritos, ganhe XP e acompanhe sua evolução."
        error={formError}
        onSubmit={handleSubmit}
        footer={
          <>
            Já tem conta? <Link to={loginLink}>Entrar</Link>
          </>
        }
      >
        <TextField
          label="Nome de usuário"
          autoComplete="username"
          hint="De 3 a 30 caracteres: letras, números, ponto, hífen ou sublinhado."
          {...bind('username')}
        />
        <TextField label="Email" type="email" autoComplete="email" {...bind('email')} />
        <TextField
          label="Senha"
          type="password"
          autoComplete="new-password"
          hint="Mínimo de 8 caracteres, com letra e número."
          {...bind('password')}
        />
        <TextField
          label="Confirme a senha"
          type="password"
          autoComplete="new-password"
          {...bind('confirmPassword')}
        />
        <Button type="submit" variant="primary" disabled={submitting}>
          {submitting ? 'Criando conta…' : 'Criar conta'}
        </Button>
      </AuthCard>
    </>
  )
}
