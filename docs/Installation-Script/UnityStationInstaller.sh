#!/bin/bash

# Fonction de gestion des erreurs
function handle_error {
    echo "Une erreur s'est produite. Sortie du script."
    exit 1
}

# Fonction pour obtenir la dernière version depuis l'API GitHub
function get_latest_version {
    local repo_url="https://api.github.com/repos/unitystation/stationhub/releases/latest"
    local version=$(wget -qO - "$repo_url" | grep -oP '"tag_name": "\K(.*)(?=")')
    echo "$version"
}

latest_version=$(get_latest_version)

while true; do
    echo "Choisissez votre distribution :"
    echo "1 - Ubuntu/Debian"
    echo "2 - Arch-Linux"
    echo "3 - Fedora"
    echo "4 - OpenSUSE"
    echo "5 - CentOS"
    echo "6 - Gentoo"
    read -p "Entrez le numéro de votre choix : " choice

    case $choice in
        1)
            # Commandes pour Ubuntu
            apt update
            apt full-upgrade
            apt install -y wget p7zip || handle_error
            ;;

        2)
            # Commandes pour Arch Linux
            pacman -Syu --noconfirm
            pacman -S --noconfirm wget p7zip || handle_error
            ;;

        3)
            # Commandes pour Fedora
            dnf update -y
            dnf install -y wget p7zip || handle_error
            ;;

        4)
            # Commandes pour openSUSE
            zypper refresh
            zypper install -y wget p7zip || handle_error
            ;;

        5)
            # Commandes pour CentOS
            yum update -y
            yum install -y wget p7zip || handle_error
            ;;

        6)
            # Commandes pour Gentoo
            emerge --sync
            emerge -av wget p7zip || handle_error
            ;;

        *)
            echo "Choix invalide. Réessayez."
            ;;
    esac

    if [ $choice -ge 1 ] && [ $choice -le 6 ]; then
        break  # Sort de la boucle si le choix est valide
    fi
done

# Téléchargement des fichiers avec wget
mkdir -p /usr/share/Unitystation/
cd /usr/share/Unitystation/
latest_url="https://github.com/unitystation/stationhub/releases/download/$latest_version/lin$latest_version.zip"
wget -O "/usr/share/Unitystation/lin-latest.zip" "$latest_url"
wget -O /usr/share/Unitystation/unityico.png https://raw.githubusercontent.com/unitystation/stationhub/develop/UnitystationLauncher/Assets/unityico.png || handle_error
wget -O /usr/share/applications/Stationhub.desktop https://raw.githubusercontent.com/Unitystation-fork/Unitystation-Others/main/Installation-Script/Unitystation.desktop || handle_error

# Extraction du fichier zip
unzip /usr/share/Unitystation/lin-latest.zip || handle_error
rm -rfv /usr/share/Unitystation/lin-latest.zip || handle_error
chmod -R 777 /usr/share/Unitystation/StationHub || handle_error

echo "Installation terminée."
exit 1
