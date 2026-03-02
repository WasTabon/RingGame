# README — Iteration 4: Rhythm Mechanics

## Что нового
- **BeatSequencer** — отсчитывает биты, открывает/закрывает timing window, рассылает события
- **TapInputHandler** — ловит тап в любом месте экрана (блокирует тапы по UI)
- **RhythmPhaseController** — полный цикловый менеджер: 4 цикла × 4 бита, попытки, capture/miss, Wild с 3-го цикла
- **RhythmPhaseUI** — shrinking ring (сужается к кольцу за время window), HIT/MISS feedback текст, flash overlay, анимация конца цикла

## Изменения по сравнению с Iteration 3
- `RhythmPhaseController` полностью переписан — теперь управляет всей ритм-фазой
- `RhythmPhaseUI` расширен — shrinking ring, feedback, flash
- `RingsManager` — добавлен `ResetCapturedRings()`
- После 4 циклов → GameState = ResultScreen (заглушка, будет в Iteration 6)

---

## Инструкция

### Шаг 1 — Импорт файлов
```
Scripts/BeatSequencer.cs               ← НОВЫЙ
Scripts/TapInputHandler.cs             ← НОВЫЙ
Scripts/RhythmPhaseController.cs       ← ОБНОВЛЁН
Scripts/RingsManager.cs                ← ОБНОВЛЁН (ResetCapturedRings)
Scripts/UI/RhythmPhaseUI.cs            ← ОБНОВЛЁН
Editor/Iteration4_Setup.cs             ← НОВЫЙ
```

### Шаг 2 — Setup
**RingGame → Iteration 4 → Setup Game Scene - Rhythm Mechanics**
- Добавляет BeatSequencer и TapInputHandler на сцену
- Добавляет ShrinkingRing и FeedbackText в RhythmPhaseCanvas
- Проверяет что RhythmPhaseController не внутри канваса

### Шаг 3 — Validate
**RingGame → Iteration 4 → Validate & Fix Scene**

---

## Как тестировать
1. MainMenu → Play → PLAY → START GAME
2. Кольца появляются, через ~0.9 сек начинается первый бит
3. Кольцо подсвечивается → появляется золотое shrinking ring вокруг него, сужается
4. **Тапни экран** пока shrinking ring не схлопнулся → HIT, кольцо зеленеет, символ захвачен
5. Не успел → MISS, кольцо краснеет, -1 попытка
6. После 4 битов → кольца сжимаются → цикл 2/4
7. После 4 циклов → экран гаснет (ResultScreen — заглушка)

## Параметры BeatSequencer (Inspector)
- **BPM** — темп (используется для расчёта, по умолчанию 120)
- **Beat Interval** — пауза между битами в секундах (default 1.5s)
- **Cue Lead Time** — время появления shrinking ring до бита
- **Stage Window Ms** — timing window по стейджам: 200 / 170 / 140 / 110 ms

---

## Структура сцены Game (итого)
```
Main Camera
EventSystem
BalanceManager
BetManager
BeatSequencer               ← НОВЫЙ
TapInputHandler             ← НОВЫЙ
RhythmPhaseController       ← активный, на корне сцены (не в канвасе!)

BetScreenCanvas
  BetScreenUI

RhythmPhaseCanvas (inactive)
  RhythmPhaseUI
  RingsManager
  ShrinkingRing             ← НОВЫЙ
  FeedbackText              ← НОВЫЙ
  FlashOverlay              ← НОВЫЙ
  TopBar / GridArea / etc.
```
