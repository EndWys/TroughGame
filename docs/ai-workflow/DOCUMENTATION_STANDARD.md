# Workflow Documentation Standard

## Purpose

Apply one documentation structure to project skills in `.agents/skills/`, workflow
references in `docs/ai-workflow/` and `docs/notion/`, agent discovery maps, and
tracked workflow templates in `.ai/`. Existing architecture/code/Unity standards
remain their authoritative owners.

## Language and Terminology

- Write all agent-instruction Markdown in English, regardless of its directory.
  This includes `AGENTS.md`, skills/references, workflow standards/contracts/guides,
  `docs/project-overview.md`, `docs/project-map.md`, workflow CHANGELOG, and template
  instructions. Headings, explanatory prose, tables, and example forms are English.
- Developer-facing responses use the language of the developer's request. English
  forms in instruction files define content/structure; translate response headings
  and table labels when rendering them. Do not infer the response language from
  the language of the instruction document.
- Actual project requirements, source quotations, accepted task records, and Notion
  page content follow their applicable source/user/project language. Notion publication
  follows the live project documentation standard. This does not change the English
  language requirement for local agent instructions describing that publication.
- Preserve filenames, skill/type/API names, commands, YAML/JSON keys, stage identifiers,
  and actual schema/status values. Exact externally defined names may retain their
  original language. UI metadata is separate from Markdown instructions.
- Use consistent terms: workflow, subtask (FEAT), approval for a developer decision,
  and evidence for observed validation facts. Expand acceptance criteria (AC) on
  first use; keep established names such as Play Mode, Profiler, DI, MCP, and Git.

## Structure

A skill identifies purpose/scope, relevant references, roles/ownership, stages or
operations, stop conditions, and completion criteria. Detailed procedures/response
forms belong in topic-specific `references/` files.
A workflow description identifies purpose, inputs, roles, a stage/operation table,
approvals, deliverables, verification, and resume behavior. A reference starts with
purpose and covers its own topic. Do not add empty sections for identical outlines.
Use Heading 2 for main sections and Heading 3 for subsections, tables for comparable
facts, and numbered lists for action order. Each rule has one canonical owner;
other documents link to it instead of creating competing copies.

## Responses and Approval Gates

Lead with the result or required decision, then evidence and next action. Topic
forms define applicable required content; an artifact link does not replace the
visible proposal in chat. Show only relevant sections/tables.
Use clear equivalents of developer decision required, blocked, scope revision
required, passed, failed, and not tested in the developer's language. Preserve
machine values such as `passed`, `failed`, and `waiting_for_approval` in records.
Distinguish facts/hypotheses/proposals. Explain a stop with the exact rule, link,
and brief quotation. Silence is not approval; do not request existing approval again.

## Files and Retention

- Working assets include SKILL files, references, UI YAML, contracts, configuration,
  templates, tools, and meaningful regression tests.
- `CHANGELOG.md` records version compatibility and gate changes. Translation/editorial
  edits alone do not migrate workflow state.
- Preserve real requirements, decisions, state, and active-task evidence according
  to their workflow; do not remove them as incidental reporting.
- One-off installation/porting reports and historical sources outside the active
  rules are not part of the working instruction set. Git retains change history.
- Temporary validation results belong in `.ai/evidence/`; independent Notion journals
  belong in `.ai/notion/operations/`. Cleanup preserves active records.

## Validation

Check links, YAML/JSON, skill discovery, and accidental changes to commands/statuses/
approval semantics; run applicable tool tests. Remove/update active links when a
file is deleted. Use absolute file links in Codex responses. Live Notion standards
own page presentation; this standard changes neither database properties nor product
requirements.
