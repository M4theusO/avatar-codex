// Fluxos 1 e 2: criar conta e entrar.
describe('Conta', () => {
  it('cria a conta pelo formulário e cai no painel com a conquista "Primeiro passo"', () => {
    cy.newUser().then((user) => {
      cy.visit('/register')
      cy.findByLabelText('Nome de usuário').type(user.username)
      cy.findByLabelText('Email').type(user.email)
      cy.findByLabelText('Senha').type(user.password)
      cy.findByLabelText('Confirme a senha').type(user.password)
      cy.findByRole('button', { name: 'Criar conta' }).click()

      cy.location('pathname').should('eq', '/dashboard')
      cy.findByText(/Conquista desbloqueada — Primeiro passo! \+10 XP/).should('be.visible')
      cy.findByRole('heading', { level: 1 }).should('contain.text', user.username)
    })
  })

  it('entra com email e senha, mantém a sessão ao recarregar e sai', () => {
    cy.newUser().then((user) => {
      cy.registerViaApi(user)
      cy.clearAllCookies()

      cy.visit('/login')
      cy.findByLabelText('Email').type(user.email)
      cy.findByLabelText('Senha').type(user.password)
      cy.findByRole('button', { name: 'Entrar' }).click()

      cy.location('pathname').should('eq', '/dashboard')
      cy.findByRole('heading', { level: 1 }).should('contain.text', user.username)

      // A sessão volta pelo cookie de refresh depois de recarregar.
      cy.reload()
      cy.findByRole('heading', { level: 1 }).should('contain.text', user.username)

      cy.findByRole('button', { name: 'Sair' }).click()
      cy.location('pathname').should('eq', '/')
      cy.findAllByRole('link', { name: 'Entrar' }).should('have.length.at.least', 1)
    })
  })

  it('senha errada mostra a mensagem da API sem entrar', () => {
    cy.newUser().then((user) => {
      cy.registerViaApi(user)
      cy.clearAllCookies()

      cy.visit('/login')
      cy.findByLabelText('Email').type(user.email)
      cy.findByLabelText('Senha').type('SenhaErrada1')
      cy.findByRole('button', { name: 'Entrar' }).click()

      cy.findByRole('alert').should('be.visible')
      cy.location('pathname').should('eq', '/login')
    })
  })

  it('exclui a conta pelo perfil, confirmando a senha', () => {
    cy.newUser().then((user) => {
      cy.registerViaApi(user)
      cy.visit('/profile')

      cy.findByRole('button', { name: 'Excluir conta' }).click()
      cy.findByRole('dialog', { name: 'Excluir conta?' }).within(() => {
        cy.findByLabelText('Senha').type('SenhaErrada1')
        cy.findByRole('button', { name: 'Excluir definitivamente' }).click()
        cy.findByText('Senha incorreta.').should('be.visible')

        cy.findByLabelText('Senha').clear().type(user.password)
        cy.findByRole('button', { name: 'Excluir definitivamente' }).click()
      })

      cy.location('pathname').should('eq', '/')
      cy.findByText('Sua conta foi excluída.').should('be.visible')
      cy.request({
        method: 'POST',
        url: `${Cypress.expose('apiUrl')}/api/auth/login`,
        body: { email: user.email, password: user.password },
        failOnStatusCode: false,
      })
        .its('status')
        .should('eq', 401)
    })
  })
})
