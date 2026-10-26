import { Compass, LogOut } from 'lucide-react'
import { ElementBadge } from '../../components/common/Badge'
import { Button, ButtonLink } from '../../components/common/Button'
import CharacterAvatar from '../../components/common/CharacterAvatar'
import { EmptyState } from '../../components/common/StatusMessage'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { elementStyle } from '../../utils/labels'
import styles from './ProfilePage.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' })
const numberFormat = new Intl.NumberFormat('pt-BR')

export default function ProfilePage() {
  const { user } = useAuth()
  const logout = useLogout()

  // RequireAuth garante a sessão; o null só existe durante o logout.
  if (!user) return null

  const avatarImage = user.avatarUrl ? { url: user.avatarUrl, sourceUrl: null, credit: null } : null

  return (
    <div className={styles.page}>
      <title>{`${user.username} · Avatar Codex`}</title>
      <header className={styles.hero} style={elementStyle(user.element?.color)}>
        <CharacterAvatar
          name={user.username}
          image={avatarImage}
          color={user.element?.color}
          size="lg"
        />
        <div className={styles.info}>
          <h1 className={styles.name}>{user.username}</h1>
          <p className={styles.email}>{user.email}</p>
          <dl className={styles.stats}>
            <div>
              <dt>Nível</dt>
              <dd>{user.level}</dd>
            </div>
            <div>
              <dt>Experiência</dt>
              <dd>{numberFormat.format(user.experiencePoints)} XP</dd>
            </div>
            <div>
              <dt>Elemento</dt>
              <dd>
                {user.element ? <ElementBadge element={user.element} /> : 'Ainda não definido'}
              </dd>
            </div>
            <div>
              <dt>Membro desde</dt>
              <dd>{dateFormat.format(new Date(user.createdAt))}</dd>
            </div>
          </dl>
        </div>
        <Button variant="ghost" className={styles.logout} onClick={logout}>
          <LogOut size={18} aria-hidden="true" />
          Sair
        </Button>
      </header>

      <EmptyState
        icon={Compass}
        title="Sua jornada está começando"
        description="Explore personagens e nações. Em breve, descobertas, favoritos e conquistas aparecerão aqui."
        action={<ButtonLink to="/characters">Explorar personagens</ButtonLink>}
      />
    </div>
  )
}
