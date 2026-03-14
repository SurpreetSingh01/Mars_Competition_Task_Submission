# JMeter Assets

This folder contains a submission-ready JMeter scaffold for Part 2.

## Included Files

- `test-plans/Mars-Part2.jmx`
- `data/users.csv`
- `data/local.example.properties`

## Verified Endpoint Families

These were found in the live Mars client bundles:

- Login: `/authentication/authentication/signin`
- Auth check: `/profile/profile/isUserAuthenticated`
- Profile action: `/profile/profile/addEducation`
- Listing categories: `/listing/listing/getCategories`
- Add listing: `/listing/listing/addListing`
- Search listings: `/listing/listing/searchListings`
- Search page query: `/Home/Search?searchString=<value>`

## Recommended Plan Hierarchy

```text
Mars Part 2 Test Plan
  HTTP Request Defaults
  HTTP Cookie Manager
  HTTP Cache Manager
  CSV Data Set Config - Mars Users
  Default JSON Headers
  Thread Group - 100 Users
    Once Only - Open Home
    Transaction - Login
    Transaction - Profile Action
    Transaction - Share Skill
    Transaction - Search Skill
    Transaction - Logout
  Thread Group - 300 Users
    same flow
  Thread Group - 500 Users
    same flow
  Thread Group - Endurance
    same flow over time
  Thread Group - Volume
    same flow with high concurrency
```

## Important Verification Notes

- Verify service hosts and ports in browser DevTools first.
- The Mars UI bundles suggest some API calls may go to separate backend ports.
- The included JMX defaults all ports to `5003` so the plan can open cleanly.
- Before your real run, override the `auth_*`, `profile_*`, and `listing_*` properties in `local.properties`.
- Verify the final login response JSON path for the auth token.
- Verify the add-listing request body shape before your graded run.
- Verify logout in network trace first. The app appears to remove `marsAuthToken` client-side.

## Suggested Property File Workflow

1. Copy `data/local.example.properties` to `data/local.properties`.
2. Replace any host or port values after checking DevTools network traffic.
3. Run JMeter with `-q performance/jmeter/data/local.properties`.

## CLI Commands

100 users:

```powershell
jmeter -n -t performance/jmeter/test-plans/Mars-Part2.jmx -q performance/jmeter/data/local.properties -Jusers_100=100 -Jramp_100=120 -l performance/jmeter/results/run_100.jtl -e -o performance/jmeter/reports/run_100
```

300 users:

```powershell
jmeter -n -t performance/jmeter/test-plans/Mars-Part2.jmx -q performance/jmeter/data/local.properties -Jusers_300=300 -Jramp_300=240 -l performance/jmeter/results/run_300.jtl -e -o performance/jmeter/reports/run_300
```

500 users:

```powershell
jmeter -n -t performance/jmeter/test-plans/Mars-Part2.jmx -q performance/jmeter/data/local.properties -Jusers_500=500 -Jramp_500=300 -l performance/jmeter/results/run_500.jtl -e -o performance/jmeter/reports/run_500
```

Endurance:

```powershell
jmeter -n -t performance/jmeter/test-plans/Mars-Part2.jmx -q performance/jmeter/data/local.properties -Jusers_endurance=100 -Jramp_endurance=300 -Jendurance_duration=7200 -l performance/jmeter/results/endurance.jtl -e -o performance/jmeter/reports/endurance
```

Volume:

```powershell
jmeter -n -t performance/jmeter/test-plans/Mars-Part2.jmx -q performance/jmeter/data/local.properties -Jusers_volume=500 -Jramp_volume=120 -Jvolume_duration=900 -l performance/jmeter/results/volume.jtl -e -o performance/jmeter/reports/volume
```

## Correlation Strategy

- Keep `HTTP Cookie Manager` enabled for session cookies.
- Extract bearer token from login response.
- Reuse the extracted token in authenticated profile, listing, and search calls.
- If anti-forgery or hidden IDs appear in network requests, add a regex or JSON extractor before the dependent call.
- Verify response shape first before finalizing extractor names.

## One-command Scenario Runs

Use the helper script so only the intended thread group is active per run:

```powershell
powershell -ExecutionPolicy Bypass -File performance/jmeter/scripts/Run-Part2-Scenarios.ps1 -Scenario load100
powershell -ExecutionPolicy Bypass -File performance/jmeter/scripts/Run-Part2-Scenarios.ps1 -Scenario load300
powershell -ExecutionPolicy Bypass -File performance/jmeter/scripts/Run-Part2-Scenarios.ps1 -Scenario load500
powershell -ExecutionPolicy Bypass -File performance/jmeter/scripts/Run-Part2-Scenarios.ps1 -Scenario endurance
powershell -ExecutionPolicy Bypass -File performance/jmeter/scripts/Run-Part2-Scenarios.ps1 -Scenario volume
```
