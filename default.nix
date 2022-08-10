{ dotnetCorePackages
, buildDotnetModule
}:

buildDotnetModule rec {
  pname = "tabletbot";
  name = pname;
  version = "1.0.0.0";

  src = ./.;

  dotnet-sdk = dotnetCorePackages.sdk_6_0;
  dotnet-runtime = dotnetCorePackages.aspnetcore_6_0;

  dotnetInstallFlags = [ "--framework=net6.0" ];

  nugetDeps = ./deps.nix;

  executables = [ "TabletBot" ];
  projectFile = executables;
}
