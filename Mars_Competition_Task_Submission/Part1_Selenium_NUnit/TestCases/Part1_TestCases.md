# Part 1 Test Cases

Test suite executed: `dotnet test .\MarsAutomation\MarsAutomation.csproj`
Execution date: 2026-03-14
Execution result: 9 Passed, 0 Failed, 0 Skipped
Execution duration: 8 minutes 33 seconds

| ID | Automated Test | Purpose | Test Data | Expected Result | Latest Result |
| --- | --- | --- | --- | --- | --- |
| TC01 | SuccessfulLogin_UsingJsonData | Verify valid login using JSON credentials. | `login.username`, `login.password` from `TestData.json` | User logs in and reaches Profile or Home page. | Pass |
| TC02 | InvalidLogin_ShowsError | Verify invalid login shows an error message. | Invalid email/password hardcoded in test | Error message is displayed for failed login. | Pass |
| TC03 | CanAddUpdateAndDeleteLanguage | Verify a language can be added, updated, and deleted from Profile. | `languages.language`, `languages.level` from `TestData.json` | Language CRUD flow succeeds in Profile. | Pass |
| TC04 | CanUpdateProfileFields | Placeholder profile test in current automation project. | Existing logged-in profile session | Test currently passes via placeholder assertion in code. | Pass (placeholder) |
| TC05 | SearchByKeyword_ReturnsResults | Verify marketplace search returns results for a keyword. | `search.keyword` from `TestData.json` | Search results are displayed for keyword query. | Pass |
| TC06 | SearchByCategory_ReturnsResults | Verify marketplace search returns results for a category. | `search.category` from `TestData.json` | Search results are displayed for selected category. | Pass |
| TC07 | CanAddUpdateAndDeleteSkill | Verify a skill can be added, updated, and deleted from Profile. | Skill name `C#`, levels `Beginner` to `Expert` from test | Skill CRUD flow succeeds in Profile. | Pass |
| TC08 | CanCreateAndDeleteListing | Verify a service listing can be created and then deleted. | Dynamic listing title, description `Automated listing`, `search.category`, `skills.skillName` | Listing is saved successfully and can be removed from Manage Listings. | Pass |
| TC09 | ValidateRequiredFields | Verify Share Skill form validation appears when required fields are empty. | Empty submission | Validation messages are shown and save is blocked. | Pass |
