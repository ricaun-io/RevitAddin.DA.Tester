# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.3.1] / 2025-06-15
- Add `AddInId` in the output. #9

## [1.3.0] / 2025-04-05
- Update to Revit 2025
- Build `DesignAutomationConsole`.
- Add `FrameworkName` and `Reference` in the output.
- Issue with Design Automation for Revit not selecting the right version of the addin in the bundle. #7
- Set bundle with version 2019, 2021, and 2025.

## [1.2.0] / 2024-03-02
- Add `Sleep` in milliseconds input.

## [1.1.0] / 2024-02-15
- Add `UI.Valid` class to check if Revit UI is available.
- Unsubscribe to `DesignAutomationReadyEvent` to execute only once.

## [1.0.1] / 2023-01-09
### Fixed
- Fix `DBApplication` misspell

## [1.0.0] / 2023-01-04
- First Release

[vNext]: ../../compare/1.0.0...HEAD
[1.3.1]: ../../compare/1.3.0...1.3.1
[1.3.0]: ../../compare/1.2.0...1.3.0
[1.2.0]: ../../compare/1.1.0...1.2.0
[1.1.0]: ../../compare/1.0.0...1.1.0
[1.0.1]: ../../compare/1.0.0...1.0.1
[1.0.0]: ../../compare/1.0.0