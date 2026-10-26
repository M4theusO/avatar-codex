import { useQuery } from '@tanstack/react-query'
import { getHealth } from '../../services/healthService'
import styles from './HomePage.module.css'

export default function HomePage() {
  const health = useQuery({ queryKey: ['health'], queryFn: getHealth, retry: false })

  let statusText = 'Verificando a API…'
  if (health.isError) statusText = 'API indisponível'
  else if (health.data?.status === 'Healthy') statusText = 'API e banco de dados conectados'
  else if (health.data) statusText = 'API no ar, banco de dados indisponível'

  return (
    <main className={styles.page}>
      <h1 className={styles.title}>Avatar Codex</h1>
      <p className={styles.subtitle}>Enciclopédia interativa do universo de Avatar.</p>
      <p className={styles.status} role="status">
        {statusText}
      </p>
    </main>
  )
}
