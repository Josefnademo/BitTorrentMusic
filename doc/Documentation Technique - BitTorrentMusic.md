# Documentation Technique - BitTorrentMusic

Ce document détaille l'architecture technique de l'application **BitTorrentMusic**, en se concentrant sur les modèles de données, les services utilitaires et le protocole réseau basé sur MQTT.

## 1. Models (`BitTorrentMusic.Models`)

Cet espace de noms contient les structures de données utilisées pour manipuler les informations musicales et les messages réseau.

### 1.1. `ISong` & `Song`

L'interface `ISong` et son implémentation `Song` définissent la structure d'un fichier audio partagé sur le réseau.

- **Rôle** : Standardiser les métadonnées des fichiers audio.
- **Propriétés principales** :
  - `Title`, `Artist`, `Year`, `Featuring` : Métadonnées ID3.
  - `Duration` : Durée de la piste (`TimeSpan`).
  - `Size` : Taille du fichier en octets (utilisée pour le transfert).
  - `Hash` : Empreinte unique (SHA256) permettant d'identifier le fichier de manière unique sur le réseau.

### 1.2. `Message`

La classe `Message` représente l'enveloppe de données échangée entre les pairs via le broker MQTT.

- **Rôle** : Transport des requêtes et des réponses (JSON).
- **Champs clés** :
  - `Action` : Le type de commande (ex: `"requestCatalog"`, `"sendChunk"`, `"online"`).
  - `Sender` / `Recipient` : Identifiants des pairs (ou `*` pour le broadcast).
  - `SongList` : Liste de chansons (utilisé lors de l'échange de catalogues).
  - `SongData` : Données binaires du fichier audio encodées en Base64 (pour le transfert de morceaux).
  - `StartByte` / `EndByte` : Définissent la plage d'octets pour le transfert par morceaux (chunking).

---

## 2. Services (`BitTorrentMusic.Services`)

Ce module gère la logique métier complexe, notamment la cryptographie et la manipulation de fichiers binaires.

### 2.1. `Helper`

Classe utilitaire statique dédiée à la sécurité et à l'intégrité des données.

- **Fonctionnalité principale** : Calcul de hash SHA256.
- **Méthodes** :
  - `HashFile(string path)` : Calcule le hash d'un fichier existant sur le disque.
  - `HashFileBytes(byte[] data)` : Calcule le hash d'un tableau d'octets (utilisé lors de la vérification après téléchargement).

### 2.2. `FileTransferService`

Service central responsable de la fragmentation et de l'assemblage des fichiers pour le transfert P2P.

- **Fonctionnement** :
  1.  **Découpage (Split)** : Lit un fichier local et le divise en blocs de 4096 octets (4 KB) via `SplitFile`.
  2.  **Réception** : Stocke temporairement les morceaux reçus en mémoire dans un dictionnaire (`receivingFiles`).
  3.  **Assemblage** : Une fois tous les morceaux reçus, la méthode `TryAssembleFile` :
      - Reconstitue le fichier complet.
      - Calcule le hash du résultat.
      - Compare ce hash avec le hash attendu (`expectedHash`).
      - Sauvegarde le fichier sur le disque uniquement si l'intégrité est validée.

---

## 3. Protocol (`BitTorrentMusic.Protocol`)

C'est le cœur de la communication réseau, implémentant l'interface `IProtocol`. Il utilise la bibliothèque **MQTTnet**.

### 3.1. `IProtocol`

Interface définissant le contrat de communication pour l'application. Elle abstrait la couche réseau du reste de l'interface utilisateur.

- **Méthodes requises** : `SayOnline`, `AskCatalog`, `SendCatalog`, `AskMedia`.

### 3.2. `NetworkProtocol`

Implémentation concrète du protocole utilisant MQTT.

- **Connexion** : Se connecte au broker `mqtt.blue.section-inf.ch` sur le port `1883` avec un ClientID unique.
- **Gestion des messages (`OnMessage`)** : Écoute le topic `BitRuisseau` et traite les actions suivantes :
  - `online` : Enregistre un nouveau pair dans la liste `onlinePeers`.
  - `requestcatalog` : Déclenche l'envoi de la liste des fichiers locaux via le délégué `LocalCatalogProvider`.
  - `sendcatalog` : Réceptionne et stocke les catalogues des autres pairs.
  - `requestchunk` : Lit une partie du fichier local et l'envoie au demandeur.
  - `sendchunk` : Réceptionne un morceau de fichier et le passe au `FileTransferService`.
- **Délégués** : Utilise `Func<List<Song>>` et `Func<string, string>` pour interagir avec les données de l'interface utilisateur (UI) sans couplage fort.
- **JSON Converter** : Intègre `SongJsonConverter` pour gérer la désérialisation polymorphique de l'interface `ISong`.

---

## Flux de Téléchargement (Workflow)

1.  **Recherche** : L'utilisateur double-clique sur une chanson distante.
2.  **Demande** : `NetworkProtocol` envoie un message `requestChunk` pour l'ensemble du fichier (byte 0 à la fin).
3.  **Envoi** : Le pair distant reçoit la demande, découpe le fichier via `FileTransferService`, et envoie une série de messages `sendChunk`.
4.  **Réception** : Votre client reçoit les morceaux, les stocke, et une fois le dernier morceau reçu, `FileTransferService` assemble le MP3 et déclenche l'événement `FileReceived`.
