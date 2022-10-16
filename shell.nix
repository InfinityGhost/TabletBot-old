{ pkgs ? import <nixpkgs> { } }:

let
  defaultPackage = pkgs.callPackage ./default.nix {};
in pkgs.mkShell {
  buildInputs = with pkgs; [ defaultPackage.dotnet-sdk ];
  hardeningDisable = [ "all" ];
}
