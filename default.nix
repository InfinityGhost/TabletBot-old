{ dotnetCorePackages
, buildDotnetModule
}:

buildDotnetModule rec {
  pname = "tabletbot";
  name = pname;
  version = "1.0.0.0";

  src = ./.;

  dotnet-sdk = dotnetCorePackages.sdk_5_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_5_0;

  dotnetInstallFlags = [ "--framework=net5.0" ];

  nugetDeps = ./deps.nix;

  executables = [ "TabletBot" ];
  projectFile = executables;
}
