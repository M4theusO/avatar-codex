import { Outlet, ScrollRestoration } from 'react-router'
import styles from './AppLayout.module.css'
import Navbar from './Navbar'

export default function AppLayout() {
  return (
    <div className={styles.app}>
      <a href="#main-content" className={styles.skipLink}>
        Pular para o conteúdo
      </a>
      <Navbar />
      <main id="main-content" className={styles.main} tabIndex={-1}>
        <Outlet />
      </main>
      <footer className={styles.footer}>
        <p>
          Avatar Codex é um projeto de portfólio feito por fã, sem fins comerciais. Avatar: The Last
          Airbender e The Legend of Korra pertencem aos seus respectivos detentores de direitos.
        </p>
      </footer>
      <ScrollRestoration />
    </div>
  )
}
