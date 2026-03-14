# Part 2 – Reqnroll Automation and JMeter Performance Testing

## Overview
This section contains the implementation of automation using Reqnroll (Cucumber for .NET) and performance testing using JMeter.

## Tools Used
- Reqnroll
- Selenium WebDriver
- C#
- JMeter
- Visual Studio

## Project Structure

Part2_Reqnroll_JMeter
- Automation – Reqnroll automation framework
- ManualTests – Manual test case documentation
- JMeter – JMeter test plan files
- Results – Performance testing results and analysis
- Screenshots – Execution evidence

## Automation Features
- BDD implementation using feature files
- Step definitions written in C#
- Selenium WebDriver integration
- Page Object Model structure
- Data driven testing using JSON files

## Performance Testing
JMeter was used to simulate load testing scenarios including:
- 100 virtual users
- 300 virtual users
- 500 virtual users

The results include response time analysis and performance reports.

## How to Run Automation
1. Open the automation project in Visual Studio
2. Build the project
3. Execute tests using the Reqnroll test runner

## How to Run JMeter Tests
1. Open the `.jmx` file in Apache JMeter
2. Configure the thread group if needed
3. Run the test plan to generate performance results