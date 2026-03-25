# Part 3 API Testing Runbook (Postman + Newman)

## What to use
- Collection: `api/postman/Mars-Part3-A2-A9.postman_collection.json`
- Environment: `api/postman/Mars-Part3.local.postman_environment.json`
- Data: `api/postman/data/part3-users.csv`
- Newman runner: `api/postman/newman/Run-Part3.ps1`

## Folder flow in collection
1. `A2-A9 Happy Path`
2. `Negative Cases`
3. `Destructive & Cleanup`

Run order is already top-to-bottom for submission flow.

## Variables used
- Auth: `authToken`
- Dynamic IDs: `createdListingId`, `listingId`
- Endpoints: `baseUrl`, `loginPath`, `addEducationPath`, etc.

## Exact Newman commands

### Option 1: one-command runner
```powershell
powershell -ExecutionPolicy Bypass -File api/postman/newman/Run-Part3.ps1 run
```

### Option 2: direct Newman command
```powershell
newman run api/postman/Mars-Part3-A2-A9.postman_collection.json --environment api/postman/Mars-Part3.local.postman_environment.json --iteration-data api/postman/data/part3-users.csv --timeout-request 15000 --reporters "cli,json,junit" --reporter-json-export api/postman/newman/reports/part3_api-report.json --reporter-junit-export api/postman/newman/reports/part3_api-junit.xml
```

### Optional HTML report (if reporter installed)
```powershell
newman run api/postman/Mars-Part3-A2-A9.postman_collection.json --environment api/postman/Mars-Part3.local.postman_environment.json --iteration-data api/postman/data/part3-users.csv --reporters "cli,htmlextra" --reporter-htmlextra-export api/postman/newman/reports/part3_api-htmlextra.html
```

## Endpoint assumptions (verify once in Chrome Network)
These are pre-wired and may need path updates in environment if your local API differs:
- `/authentication/authentication/signin`
- `/profile/profile/isUserAuthenticated`
- `/profile/profile/addEducation`
- `/profile/profile/addCertification`
- `/listing/listing/getCategories`
- `/listing/listing/addListing`
- `/listing/listing/searchListings`
- `/listing/listing/deleteListing/{id}`

## If endpoint differs, update here
- Edit `api/postman/Mars-Part3.local.postman_environment.json` path variables only.
- Do not hardcode inside requests.

## Excel-ready output
- Final CSV: `analysis/excel/final/Part3-API-Results-Final.csv`
- Final workbook: `analysis/excel/final/Part3-API-Results-Final.xlsx`
- Generated reports: `part3_api-report.json`, `part3_api-junit.xml`, `part3_api-report.html`

## Final submission checklist
- `docs/submission-evidence/Part3-Submission-Checklist.md`
