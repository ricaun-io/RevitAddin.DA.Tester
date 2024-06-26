using Nuke.Common;
using Nuke.Common.Execution;
using ricaun.Nuke;
using ricaun.Nuke.Components;

class Build : NukeBuild, IPublishRevit, IBuildConsole, IPrePack
{
    string IHazRevitPackageBuilder.ApplicationType => "DBApplication";
    string IHazRevitPackageBuilder.Application => "Revit.App";
    public static int Main() => Execute<Build>(x => x.From<IPublishRevit>().Build);
}