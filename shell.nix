{ pkgs ? import <nixpkgs> { } }:

let
  tabletbot = pkgs.callPackage ./default.nix {};
in pkgs.mkShell {
  buildInputs = with pkgs; [
    tabletbot
  ];
  hardeningDisable = [ "all" ];
}
