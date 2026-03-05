# Pacman RL (Unity ML-Agents)

Projet Pacman développé dans Unity avec **ML-Agents**.
L’agent apprend à se déplacer dans la carte, récupérer les pellets, éviter les ghosts (ou les chasser en mode power-up), et maximiser son score.

---

## 🧱 Structure principale

- `Scripts/PacmanAgent.cs` : logique agent ML (actions, rewards, reset épisode).
- `Scripts/LevelGenerator.cs` : génération du niveau (murs, pellets, power pellets, porte).
- `Scripts/Ghost*.cs` : comportements des ghosts (scatter, chase, frightened, home).
- `Scripts/LevelDataLight.cs` : map et paramètres version facile.
- `Scripts/LevelDataMedium.cs` : map et paramètres version intermédiaire.
- `Scripts/LevelDataHard.cs` : map et paramètres version difficile.
- `Scripts/LevelData.cs` : **sélecteur actuel de difficulté par héritage**.
- `Pacman.yaml` : configuration d’entraînement ML-Agents (PPO).

---

## ▶️ Lancer le projet

1. Ouvrir la scène principale : `Scenes/MainScene.unity`.
2. Lancer la scène dans Unity (`Play`).

---

## 🧠 Entraîner l’agent

Le fichier `Pacman.yaml` contient les hyperparamètres PPO du behavior `Pacman`.

Exemple de commande (à lancer depuis le projet Unity contenant ML-Agents) :

```bash
mlagents-learn Assets/ML-Agents/Examples/Pacman/Pacman.yaml --run-id=Pacman_Run_01
```

Puis lancer la scène Unity pour démarrer l’entraînement.

---

## 👨‍🏫 Changer le niveau

Le choix du niveau se fait actuellement dans :

- `Scripts/LevelData.cs`

### Étapes

1. Ouvrir `Scripts/LevelData.cs`.
2. Modifier l’héritage de la classe `LevelData`.
3. Sauvegarder, puis relancer la scène.

### Exemple actuel (Medium)

```csharp
public class LevelData : LevelDataMedium {}
```

### Mettre en facile

```csharp
public class LevelData : LevelDataLight {}
```

### Mettre en difficile

```csharp
public class LevelData : LevelDataHard {}
```

---
