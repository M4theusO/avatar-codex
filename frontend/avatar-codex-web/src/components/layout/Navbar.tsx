import {
  BookOpenCheck,
  Hourglass,
  House,
  Landmark,
  LayoutDashboard,
  Menu,
  Users,
  X,
} from 'lucide-react'
import { useState } from 'react'
import { Link, NavLink } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import styles from './Navbar.module.css'
import UserMenu from './UserMenu'

// Para quem está logado, o primeiro link leva ao painel; a Home continua no logo.
const homeLink = { to: '/', label: 'Início', icon: House, end: true }
const dashboardLink = { to: '/dashboard', label: 'Painel', icon: LayoutDashboard, end: false }

const sectionLinks = [
  { to: '/characters', label: 'Personagens', icon: Users, end: false },
  { to: '/nations', label: 'Nações', icon: Landmark, end: false },
  { to: '/quizzes', label: 'Quizzes', icon: BookOpenCheck, end: false },
  { to: '/timeline', label: 'Linha do tempo', icon: Hourglass, end: false },
]

export default function Navbar() {
  const { user } = useAuth()
  const [menuOpen, setMenuOpen] = useState(false)
  const links = [user ? dashboardLink : homeLink, ...sectionLinks]
  const closeMenu = () => setMenuOpen(false)

  return (
    <header className={styles.header}>
      <div className={styles.bar}>
        <Link to="/" className={styles.brand} onClick={closeMenu}>
          <span className={styles.brandMark} aria-hidden="true" />
          Avatar Codex
        </Link>

        <button
          type="button"
          className={styles.menuButton}
          onClick={() => setMenuOpen((open) => !open)}
          aria-expanded={menuOpen}
          aria-controls="main-navigation"
          aria-label={menuOpen ? 'Fechar menu' : 'Abrir menu'}
        >
          {menuOpen ? <X size={22} aria-hidden="true" /> : <Menu size={22} aria-hidden="true" />}
        </button>

        <nav
          id="main-navigation"
          className={`${styles.nav} ${menuOpen ? styles.navOpen : ''}`}
          aria-label="Principal"
        >
          <ul className={styles.list} aria-label="Seções">
            {links.map(({ to, label, icon: Icon, end }) => (
              <li key={to}>
                <NavLink
                  to={to}
                  end={end}
                  className={({ isActive }) => `${styles.link} ${isActive ? styles.active : ''}`}
                  onClick={closeMenu}
                >
                  <Icon size={18} aria-hidden="true" />
                  {label}
                </NavLink>
              </li>
            ))}
          </ul>
          <div className={styles.session}>
            <UserMenu onNavigate={closeMenu} />
          </div>
        </nav>
      </div>
    </header>
  )
}
