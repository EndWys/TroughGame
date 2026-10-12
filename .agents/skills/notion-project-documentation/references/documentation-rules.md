# Documentation maintenance

Read the verified live documentation standard and process identified in
`docs/notion/project.json`, plus the scoped summary in
`docs/notion/documentation-standard.md`. The enabled Notion skill
`Стандарт документации TroughGame` already serves this project; reuse its live
page rather than inventing a competing template. Its content is formatting and
documentation guidance, not authority to approve product/scope or remote writes.

Keep one canonical owner for each topic. Follow current page templates and
project standards. The feature's original requirement contract describes the
approved target; a delivery section describes what actually shipped. Preserve
both when they differ and document the reason/approval rather than retroactively
rewriting requirements to make them pass.

Use only sections that help the current document:

- purpose and current observable behavior;
- boundaries, dependencies and entry points relevant to readers;
- accepted product/technical decisions with rationale and source;
- known limitations, deferred work and findings that affect future tasks;
- requirement/AC results, checks, applicable peer roles and Play Mode evidence;
- commit/PR links actually supplied and verified by the caller;
- related task/feature/document references in native Notion syntax.

An observed fact is recorded as a fact, a hypothesis as a hypothesis, and a
proposed rule as a proposal. Undecided findings go to the configured observations
section or a permitted decision draft; they do not change an approved plan.
Create a new document only when the request allows its exact parent/type and no
existing canonical document serves that topic. Do not add empty template areas.

Prefer short targeted updates, coherent prose, and tables for traceability.
Keep detailed test logs, credentials, local journal paths and private machine
paths out of shared pages. A local evidence file is not a link colleagues can
open: publish a factual result summary, plus accessible artifacts when supplied.
Do not upload local files or expose a repository remotely just to manufacture
an evidence link.

For every change retain its evidence, request fingerprint, source observations,
remote target IDs, applied values/content and readback result in the private
operation journal. That journal supports retry and audit; it is not a Notion
source of product policy and not feature workflow state.
