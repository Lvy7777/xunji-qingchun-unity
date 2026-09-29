# AI Collaboration Workflow

## Roles

- **ChatGPT (Lead):** defines goals, priorities, acceptance criteria, and task boundaries.
- **Codex (Executor):** inspects the repository, implements the approved task, verifies the result, and reports evidence.
- **User:** approves scope-changing decisions and controls releases, destructive operations, and other high-impact actions.

## Source of Truth

- `AI_MASTER_PLAN.md` records the project-level plan and priorities.
- `AI_CURRENT_TASK.md` records the single task currently authorized for execution.
- `AI_CODEX_REPORT.md` records implementation results, verification, and blockers.

## Working Cycle

1. ChatGPT updates the master plan when project priorities change.
2. ChatGPT defines one bounded task in `AI_CURRENT_TASK.md`, including acceptance criteria and exclusions.
3. Codex inspects the relevant files before making changes and preserves unrelated work.
4. Codex implements only the authorized scope.
5. Codex runs checks appropriate to the change and records the results in `AI_CODEX_REPORT.md`.
6. ChatGPT reviews the report and either accepts the result or prepares the next task.

## Guardrails

- Do not begin work that is absent from `AI_CURRENT_TASK.md`.
- Do not modify Unity scenes, scripts, packages, or project settings unless the current task explicitly authorizes it.
- Do not overwrite unrelated user changes.
- Stop and report a blocker when required information or authority is missing.
- Keep commits focused and describe exactly what changed.

