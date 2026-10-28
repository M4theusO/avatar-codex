import { LogOut } from 'lucide-react'
import { ElementBadge } from '../../components/common/Badge'
import { Button } from '../../components/common/Button'
import CharacterAvatar from '../../components/common/CharacterAvatar'
import Section from '../../components/common/Section'
import ExperienceHistory from '../../components/profile/ExperienceHistory'
import ExplorationStats from '../../components/profile/ExplorationStats'
import XpBar from '../../components/progress/XpBar'
import RecentDiscoveries from '../../components/profile/RecentDiscoveries'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { elementStyle } from '../../utils/labels'
import styles from './ProfilePage.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' })

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
          element={user.element}
          size="lg"
        />
        <div className={styles.info}>
          <h1 className={styles.name}>{user.username}</h1>
          <p className={styles.email}>{user.email}</p>
          <div className={styles.xp}>
            <XpBar user={user} />
          </div>
          <dl className={styles.stats}>
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

      <Section title="Exploração">
        <ExplorationStats />
      </Section>

      <Section title="Descobertas recentes">
        <RecentDiscoveries />
      </Section>

      <Section title="Histórico de XP">
        <ExperienceHistory />
      </Section>
    </div>
  )
}
