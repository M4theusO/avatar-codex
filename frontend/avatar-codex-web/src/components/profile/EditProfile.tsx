import { AlertCircle, Camera, Trash2 } from 'lucide-react'
import { useId, useRef, useState, type ChangeEvent, type FormEvent } from 'react'
import { Link } from 'react-router'
import { useAuth } from '../../hooks/useAuth'
import {
  useRemoveAvatar,
  useUpdatePrivacy,
  useUpdateUsername,
  useUploadAvatar,
} from '../../hooks/useProfile'
import { useToast } from '../../hooks/useToast'
import { getApiError } from '../../utils/apiError'
import { profilePath } from '../../utils/profile'
import { ACCEPTED_TYPES, checkAvatarFile, userAvatarImage } from '../../utils/userAvatar'
import { Button } from '../common/Button'
import CharacterAvatar from '../common/CharacterAvatar'
import TextField from '../common/TextField'
import styles from './EditProfile.module.css'

/** Seção do perfil para trocar a foto e o nome de usuário. As regras ficam no backend. */
export default function EditProfile() {
  const { user } = useAuth()
  if (!user) return null
  return (
    <div className={styles.panel}>
      <PhotoEditor />
      <UsernameForm key={user.username} current={user.username} />
      <PrivacyToggle />
    </div>
  )
}

function PhotoEditor() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const upload = useUploadAvatar()
  const remove = useRemoveAvatar()
  const input = useRef<HTMLInputElement>(null)
  const [error, setError] = useState<string>()
  const busy = upload.isPending || remove.isPending

  if (!user) return null

  const choose = async (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = '' // permite escolher o mesmo arquivo de novo
    if (!file) return
    setError(undefined)
    const problem = checkAvatarFile(file)
    if (problem) {
      setError(problem)
      return
    }
    try {
      await upload.mutateAsync(file)
      showToast('Foto atualizada.', 'success')
    } catch (failure) {
      const { message, fieldErrors } = getApiError(failure)
      setError(fieldErrors.file ?? message)
    }
  }

  const removePhoto = async () => {
    setError(undefined)
    try {
      await remove.mutateAsync()
      showToast('Foto removida.', 'info')
    } catch (failure) {
      setError(getApiError(failure).message)
    }
  }

  return (
    <div className={styles.photo}>
      <CharacterAvatar
        name={user.username}
        image={userAvatarImage(user.avatarUrl)}
        element={user.element}
        size="lg"
      />
      <div className={styles.photoBody}>
        <h3 className={styles.label}>Foto de perfil</h3>
        <p className={styles.hint}>
          JPEG, PNG ou WebP. A foto é recortada no centro e substitui a anterior.
        </p>
        <input
          ref={input}
          type="file"
          accept={ACCEPTED_TYPES.join(',')}
          className="visually-hidden"
          onChange={choose}
          tabIndex={-1}
          aria-label="Escolher foto de perfil"
        />
        <div className={styles.actions}>
          <Button onClick={() => input.current?.click()} disabled={busy}>
            <Camera size={18} aria-hidden="true" />
            {upload.isPending ? 'Enviando…' : user.avatarUrl ? 'Trocar foto' : 'Enviar foto'}
          </Button>
          {user.avatarUrl && (
            <Button variant="ghost" onClick={removePhoto} disabled={busy}>
              <Trash2 size={18} aria-hidden="true" />
              {remove.isPending ? 'Removendo…' : 'Remover foto'}
            </Button>
          )}
        </div>
        {error && (
          <p className={styles.alert} role="alert">
            <AlertCircle size={18} aria-hidden="true" />
            {error}
          </p>
        )}
      </div>
    </div>
  )
}

function UsernameForm({ current }: { current: string }) {
  const { showToast } = useToast()
  const update = useUpdateUsername()
  const [username, setUsername] = useState(current)
  const [error, setError] = useState<string>()
  const unchanged = username.trim() === current

  const submit = async (event: FormEvent) => {
    event.preventDefault()
    setError(undefined)
    try {
      await update.mutateAsync(username.trim())
      showToast('Nome de usuário atualizado.', 'success')
    } catch (failure) {
      const { message, fieldErrors } = getApiError(failure)
      setError(fieldErrors.username ?? message)
    }
  }

  return (
    <form className={styles.form} onSubmit={submit} noValidate>
      <TextField
        label="Nome de usuário"
        value={username}
        onChange={(event) => setUsername(event.target.value)}
        hint="3 a 30 caracteres: letras sem acento, números, ponto, hífen ou sublinhado. Você continua entrando pelo email."
        error={error}
        autoComplete="username"
        maxLength={30}
        required
      />
      <Button type="submit" variant="primary" disabled={unchanged || update.isPending}>
        {update.isPending ? 'Salvando…' : 'Salvar nome'}
      </Button>
    </form>
  )
}

/** Liga/desliga o perfil privado na hora; o estado vem do usuário devolvido pela API. */
function PrivacyToggle() {
  const { user } = useAuth()
  const { showToast } = useToast()
  const update = useUpdatePrivacy()
  const [error, setError] = useState<string>()
  const hintId = useId()

  if (!user) return null

  const toggle = async (event: ChangeEvent<HTMLInputElement>) => {
    setError(undefined)
    const isPrivate = event.target.checked
    try {
      await update.mutateAsync(isPrivate)
      showToast(
        isPrivate ? 'Seu perfil agora é privado.' : 'Seu perfil agora é visível.',
        'success',
      )
    } catch (failure) {
      setError(getApiError(failure).message)
    }
  }

  return (
    <div className={styles.privacy}>
      <h3 className={styles.label}>Visibilidade</h3>
      <label className={styles.switch}>
        <input
          type="checkbox"
          role="switch"
          checked={user.isProfilePrivate}
          onChange={toggle}
          disabled={update.isPending}
          aria-describedby={hintId}
        />
        Perfil privado
      </label>
      <p id={hintId} className={styles.hint}>
        {user.isProfilePrivate
          ? 'Só você vê seu perfil público. No ranking, seu nome continua aparecendo, mas sem link.'
          : 'Quem está logado pode abrir seu perfil pelo ranking: nível, elemento, estatísticas e conquistas. Email e histórico nunca aparecem.'}
      </p>
      <Link to={profilePath(user.username)} className={styles.publicLink}>
        Ver meu perfil público
      </Link>
      {error && (
        <p className={styles.alert} role="alert">
          <AlertCircle size={18} aria-hidden="true" />
          {error}
        </p>
      )}
    </div>
  )
}
