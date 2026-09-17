# Contributing to AgenticBoard

Thank you for your interest in contributing to **AgenticBoard**! This project demonstrates a spec-driven, AI-first software engineering methodology where code is built with high standards of architecture, security, and testing.

## Core Development Philosophy

1. **Spec-Driven**: No feature or significant architectural change begins with raw code. Every feature must trace back to a specification in `/specs` and an implementation plan in `/plans`.
2. **AI-Assisted, Human-Governed**: Automated coding agents must strictly operate under the guidance defined in [`AGENTS.md`](./AGENTS.md). AI does not replace engineering judgment.
3. **Quality & Validation**: Every feature must include automated tests, adherence to Clean Architecture, robust authorization, and a post-implementation review in `/reviews`.

## Branching & Commit Workflow

- **`main`**: Production-ready, stable releases.
- **`develop`**: Integration branch for completed and reviewed features.
- **`feature/<name>`**: Dedicated branch created off `develop` for each specific capability. Once all tests and reviews pass, merge back into `develop`.

### Commit Conventions

Use [Conventional Commits](https://www.conventionalcommits.org/):
- `chore: ...`
- `docs: ...`
- `plan: ...`
- `feat: ...`
- `test: ...`
- `review: ...`
- `fix: ...`
- `refactor: ...`

## Running Locally

Refer to the [README.md](./README.md) for step-by-step instructions for running via Docker Compose or using local `.NET 8` and `Angular 20` toolchains.
