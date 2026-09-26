# JOURNAL.md — Architecture Journal

Informal running log of decisions and trade-offs made while building this harness,
kept for the optional +10% bonus component.

---

**On choosing the demonstration feature.** Of the four suggested features in
Section 3.4, I picked shift-handover bulk update over the SLA-breach alerting
feature specifically because it exercises the event-bus rule (Rule 3) end-to-end
in a way that's easy to demonstrate deterministically — a time-based SLA breach
check would have needed either a real clock dependency or a fake-clock test
harness to demonstrate convincingly within this capstone's scope, and I wanted the
demonstration run to be about proving the *governance* pattern, not about building
scheduling infrastructure.

**On why Reports stayed a stub.** Section 3.4's regional rollup report was the
other strong candidate feature, and I nearly built it instead. I chose not to,
because it would have made Sprint 1 do double duty — proving the harness *and*
proving a genuinely more complex aggregation feature — and I'd rather have one
sprint that cleanly demonstrates every rubric dimension than two sprints where the
second one is thinner on tests because of time budget. REFLECTION.md's "one
concrete improvement" is deliberately scoped to something achievable in a follow-up
sprint rather than something that requires re-scoping this one.

**On the DI lifetime bug class.** While writing `coding-conventions/SKILL.md`, I
realized the repository-must-be-Singleton rule isn't obvious from first principles
in ASP.NET Core — `Scoped` is the more commonly-reached-for default for anything
DI-registered, and it's the kind of mistake that compiles cleanly and only shows up
as "my data disappeared between requests" during manual testing, not during
`dotnet build`. That's exactly the kind of project-specific tacit knowledge a skill
file should encode explicitly rather than assume a Generator will infer correctly —
so I wrote the "why" (in-memory storage has no other source of truth) alongside the
rule, not just the rule.

**On what I'd do differently with more time.** The escalation path (CLAUDE.md
Section 4) is fully specified but never actually exercised in this repository,
because Sprint 1 passed on the first iteration. If I had a second block of time, I
would deliberately write a sprint contract I expect to fail twice (e.g. one with an
acceptance criterion that's subtly underspecified, like "the bulk endpoint should
be fast") specifically to generate a real `escalation.md` and prove the
three-strikes routing logic in CLAUDE.md actually fires as designed, rather than
only existing as documentation.
