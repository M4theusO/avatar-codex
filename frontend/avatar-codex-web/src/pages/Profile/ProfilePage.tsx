import { LogOut } from 'lucide-react'
import { Link } from 'react-router'
import { ElementBadge } from '../../components/common/Badge'
import { Button } from '../../components/common/Button'
import Section from '../../components/common/Section'
import DeleteAccount from '../../components/profile/DeleteAccount'
import EditProfile from '../../components/profile/EditProfile'
import ExperienceHistory from '../../components/profile/ExperienceHistory'
import ExplorationStats from '../../components/profile/ExplorationStats'
import ProfileHero from '../../components/profile/ProfileHero'
import { useAuth } from '../../hooks/useAuth'
import { useLogout } from '../../hooks/useLogout'
import { formatMemberSince } from '../../utils/profile'
import styles from './ProfilePage.module.css'

export default function ProfilePage() {
  const { user } = useAuth()
  const logout = useLogout()

  // RequireAuth garante a sessão; o null só existe durante o logout.
  if (!user) return null

  return (
    <div className={styles.page}>
      <title>{`${user.username} · Avatar Codex`}</title>
      <ProfileHero
        profile={user}
        subtitle={user.email}
        details={
          <>
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
              <dd>{formatMemberSince(user.createdAt)}</dd>
            </div>
          </>
        }
        action={
          <Button variant="ghost" onClick={logout}>
            <LogOut size={18} aria-hidden="true" />
            Sair
          </Button>
        }
      />

      <Section title="Exploração">
        <ExplorationStats />
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
