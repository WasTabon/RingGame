# README — Iteration 3: Rings Visual System

## Что нового в этой итерации
- **RingController** — одно кольцо с полной системой анимаций:
  - Постоянное вращение с настраиваемой скоростью и направлением
  - Символ-маркер на кольце, counter-rotate (всегда читается, не вращается со стрелкой)
  - Live-реакция на музыку: кольцо слегка пульсирует с басами
  - Состояния: Spinning / Highlighted / Captured / Stopped
  - Каждый переход — своя DOTween анимация (shake, punch, glow, color)
- **RingsManager** — создаёт кольца под stage, управляет ими
  - Stage 1: 2 кольца, Stage 2: 3 кольца и т.д.
  - Каждое кольцо — своя скорость, цвет, символ
  - `ShrinkAllRings()` — анимация сжатия между циклами + ускорение 15%
  - `PlayEntrance()` — плавное появление
- **RhythmPhaseController** — активирует фазу при GameState = RhythmPhase
- **RhythmPhaseUI** — полный UI экрана ритма:
  - Верхняя панель: цикл (1/4), ставка, попытки (●●●●)
  - Область колец по центру
  - Заглушка грида снизу (будет заполняться в Iteration 5)
  - Dev-кнопка ← BACK для тестирования

## Что изменилось по сравнению с Iteration 2
- После нажатия START в BetScreen → появляется RhythmPhaseCanvas
- Кольца автоматически создаются и запускаются
- BetScreen скрывается, RhythmPhase показывается

---

## Инструкция по настройке

### Шаг 1 — Импорт файлов
```
Assets/RingGame/Scripts/RingController.cs          ← НОВЫЙ
Assets/RingGame/Scripts/RingsManager.cs            ← НОВЫЙ
Assets/RingGame/Scripts/RhythmPhaseController.cs   ← НОВЫЙ
Assets/RingGame/Scripts/UI/RhythmPhaseUI.cs        ← НОВЫЙ
Assets/RingGame/Editor/Iteration3_Setup.cs         ← НОВЫЙ
```

### Шаг 2 — Запуск editor скрипта
**RingGame → Iteration 3 → Setup Game Scene - Rings**
- Откроет Game.unity
- Добавит RhythmPhaseCanvas (скрытый, активируется по GameState)
- Создаст RingsManager с нужными спрайтами и SymbolConfig
- Сохранит сцену

### Шаг 3 — Создать SymbolConfig (для своих иконок)
**RingGame → Iteration 3 → Create Symbol Config Asset**
- Создаёт `Assets/RingGame/Data/SymbolConfig.asset`
- Открывает его в Inspector
- Назначь спрайты для ♠♥♦♣ и Wild (пока можно оставить пустыми)
- Если спрайты не назначены — символы отображаются белым кружком с цветной заливкой

---

## Как тестировать

1. Открой MainMenu → Play
2. PLAY → Bet Screen → START GAME
3. Ожидаемый результат:
   - Bet Screen плавно скрывается
   - Появляется RhythmPhase экран
   - Кольца появляются с анимацией spawn (scale из 0 с OutBack ease)
   - Кольца вращаются: внутреннее медленнее, внешнее быстрее, разные направления
   - Символы на кольцах не вращаются (counter-rotate), всегда читаются
   - Если назначена музыка — кольца слегка пульсируют с басами
   - Кнопка ← BACK возвращает на Bet Screen

## Тестирование анимаций состояний в Play mode
В Inspector найди RingsManager и вызови через контекстное меню (или напрямую из другого скрипта):
- `RingsManager.Instance.HighlightRing(0, true)` — подсветит первое кольцо (жёлтое свечение)
- `RingsManager.Instance.CaptureRing(0)` — захватит первое кольцо (зелёная вспышка + shake)
- `RingsManager.Instance.MissOnRing(1)` — промах на втором (красный flash + shake)
- `RingsManager.Instance.ShrinkAllRings()` — анимация конца цикла

---

## Структура RhythmPhase Canvas
```
RhythmPhaseCanvas (sortOrder 15, изначально скрыт)
  Background
  TopBar
    CycleText               ← "CYCLE 1/4"
    BetText                 ← "$10" (золотой)
    AttemptsRow
      Dot_0/1/2/3           ← попытки (жёлтые кружки)
  RingsArea
    RingsContainer          ← сюда RingsManager добавляет кольца
  GridArea                  ← заглушка грида
    GridLabel
    GridCells (2x4 ячейки)
  RingsManager              ← создаёт кольца при SetupForStage()
  RhythmPhaseController     ← слушает GameManager.OnStateChanged
  BackButton                ← только для тестирования
```

## Структура одного кольца (создаётся RingsManager динамически)
```
Ring_N
  Glow          ← Image, прозрачный, светится при highlight/beat
  Body          ← Image, ring sprite, основной визуал
  MarkerRoot    ← RectTransform на верхушке кольца, вращается с ним
    SymbolBg    ← круглая подложка с цветом масти
      Icon      ← Image для спрайта иконки масти
```

---

## Заметки
- Спрайты колец и символов генерируются процедурально при первом запуске editor скрипта
- SymbolConfig — ScriptableObject, назначь свои иконки когда будут готовы
- RhythmPhaseCanvas по умолчанию `SetActive(false)`, активируется через RhythmPhaseController
- Скорости колец в Stage 1 = базовые × 1.0, Stage 2 = × 1.15, и т.д.
