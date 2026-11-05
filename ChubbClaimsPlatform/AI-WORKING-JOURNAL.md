# AI Working Journal

Keep this file truthful and update it during implementation. The assessment explicitly asks for prompts, accepted suggestions, challenged suggestions and overrides.

## Entry 1 — Initial decomposition
- Asked AI: Break down the supplied Chubb APAC assessment into a buildable MVP.
- Accepted: Prioritise a complete claim lifecycle over a large number of disconnected screens.
- Reasoning: The brief says to prioritise ruthlessly and start with the backend.
- Challenged / changed: Avoided implementing microservices and a Kafka cluster within the timebox.
- Reasoning: A modular monolith is easier to run locally and demonstrate; the event boundary can be explained.

## Entry 2 — Technology and local run strategy
- Asked AI: Recommend a stack and local setup.
- Accepted: Angular frontend, ASP.NET Core 8 API, EF Core, JWT and Swagger.
- Changed: SQLite is the default local database rather than requiring a SQL Server installation.
- Reasoning: It reduces setup friction for the evaluator. SQL Server remains a deployment/configuration option.

## Entry 3 — Workflow
- Asked AI: Define claim lifecycle and transitions.
- Accepted: Submitted → UnderReview → InformationRequested / Approved / Rejected; Approved → Settled.
- Reasoning: Explicit transition endpoints prevent arbitrary status changes.
- Validation still required: Confirm the real Chubb business workflow and whether additional states are needed.

## Candidate-owned entries
Add the actual prompts you use, code you review, manual changes you make, and test results. Do not claim code was tested unless you ran the tests.
