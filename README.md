# GitTank

[![.NET Core Desktop](https://github.com/Yuriy-Pelekh/GitTank/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Yuriy-Pelekh/GitTank/actions/workflows/dotnet-desktop.yml)

[![SonarCloud](https://sonarcloud.io/images/project_badges/sonarcloud-white.svg)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)

[![Quality gate](https://sonarcloud.io/api/project_badges/quality_gate?project=Yuriy-Pelekh_GitTank)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)

[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Maintainability Rating](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=sqale_rating)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Lines of Code](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=ncloc)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=coverage)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Technical Debt](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=sqale_index)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Reliability Rating](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=reliability_rating)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Duplicated Lines (%)](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=duplicated_lines_density)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Vulnerabilities](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=vulnerabilities)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=Yuriy-Pelekh_GitTank&metric=bugs)](https://sonarcloud.io/summary/new_code?id=Yuriy-Pelekh_GitTank)

Version | Build Status | Tests | Maintainability | Test Coverage
------------ | ------------- | ------------- | ------------- | -------------
Latest build | [![Build status](https://ci.appveyor.com/api/projects/status/a6t2412jpppsrern?svg=true)](https://ci.appveyor.com/project/Yuriy-Pelekh/gittank) | [![codecov](https://codecov.io/gh/Yuriy-Pelekh/GitTank/graph/badge.svg?token=3DFPOFMG80)](https://codecov.io/gh/Yuriy-Pelekh/GitTank) | |
Stable build | [![Build status](https://ci.appveyor.com/api/projects/status/a6t2412jpppsrern/branch/main?svg=true)](https://ci.appveyor.com/project/Yuriy-Pelekh/gittank/branch/main) | [![codecov](https://codecov.io/gh/Yuriy-Pelekh/GitTank/branch/main/graph/badge.svg?token=3DFPOFMG80)](https://codecov.io/gh/Yuriy-Pelekh/GitTank) | [![Maintainability](https://api.codeclimate.com/v1/badges/0051cc0a2ffddf2326fd/maintainability)](https://codeclimate.com/github/Yuriy-Pelekh/GitTank/maintainability) | [![Test Coverage](https://api.codeclimate.com/v1/badges/0051cc0a2ffddf2326fd/test_coverage)](https://codeclimate.com/github/Yuriy-Pelekh/GitTank/test_coverage)

## About
This tool helps to manage and keep multiple repositories in sync. Its main goal is the simplification and speeding up of work with git repositories whenever the project requires engineers to work with several repos at once. GitTank supports popular git commands and allows to check out or create a new branch for all configured repositories in matter of seconds. The tool is highly efficient as it works with all repositories in separate threads and also has an emphasis on advanced logging for both the app actions and git commands.

### Benefits it grants
- Support of fetch, pull, push and other commands for simultaneous execution in multiple repos
- Separate threads logic ensures much faster work with several git repositories
- Enhanced logging for the application and for git commands
- Decreased possibility of human error when switching between repos to complete the needed work

![image](https://user-images.githubusercontent.com/4256363/169288310-54338b69-9960-4073-984f-160796ce5ec9.png)

## TODO
- [x] Installer
- [x] Open terminal from the app 
- [ ] Select default terminal to open
- [ ] UI for application configuration and settings
- [x] Application logs
- [x] Git logs
- [x] Multithreading work with repositories
- [ ] New UI
- [ ] Repository statuses
- [ ] Advanced repository dependency configuration
- [ ] Post actions
- [ ] Commit to all repositories at once
- [ ] Create pull request from the application
- [ ] Reminders for uncommitted changes
- [ ] Reminder to pull
- [ ] Automatic fetch in background
- [ ] ...

### Roadmap
![image](https://user-images.githubusercontent.com/4256363/169280350-bee2c76d-5e2c-4e9e-9f7c-aa7d767e3051.png)

### How to contribute
- Step 1: Sign into GitHub
- Step 2: Fork the project repository
- Step 3: Clone your fork
- Step 4: Navigate to your local repository
- Step 5: Check that your fork is the "origin" remote
- Step 6: Add the project repository as the "upstream" remote
- Step 7: Pull the latest changes from upstream into your local repository
- Step 8: Create a new branch
- Step 9: Make changes in your local repository
- Step 10: Commit your changes
- Step 11: Push your changes to your fork
- Step 12: Begin the pull request
- Step 13: Create the pull request
- Step 14: Review the pull request
- Step 15: Add more commits to your pull request
- Step 16: Discuss the pull request
- Step 17: Delete your branch from your fork
- Step 18: Delete your branch from your local repository
- Step 19: Synchronize your fork with the project repository

Congratulations!

