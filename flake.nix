{
  description = "TabletBot flake";

  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixpkgs-unstable";
  };

  outputs = { self, nixpkgs, ... }: let
    
    system = "x86_64-linux";

    pkgs = import nixpkgs {
      inherit system;
      config.allowUnfree = true;
    };

    tabletbot = pkgs.callPackage ./. {};

  in rec {
    
    packages.x86_64-linux = {
      inherit tabletbot;
      default = tabletbot;
    };

    devShell.${system} = import ./shell.nix { inherit pkgs; };

  };
}
