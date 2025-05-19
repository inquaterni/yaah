# yaah
## Yet another AUR helper
yaah is an AUR helper written in c# capable of installing/updating AUR packages and their AUR dependencies
## Program flow
Add UML program flow diagram here
## Installation
Arch Linux based systems
```shell
sudo pacman -S --needed git base-devel dotnet-runtime-8.0; git clone https://github.com/inquaterni/yaah.git; cd ./yaah
```
Emulating using Docker on Windows<br>
[Install Docker](https://docs.docker.com/desktop/setup/install/windows-install/)
```shell
docker pull archlinux
docker run -u root -a stdin -a stdout -i -t archlinux /bin/bash
```
## Usage
Print help
```shell
dotnet run --project ./Yaah.CLI.csproj --help
```

Install/Update packages
```shell
dotnet run --project ./Yaah.CLI.csproj -S <aur-package-name> <another-aur-package-name>
```

Enable debug logging level
```shell
dotnet run --project ./Yaah.CLI.csproj -D <other options>
```

Serialize graph for given package (does not install package)
```shell
dotnet run --project ./Yaah.CLI.csproj -Dd <aur-package-name> <output-path>
```
