{ pkgs ? import <nixpkgs> {}, stdenv ? pkgs.stdenv }:

let
in

pkgs.buildDotnetModule rec {
  pname = "ryujinx-ldn-server";
  version = "0.1";

  src = ./.;

  projectFile = "LanPlayServer.sln";
  nugetDeps = ./deps.nix;

  dotnet-sdk = pkgs.dotnetCorePackages.sdk_7_0;
  dotnet-runtime = pkgs.dotnetCorePackages.runtime_7_0;
  selfContainedBuild = false;

  dotnetFlags = [ "-p:PublishAOT=false" ];

  executables = [ "LanPlayServer" ];
}