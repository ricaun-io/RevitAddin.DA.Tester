# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.5.0] / 2025-01-22
### Features
- Add `ricaun.Revit.DA` to fix issues. (Fix: #7) (Fix: #9)
### Updated
- Delete `Revit.DesignApplication` project.

## [1.4.2] / 2025-01-16
### Features
- Add `ApplicationExtensions` to check `InAddInContext` and `InEventContext`.
- Add `DocumentCreated` event to confirm `ActiveAddInId` is null inside event.
### Updates
- Update `Build` copy.

## [1.4.1] / 2024-09-18
### Features
- Create `Revit.DesignApplication` with `DesignApplication` to load correct assembly version in the bundle. (#7)
### Updates
- Use `DesignApplication` to load correct assembly version in the bundle.
- Add `ApplicationInitialized` event to test.

## [1.4.0] / 2024-08-27
### Features
- Support Bundle multiple versions of Revit using `DesignAutomationLoadVersion`. (#7)
### Updates
- Add `DesignAutomation` and `IDesignAutomation` interface.
- Add `DesignAutomationLoadVersion` to load the right version of the addin.
- Add `AssemblyResolve` in the `DesignAutomationLoadVersion` to load dependencies in the right version.
- Use `where T : IDesignAutomation` in `DesignAutomation<T>` and `DesignAutomationLoadVersion<T>`.
- Fix `DesignAutomation` method selection by finding first method `Execute` with 3 parameters.

## [1.3.1] / 2024-06-15 - 2024-08-27
- Add `AddInId` in the output. #9
- Update `AddInId` to `AddInName` in model class.

## [1.3.0] / 2024-04-05
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
[1.5.0]: ../../compare/1.4.2...1.5.0
[1.4.2]: ../../compare/1.4.1...1.4.2
[1.4.1]: ../../compare/1.4.0...1.4.1
[1.4.0]: ../../compare/1.3.1...1.4.0
[1.3.1]: ../../compare/1.3.0...1.3.1
[1.3.0]: ../../compare/1.2.0...1.3.0
[1.2.0]: ../../compare/1.1.0...1.2.0
[1.1.0]: ../../compare/1.0.0...1.1.0
[1.0.1]: ../../compare/1.0.0...1.0.1
[1.0.0]: ../../compare/1.0.0