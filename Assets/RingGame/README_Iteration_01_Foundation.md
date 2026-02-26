# README — Iteration 1: Foundation

## Что нового в этой итерации
- **GameManager** — синглтон, стейт машина (MainMenu → BetScreen → RhythmPhase → ResultScreen)
- **AudioManager** — воспроизведение музыки + автоматическая детекция битов через анализ спектра басов
- **SceneTransitionManager** — плавные fade-переходы между сценами
- **MainMenuUI** — анимированное главное меню: лого, кнопка PLAY, вращающиеся кольца на фоне, пульсация на биты
- **SymbolConfig** — ScriptableObject для иконок мастей (заглушки, заменишь позже)
- **Две сцены:** MainMenu, Game (пустая)
- **Проект 2D** — ортографическая камера

---

## Требования перед запуском
1. Создай проект в Unity 2022.3 как **2D** (Built-in Render Pipeline)
2. Установи **DOTween Free** из Asset Store
3. Установи **TextMeshPro** — Unity предложит при первом использовании (Import TMP Essentials)

---

## Инструкция по настройке

### Шаг 1 — Импорт файлов
Скопируй папку `Assets/RingGame/` в папку `Assets/` своего проекта, сохранив структуру.

### Шаг 2 — Настройка сцены MainMenu
**RingGame → Iteration 1 → Setup Main Menu Scene**
- Создаёт/открывает `Assets/RingGame/Scenes/MainMenu.unity`
- Расставляет камеру (ортографическая 2D), менеджеры, fade overlay, весь UI главного меню
- Добавляет сцену в Build Settings (индекс 0)

### Шаг 3 — Создание пустой Game сцены
**RingGame → Iteration 1 → Create Empty Game Scene**
- Создаёт `Assets/RingGame/Scenes/Game.unity` с 2D камерой
- Добавляет в Build Settings (индекс 1)

### Шаг 4 — Проверь Build Settings
File → Build Settings:
```
0: Assets/RingGame/Scenes/MainMenu.unity  ✓
1: Assets/RingGame/Scenes/Game.unity      ✓
```

### Шаг 5 — Добавь музыку (опционально)
- Выбери **AudioManager** в Hierarchy
- Назначь аудиоклип в поле **Default Music Clip**
- Настрой **Beat Sensitivity** (0.1 = очень чувствительно, 2.0 = только сильные удары)
- Лучше всего работает с EDM, электронной, поп музыкой

---

## Как тестировать
1. Открой `Assets/RingGame/Scenes/MainMenu.unity`
2. Нажми Play
3. Ожидаемый результат:
   - Тёмный фон с тремя медленно вращающимися кольцами
   - Лого "RING GAME" плавно появляется с эффектом scale
   - Подзаголовок "Rhythm · Luck · Skill"
   - Кнопка PLAY появляется с мягкой пульсацией
   - Если назначена музыка — лого пульсирует на каждый бит
   - Нажатие PLAY: анимация кнопки → fade out → fade in → пустая Game сцена

---

## Структура сцены MainMenu
```
Main Camera                  ← ортографическая 2D
EventSystem
GameManager                  ← DontDestroyOnLoad
AudioManager
  MusicSource
  SFXSource
SceneTransitionManager       ← DontDestroyOnLoad
  FadeCanvas (sortOrder 999)
    FadeOverlay              ← чёрный fullscreen, управляется через CanvasGroup
MainMenuCanvas (sortOrder 10)
  Background                 ← тёмно-синий фон
  BgRings                    ← контейнер фоновых колец
    BgRing_0/1/2             ← вращающиеся кольца разного размера
  LogoContainer
    TitleText                ← "RING GAME" (золотой)
    SubtitleText             ← "Rhythm · Luck · Skill"
  ButtonsContainer
    PlayButton               ← золотая кнопка
```

---

## Заметки
- Текстуры колец генерируются процедурально и сохраняются в `Assets/RingGame/Textures/`
- Editor скрипт не пересоздаёт объекты если они уже существуют на сцене
- SymbolConfig спрайты — заглушки, будут использоваться в Iteration 3
