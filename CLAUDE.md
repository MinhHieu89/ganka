# GENERAL

- Always use Context7 MCP when I need library/API documentation, code generation, setup or configuration steps without me having to explicitly ask.
- Use frontend design skill, vercel-react-best-practices skill and web-design-guidelines skill for any frontend tasks without me having to explicitly ask.
- Whenever you need human verification make sure you run backend at port 5255 and frontend at port 3000. Try to kill other processes if those ports are in use.

# FRONTEND

- Always use shadcn/ui components where applicable. For example, you need to use card component instead of draw a card yourself.
- If shadcn/ui does not have components that meet our UI/UX requirement, we can use components from https://mantine.dev. Make sure we create wrapper for these components and the styling needs to meet the theme of application.
- Only use libraries that are free, if they are not, you need to ask me about it.
- Only add placeholder where it makes sense, don't add it to all inputs by default.

# BACKEND

- Apply Testing Driven Design (TDD) strictly: write failing tests first, then implement (red-green-refactor).
- Make sure to have at least 80% code coverage.
- When you make changes to the models, make sure remember to create migrations and run them to update database schema.
- Whenever there is lock file issue due to running backend, stop the backend then continue.

# Test Account Credentials
- Username: Admin@ganka28.com
- Password: Admin@123456