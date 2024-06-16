# RevitAddin.DA.Tester

[![Revit +2019](https://img.shields.io/badge/Revit-2019+-blue.svg)](../..)
[![Visual Studio 2022](https://img.shields.io/badge/Visual%20Studio-2022-blue)](../..)
[![Nuke](https://img.shields.io/badge/Nuke-Build-blue)](https://nuke.build/)
[![License MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build](../../actions/workflows/Build.yml/badge.svg)](../../actions)

Revit Addin project to run in [Design Automation](https://aps.autodesk.com/design-automation-cover-page) with some simple file `input.json` and `output.json`.

### Input / Output
```
├── ...
├── input.json
├── output.json
└── ...
```

### [InputModel.cs](RevitAddin.DA.Tester/Models/InputModel.cs)
```C#
public class InputModel
{
    public string Text { get; set; }
    public int Sleep { get; set; }
}
```

### [OutputModel.cs](RevitAddin.DA.Tester/Models/OutputModel.cs)
```C#
public class OutputModel
{
    public string AddInName { get; set; }
    public string VersionName { get; set; }
    public string VersionBuild { get; set; }
    public DateTime TimeStart { get; set; } = DateTime.UtcNow;
    public string Text { get; set; }
    public string Reference { get; set; }
    public string FrameworkName { get; set; }
}
```

## Installation

* Download bundle version [RevitAddin.DA.Tester.bundle.zip](../../releases/latest/download/RevitAddin.DA.Tester.bundle.zip)
* Download and install [RevitAddin.DA.Tester.exe](../../releases/latest/download/RevitAddin.DA.Tester.zip)

## License

This project is [licensed](LICENSE) under the [MIT License](https://en.wikipedia.org/wiki/MIT_License).

---

Do you like this project? Please [star this project on GitHub](../../stargazers)!