#!/bin/bash

# Fonction pour afficher un message de confirmation
function confirm_delete {
    echo "Voulez-vous vraiment supprimer le jeu Unitystation ?"
    read -p "Tapez 'y' pour confirmer ou 'n' pour annuler : " response
    if [[ $response == "y" ]]; then
        return 0
    else
        return 1
    fi
}

# Fonction pour supprimer uniquement le dossier contenant le Launcher et le raccourci du bureau
function delete_launcher_and_shortcut {
    rm -rfv /usr/share/Unitystation
    rm -fv /usr/share/applications/Stationhub.desktop
}

# Fonction pour supprimer TOUS (jeux + raccourcis + données du jeu)
function delete_all {
    rm -rfv ~/local/share/StationHub
    rm -rfv /usr/share/Unitystation
    rm -fv /usr/share/applications/Stationhub.desktop
}

# Afficher un message de confirmation
confirm_delete
if [ $? -eq 0 ]; then
    echo "Choisissez une option :"
    echo "1 - Supprimer uniquement le dossier contenant le Launcher et le raccourci du bureau"
    echo "2 - Supprimer TOUS (jeux + raccourcis + données du jeu)"
    read -p "Entrez le numéro de votre choix : " choice

    case $choice in
        1)
            delete_launcher_and_shortcut
            echo "Suppression du Launcher et du raccourci terminée."
            ;;
        2)
            delete_all
            echo "Suppression complète terminée."
            ;;
        *)
            echo "Choix invalide. Aucune action effectuée."
            ;;
    esac
fi
exit 1
