{ lib, stdenv, fetchNuGet, buildDotnetModule, dotnetCorePackages }:

let
in

buildDotnetModule rec {
  pname = "ryujinx-ldn-server";
  version = "0.1";

  src = ./.;

  projectFile = "LanPlayServer.sln";
  nugetDeps = ./deps.nix;

  dotnet-sdk = dotnetCorePackages.sdk_6_0;
  dotnet-runtime = dotnetCorePackages.runtime_6_0;
  selfContainedBuild = false;

  dotnetFlags = [
    "-p:ExtraDefineConstants=DISABLE_CLI"
  ];

  executables = [ "LanPlayServer" ];
}