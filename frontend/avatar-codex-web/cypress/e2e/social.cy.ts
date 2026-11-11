import type { TestUser } from '../support/commands'

const apiUrl = () => Cypress.expose('apiUrl') as string

/** Troca a sessão do navegador: o login pela API grava o cookie de refresh do outro usuário. */
function switchTo(user: TestUser) {
  cy.clearAllCookies()
  cy.request('POST', `${apiUrl()}/api/auth/login`, { email: user.email, password: user.password })
}

// Fase 23: perfil público e privacidade, com dois usuários.
describe('Perfil público', () => {
  it('outro usuário vê o perfil; depois de ficar privado, só o dono vê', () => {
    cy.newUser().then((owner) => {
      cy.newUser().then((viewer) => {
        cy.registerViaApi(owner)
        cy.clearAllCookies()
        cy.registerViaApi(viewer)

        // Público: quem está logado vê nível, estatísticas e conquistas, sem atalho de edição.
        cy.visit(`/users/${owner.username}`)
        cy.findByRole('heading', { level: 1, name: owner.username }).should('be.visible')
        cy.findByText('Nível 1').should('be.visible')
        cy.findByRole('heading', { name: 'Conquistas (1)' }).should('be.visible')
        cy.findByRole('heading', { level: 3, name: 'Primeiro passo' }).should('be.visible')
        cy.findByRole('link', { name: /Editar perfil/ }).should('not.exist')
        cy.contains(owner.email).should('not.exist')

        // O dono torna o perfil privado pela seção de edição.
        switchTo(owner)
        cy.visit('/profile')
        cy.findByRole('switch', { name: 'Perfil privado' }).should('not.be.checked').click()
        cy.findByText('Seu perfil agora é privado.').should('be.visible')
        cy.findByRole('switch', { name: 'Perfil privado' }).should('be.checked')
        cy.findByRole('link', { name: 'Ver meu perfil público' }).click()
        cy.location('pathname').should('eq', `/users/${owner.username}`)
        cy.findByText(/só você vê esta página/).should('be.visible')
        cy.findByRole('link', { name: /Editar perfil/ }).should('have.attr', 'href', '/profile')

        // Para os outros, privado é igual a inexistente.
        switchTo(viewer)
        cy.visit(`/users/${owner.username}`)
        cy.findByText('Perfil não encontrado').should('be.visible')
        cy.findByRole('heading', { level: 1, name: owner.username }).should('not.exist')
      })
    })
  })
})
