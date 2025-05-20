# yaah
## Yet another AUR helper
yaah is an AUR helper written in c# capable of installing/updating AUR packages and their AUR dependencies
## Program flow
Add UML program flow diagram here
## Installation/Cloning
### Arch Linux based systems
```shell
sudo pacman -S --needed git base-devel dotnet-runtime-8.0; git clone https://github.com/inquaterni/yaah.git; cd ./yaah
```
### On Windows through WSL 2
Check if `archlinux` is available
```shell
wsl -l -o
```
If `archlinux` is in list, install it with
```shell
wsl --install archlinux
```
Proceed to [Arch Linux installation](#arch-linux-based-systems)
## Usage
Print help
```shell
dotnet run --project ./Yaah.CLI/Yaah.CLI.csproj --help
```

Install/Update packages
```shell
dotnet run --project ./Yaah.CLI/Yaah.CLI.csproj -S <aur-package-name> <another-aur-package-name>
```

Enable debug logging level
```shell
dotnet run --project ./Yaah.CLI/Yaah.CLI.csproj -D <other options>
```

Serialize graph for given package (does not install package)
```shell
dotnet run --project ./Yaah.CLI/Yaah.CLI.csproj -Dd <aur-package-name> <output-path>
```
