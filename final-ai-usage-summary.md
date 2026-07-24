# Final AI Usage Summary

## Overview
This project was built entirely with Cursor as the AI development
tool, across roughly 19 feature and documentation prompts, organized
into 7 sequential, independently verified and merged Git branches/pull
requests, plus a further round of lifecycle documentation generation.

## Scope of AI Involvement
AI was used for: solution scaffolding, domain modeling, database
configuration and migrations, seed data, authentication and role-based
access control, business feature implementation (quotations, PDF
generation, settings, search, dashboard), test generation, bug fixing,
and documentation drafting grounded in the actual codebase.

## Human Judgment Applied
Every AI-generated feature was manually verified before being accepted
as complete: logged into the running application as multiple seeded
users, cross-checked business calculations by hand, inspected the
database directly rather than trusting console output, and reviewed
code diffs against established architectural rules before committing.
Six genuine defects were found and fixed this way, all documented with
root cause and resolution in debugging-notes.md. Decisions about scope
(e.g. not building a separate public self-service tenant registration
feature, not adding a full REST API layer) were made deliberately to
stay within Core/Stretch requirements and preserve time for
documentation, rather than accepting every technically-possible
addition Cursor could have built.

## Overall Assessment
AI meaningfully accelerated implementation speed once architectural
context was established, but required consistent, active verification
throughout — several defects (incomplete multi-part seed data, a
recurring concurrency anti-pattern, a missing DI registration, a font
rendering bug) would have shipped silently without deliberate manual
testing against real application behavior rather than trust in
AI-reported success.