# Iteration 6 — Stage Progression

## Что добавлено

### Новые скрипты
- `StageManager.cs` — синглтон управления стейджами, сохранение в PlayerPrefs

### Изменённые скрипты
- `ResultScreenController.cs` — при выигрыше вызывает `StageManager.AdvanceStage()`
- `RhythmPhaseController.cs` — берёт текущий Stage из StageManager, содержит все ручные фиксы
- `RingController.cs` — содержит все ручные фиксы (ResetForNewCycle, BeatPulse)
- `BetScreenUI.cs` — отображает текущий Stage и количество колец

---

## Установка

1. Скопируй файлы:
   - `StageManager.cs` → `Assets/RingGame/Scripts/`
   - `ResultScreenController.cs` → заменить старый
   - `RhythmPhaseController.cs` → заменить старый
   - `RingController.cs` → заменить старый
   - `BetScreenUI.cs` → заменить старый
   - `Iteration6_Setup.cs` → `Assets/RingGame/Editor/`

2. Дождись компиляции

3. Запусти: **RingGame → Iteration 6 → Setup Stage Progression**
   - Открывает MainMenu.unity, создаёт StageManager
   - Открывает Game.unity, добавляет Stage Display на BetScreen

4. Запусти: **RingGame → Iteration 6 → Validate & Fix**

---

## Логика прогрессии

| Результат | Что происходит |
|-----------|----------------|
| WIN       | Stage +1, следующая игра сложнее |
| LOSS      | Stage не меняется |
| Stage 4 WIN | Stage сбрасывается на 1 (цикл) |

### Стейджи

| Stage | Колец | Timing Window |
|-------|-------|---------------|
| 1     | 2     | 800ms         |
| 2     | 3     | 600ms         |
| 3     | 4     | 400ms         |
| 4     | 5     | 250ms         |

---

## Debug

**RingGame → Iteration 6 → Reset Stage (Debug)** — сбрасывает Stage на 1 через PlayerPrefs.

---

## Тест чеклист

- [ ] MainMenu → StageManager существует
- [ ] BetScreen показывает "STAGE 1 / 2 RINGS" при первом запуске
- [ ] Выиграл → Result Screen → Play Again → BetScreen показывает Stage 2 / 3 RINGS
- [ ] Проиграл → Stage не изменился
- [ ] Stage 4 → Win → Stage сбрасывается на 1
- [ ] Stage сохраняется между сессиями (выход и вход в игру)
