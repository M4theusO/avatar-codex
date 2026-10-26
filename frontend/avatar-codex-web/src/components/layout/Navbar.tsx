import { Hourglass, House, Landmark, Menu, Users, X } from 'lucide-react'
import { useState } from 'react'
import { Link, NavLink } from 'react-router'
import styles from './Navbar.module.css'

const links = [
  { to: '/', label: 'Início', icon: House, end: true },
  { to: '/characters', label: 'Personagens', icon: Users, end: false },
  { to: '/nations', label: 'Nações', icon: Landmark, end: false },
  { to: '/timeline', label: 'Linha do tempo', icon: Hourglass, end: false },
]

export default function Navbar() {
  const [menuOpen, setMenuOpen] = useState(false)
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
          <ul className={styles.list}>
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
        </nav>
      </div>
    </header>
  )
}
