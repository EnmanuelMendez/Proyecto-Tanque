# Instrucciones de Configuración - Proyecto Tanque
## Enmanuel Mendez | 2-19-0262

---

## Resumen de cambios realizados

Se han agregado las siguientes funcionalidades al juego de tanques:

### 1. Nombre y Matrícula (esquina superior derecha)
**Estado:** ✅ Automático - No requiere configuración adicional.

El `GameManager.cs` ahora crea automáticamente un texto en la esquina superior derecha mostrando:
```
Enmanuel Mendez
2-19-0262
```

### 2. Música de Fondo (Marcha Fúnebre)
**Estado:** ⚠️ Requiere asignar el AudioClip en el Inspector.

**Pasos:**
1. Conseguir un archivo de audio de **marcha fúnebre** (formato .wav, .mp3, o .ogg)
2. Arrastrar el archivo a la carpeta `Assets/AudioClips/`
3. En la escena, crear un **GameObject vacío** llamado "GameInitializer"
4. Agregar el componente **GameInitializer** al objeto
5. En el inspector, arrastrar el clip de marcha fúnebre al campo `m_MarchaFunebre`
6. Ajustar el volumen con `m_MusicVolume` (0.3 por defecto)

**Alternativa:** Si prefieres usar el BackgroundMusic.wav que ya existe:
1. Renombrar o reemplazar `Assets/AudioClips/BackgroundMusic.wav` con una marcha fúnebre
2. Asignarlo al campo `m_MarchaFunebre` del GameInitializer

### 3. Sistema de 5 Vidas (el juego termina a los 5 intentos de perder)
**Estado:** ✅ Automático - Ya integrado en el GameManager.

- Cada vez que un jugador pierde una ronda, se le resta una vida
- Cuando un jugador llega a 5 derrotas, el otro jugador gana automáticamente
- Las vidas se muestran en la UI en la esquina superior izquierda

### 4. Pantalla de Ganador (con puntaje y tiempo)
**Estado:** ✅ Automático - Ya integrado en el GameManager.

Al ganar, se muestra:
- Nombre del jugador ganador (coloreado)
- Puntaje (rondas ganadas)
- Tiempo que le tomó ganar la partida (formato MM:SS)
- Resultados finales de ambos jugadores
- La escena se reinicia automáticamente después de 5 segundos

### 5. Temporizador de 4 Minutos
**Estado:** ✅ Automático - Ya integrado en el GameManager.

- Se muestra en la esquina superior izquierda en formato "TIEMPO: 04:00"
- Se pone en rojo cuando quedan menos de 30 segundos
- Si el tiempo llega a 0 y nadie ha ganado, **ambos jugadores pierden**
- Se muestra un mensaje de "TIEMPO AGOTADO" y la escena se reinicia

### 6. Enemigos Obstáculo
**Estado:** ⚠️ Requiere configuración en el Inspector.

**Pasos con GameInitializer (método recomendado):**
1. Seleccionar el GameObject "GameInitializer" en la escena
2. Marcar `m_SpawnEnemies` como true
3. Ajustar `m_NumberOfEnemies` (3 por defecto)
4. **IMPORTANTE:** En `m_TankLayerMask`, seleccionar la layer "Players"
5. Ajustar `m_EnemySpawnRadius` si es necesario (20 por defecto)

**Comportamiento de los enemigos:**
- Patrullan aleatoriamente por el mapa
- Cuando detectan un tanque cercano (radio de 12-17 unidades), lo persiguen
- Al colisionar con un tanque, lo empujan y causan 5 puntos de daño
- Cada enemigo tiene velocidad y comportamiento ligeramente diferente
- Son visualmente distintos: cubos púrpuras con indicador rojo frontal

---

## Archivos Nuevos Creados

| Archivo | Descripción |
|---------|-------------|
| `Scripts/Managers/BackgroundMusicManager.cs` | Gestiona la música de fondo con patrón Singleton |
| `Scripts/Managers/GameInitializer.cs` | Inicializa música y enemigos al comenzar |
| `Scripts/UI/PlayerInfoUI.cs` | Muestra nombre/matrícula (alternativa standalone) |
| `Scripts/Enemy/EnemyObstacle.cs` | IA de los enemigos obstáculo |
| `Scripts/Enemy/EnemySpawner.cs` | Generador de enemigos con prefab |
| `Scripts/Enemy/EnemySetup.cs` | Helper para crear enemigo por código |

## Archivo Modificado

| Archivo | Cambios |
|---------|---------|
| `Scripts/Managers/GameManager.cs` | Temporizador 4min, sistema de 5 vidas, pantalla de ganador con tiempo/puntaje, UI de nombre/matrícula |

---

## Configuración Rápida (Mínima)

Para que todo funcione con el mínimo esfuerzo:

1. **Abrir la escena principal** (escena.unity o Principal.unity)
2. **Crear un GameObject vacío** → Renombrar a "GameInitializer"
3. **Agregar componente** → GameInitializer
4. **Asignar un AudioClip de marcha fúnebre** al campo `m_MarchaFunebre`
5. **Configurar Layer Mask**: En `m_TankLayerMask` seleccionar "Players"
6. **Play** → Todo debería funcionar

---

## Publicación

### GitHub
El código ya está en el repositorio. Hacer commit y push:
```bash
git add .
git commit -m "Agregar funcionalidades: timer, vidas, enemigos, música, UI nombre"
git push origin main
```

### GitHub Pages (WebGL Build)
1. En Unity: File → Build Settings → WebGL
2. Build → Guardar en una carpeta "docs" o "Build"
3. Subir a GitHub y activar GitHub Pages en Settings → Pages

### Itch.io
1. Build WebGL del juego
2. Crear cuenta en itch.io
3. Crear nuevo proyecto → Subir el ZIP del build WebGL
4. Configurar como "HTML" y marcar "This file will be played in the browser"
