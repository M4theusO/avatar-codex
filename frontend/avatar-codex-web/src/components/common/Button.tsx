import type { ButtonHTMLAttributes } from 'react'
import { Link, type LinkProps } from 'react-router'
import styles from './Button.module.css'

type Variant = 'primary' | 'secondary' | 'ghost'

function buttonClass(variant: Variant, className?: string) {
  return [styles.button, styles[variant], className].filter(Boolean).join(' ')
}

type ButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & { variant?: Variant }

/** Ação na página. Para navegação, use `ButtonLink`. */
export function Button({
  variant = 'secondary',
  className,
  type = 'button',
  ...props
}: ButtonProps) {
  return <button type={type} className={buttonClass(variant, className)} {...props} />
}

type ButtonLinkProps = LinkProps & { variant?: Variant }

export function ButtonLink({ variant = 'secondary', className, ...props }: ButtonLinkProps) {
  return <Link className={buttonClass(variant, className)} {...props} />
}
