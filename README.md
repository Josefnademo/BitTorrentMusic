# 🎧 BitTorrentMusic

## Description

BitRuisseau est une application de médiathèque audio partagée basée sur un protocole P2P (peer-to-peer),développée en C# (Windows Forms).
Le but est de permettre à plusieurs utilisateurs d’échanger des fichiers multimédias(mp3) sans passer par un serveur central.
Le projet met en œuvre des mécanismes avancés de transfert de données tels que le chunking, la gestion des timeouts pour améliorer la stabilité et la fluidité des échanges.

## ⚙️ Technologies utilisées

- Langage : C#
- Framework : (Windows Forms)
- Gestion de temps : Timer, Task.Delay()
- Smart Delay (sdelay) : algorithme d’adaptation dynamique du délai entre envois de chunks

## Fonctionnalités implémentées

- Chunking : le fichier est découpé en blocs de taille fixe pour faciliter le transfert.
- Transfert P2P basique : un pair peut envoyer ou recevoir des chunks.
- Reconstitution : les chunks sont assemblés dans le bon ordre pour reformer le fichier.
- Contrôle d’intégrité : comparaison du hash de chaque chunk.

⚙️ Installation

1. Cloner le dépôt :
   ```bash
   git clone https://github.com/Josefnademo/BitTorrentMusic
   ```
2. Se placer dans le dossier du projet :

   ```bash
    cd src/BitTorrentMusic
   ```

3. Lancer le programme :
   ```bash
    dotnet run
   ```

## Maquette Figma

La maquette de l’interface utilisateur (UI) a été réalisée sur Figma

👉 Lien vers la [maquette Figma]()

## Planification

La planification du projet a été réalisée en format `.md`

👉 Lien vers la [Planification.md](https://github.com/Josefnademo/BitTorrentMusic/blob/main/doc/Planifiaction.md)

## Webographie:

- [online-tools](https://emn178.github.io/online-tools/sha1_checksum.html)
