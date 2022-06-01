# GitTank

[![.NET Core Desktop](https://github.com/Yuriy-Pelekh/GitTank/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/Yuriy-Pelekh/GitTank/actions/workflows/dotnet-desktop.yml)

Version | Build Status | Tests | Maintainability | Test Coverage
------------ | ------------- | ------------- | ------------- | -------------
Latest build | [![Build status](https://ci.appveyor.com/api/projects/status/a6t2412jpppsrern?svg=true)](https://ci.appveyor.com/project/Yuriy-Pelekh/gittank) | [![codecov](https://codecov.io/gh/Yuriy-Pelekh/GitTank/graph/badge.svg?token=3DFPOFMG80)](https://codecov.io/gh/Yuriy-Pelekh/GitTank) | |
Stable build | [![Build status](https://ci.appveyor.com/api/projects/status/a6t2412jpppsrern/branch/main?svg=true)](https://ci.appveyor.com/project/Yuriy-Pelekh/gittank/branch/main) | [![codecov](https://codecov.io/gh/Yuriy-Pelekh/GitTank/branch/main/graph/badge.svg?token=3DFPOFMG80)](https://codecov.io/gh/Yuriy-Pelekh/GitTank) | [![Maintainability](https://api.codeclimate.com/v1/badges/0051cc0a2ffddf2326fd/maintainability)](https://codeclimate.com/github/Yuriy-Pelekh/GitTank/maintainability) | [![Test Coverage](https://api.codeclimate.com/v1/badges/0051cc0a2ffddf2326fd/test_coverage)](https://codeclimate.com/github/Yuriy-Pelekh/GitTank/test_coverage)

## About
This project is designed to simplify work with multiple repositories simultaneously. The idea behind is that there are multiple dependent repositories that have always be on the same branch. GitTank does that and even more. It allows user to `checkout` or create new branch in all configured repositories in one click. Also, it supports other `git` commands like `fetch`, `pull`, `push`. There are plans to extend list of commands and use cases.

GitTank is highly efficient as it works with all repositories in separate threads. For example, regular user working with, let say, 5 repositories has to navigate to 5 directories and execute `pull` command in each of them and wait for completion before switching to another repository. GitTank does all that in one click and at least user spends 5 times less efforts and time.

This application is dedicated for developers. So, it means there is a big focus on enhanced logging for the application and for git commands.

There are many other useful features and plans. Stay tuned 😊

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

