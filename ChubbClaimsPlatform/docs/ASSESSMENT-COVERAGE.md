# Assessment Coverage

| Assessment concern | MVP response |
|---|---|
| Claimants report incidents | `POST /api/claims` and claimant form |
| Claimants track claims | `GET /api/claims/mine`, status field and history endpoint |
| Claimants provide additional information | `POST /api/claims/{id}/provide-information` |
| Claimants receive decisions | status visible in claimant claim list |
| Officers pick up incoming claims | officer queue and assign endpoint |
| Officers review and assess | claim detail API and assessment workflow state |
| Progress to settlement or rejection | approve, reject and settle endpoints |
| Team workload and performance | officer dashboard summary (basic workload view) |
| Outstanding liability exposure | dashboard aggregate |
| Same Angular app for both roles | role-aware shared SPA |
| REST and Kafka decision | REST implemented; Kafka integration boundary documented |

## Not implemented in this time-boxed MVP
Actual document binary upload, Kafka broker and consumers, multi-currency FX conversion, detailed officer performance metrics, policy coverage integration, formal reserve calculations, manager-specific role, and production-grade identity management.
