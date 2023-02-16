{
  inputs.nixpkgs.url = "nixpkgs/nixos-22.11";

  description = "Ryujinx-LdnServer flake env";


  outputs = { self, nixpkgs }:
    let 
    # TODO: Cross arch and platform
    in

    {
      overlay = final: prev: {};

      packages.x86_64-linux.ryujinx-ldn-server = with import nixpkgs { system = "x86_64-linux"; }; pkgs.callPackage ./default.nix {};

      packages.x86_64-linux.default = self.packages.x86_64-linux.ryujinx-ldn-server;
    };
}

