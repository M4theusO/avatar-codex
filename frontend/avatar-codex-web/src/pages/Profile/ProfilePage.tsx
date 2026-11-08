import { LogOut } from 'lucide-react'
import { Link } from 'react-router'
import { ElementBadge } from '../../components/common/Badge'
import { Button } from '../../components/common/Button'
import CharacterAvatar from '../../components/common/CharacterAvatar'
import Section from '../../components/common/Section'
import DeleteAccount from '../../components/profile/DeleteAccount'
import EditProfile from '../../components/profile/EditProfile'
import ExperienceHistory from '../../components/profile/ExperienceHistory'
import ExplorationStats from '../../components/profile/ExplorationStats'
import XpBar from '../../components/progress/XpBar'
import RecentDiscoveries from '../../components/profile/RecentDiscoveries'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { elementStyle } from '../../utils/labels'
import { userAvatarImage } from '../../utils/userAvatar'
import styles from './ProfilePage.module.css'

const dateFormat = new Intl.DateTimeFormat('pt-BR', { dateStyle: 'long' })

export default function ProfilePage() {
  const { user } = useAuth()
  const logout = useLogout()

  // RequireAuth garante a sessão; o null só existe durante o logout.
  if (!user) return null

  return (
    <div className={styles.page}>
      <title>{`${user.username} · Avatar Codex`}</title>
      <header className={styles.hero} style={elementStyle(user.element?.color)}>
        <CharacterAvatar
          name={user.username}
          image={userAvatarImage(user.avatarUrl)}
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
              <dd className={styles.element}>
                {user.element ? (
                  <>
                    <ElementBadge element={user.element} />
                    <Link to="/element-test" className={styles.elementLink}>
                      Refazer o teste
                    </Link>
                  </>
                ) : (
                  <>
                    Ainda não definido
                    <Link to="/element-test" className={styles.elementLink}>
                      Descobrir meu elemento
                    </Link>
                  </>
                )}
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

      <Section title="Editar perfil">
        <EditProfile />
      </Section>

      <Section title="Excluir conta">
        <DeleteAccount />
      </Section>
    </div>
  )
}
