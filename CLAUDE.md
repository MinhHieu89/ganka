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
- When there is 500-code error in the backend, check the logs before debugging. 

# LOGS

- Backend logs are written to `backend/src/Bootstrapper/logs/` (dev only, gitignored)
- Log files rotate daily with pattern `ganka-YYYYMMDD.log`
- Retained for 7 days, plain text format
- To tail logs: `Get-Content backend/src/Bootstrapper/logs/ganka-*.log -Tail 50 -Wait` (PowerShell) or `tail -f backend/src/Bootstrapper/logs/ganka-*.log` (bash)

# Test Account Credentials
- Username: Admin@ganka28.com
- Password: Admin@123456

# Workflow Orchestration

## 1. Plan Node Default
- Enter plan mode for ANY non-trivial task (3+ steps or architectural decisions)
- If something goes sideways, STOP and re-plan immediately – don't keep pushing
- Use plan mode for verification steps, not just building
- Write detailed specs upfront to reduce ambiguity

## 2. Subagent Strategy
- Use subagents liberally to keep main context window clean
- Offload research, exploration, and parallel analysis to subagents
- For complex problems, throw more compute at it via subagents
- One task per subagent for focused execution

## 3. Self-Improvement Loop
- After ANY correction from the user: update `tasks/lessons.md` with the pattern
- Write rules for yourself that prevent the same mistake
- Ruthlessly iterate on these lessons until mistake rate drops
- Review lessons at session start for relevant project

## 4. Verification Before Done
- Never mark a task complete without proving it works
- Diff behavior between main and your changes when relevant
- Ask yourself: "Would a staff engineer approve this?"
- Run tests, check logs, demonstrate correctness

## 5. Demand Elegance (Balanced)
- For non-trivial changes: pause and ask "is there a more elegant way?"
- If a fix feels hacky: "Knowing everything I know now, implement the elegant solution"
- Skip this for simple, obvious fixes – don't over-engineer
- Challenge your own work before presenting it

## 6. Autonomous Bug Fixing
- When given a bug report: just fix it. Don't ask for hand-holding
- Point at logs, errors, failing tests – then resolve them
- Zero context switching required from the user
- Go fix failing CI tests without being told how

## Task Management

1. **Plan First**: Write plan to `tasks/todo.md` with checkable items
2. **Verify Plan**: Check in before starting implementation
3. **Track Progress**: Mark items complete as you go
4. **Explain Changes**: High-level summary at each step
5. **Document Results**: Add review section to `tasks/todo.md`
6. **Capture Lessons**: Update `tasks/lessons.md` after corrections

## Core Principles

- **Simplicity First**: Make every change as simple as possible. Impact minimal code.
- **No Laziness**: Find root causes. No temporary fixes. Senior developer standards.
- **Minimal Impact**: Changes should only touch what's necessary. Avoid introducing bugs.