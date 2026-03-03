# Iteration 5 — Result Screen & Payout System

## Что добавлено

### Новые скрипты
- `PayoutCalculator.cs` — статический класс подсчёта выигрыша по захваченным символам
- `ResultScreenController.cs` — управляет переходом на результат, добавляет выигрыш на баланс
- `ResultScreenUI.cs` — анимированный экран результатов

### Изменённые скрипты
- `RhythmPhaseController.cs` — `EndPhase()` теперь передаёт символы в ResultScreenController

---

## Установка

1. Скопируй файлы в проект:
   - `PayoutCalculator.cs` → `Assets/RingGame/Scripts/`
   - `ResultScreenController.cs` → `Assets/RingGame/Scripts/`
   - `ResultScreenUI.cs` → `Assets/RingGame/Scripts/UI/`
   - `RhythmPhaseController.cs` → `Assets/RingGame/Scripts/` (заменить старый)
   - `Iteration5_Setup.cs` → `Assets/RingGame/Editor/`

2. Дождись компиляции Unity

3. Запусти: **RingGame → Iteration 5 → Setup Result Screen**

4. Запусти: **RingGame → Iteration 5 → Validate & Fix**

---

## Логика выигрыша

| Совпадений | Множитель (Hearts) | Множитель (Diamonds) |
|------------|-------------------|----------------------|
| 2          | x0.9              | x1.1                 |
| 3          | x2.7              | x3.3                 |
| 4          | x5.4              | x6.6                 |
| Wild x2    | x2.5              | x2.5                 |
| Wild x3    | x7.5              | x7.5                 |

Wild заменяет любой символ и добавляется к лучшей группе.

### Метки результата
- `SMALL WIN` — множитель < 2
- `WIN!` — множитель 2–5
- `BIG WIN!` — множитель 5–10
- `JACKPOT!` — множитель 10+
- `NO MATCH` — символы захвачены но нет пары
- `NO CAPTURE` — ни одного символа не захвачено

---

## Экран результатов — флоу анимации

1. Fade in фона (0.3s)
2. Result Label появляется с punch scale (0.35s)
3. Символы появляются по одному слева направо (0.12s между каждым)
4. Если WIN — незахваченные символы тускнеют, выигрышные пульсируют жёлтым
5. Счётчик выигрыша анимированно считает от 0 до суммы (0.6s)
6. Обновляется баланс
7. Появляются кнопки Play Again / Menu

---

## Структура сцены после Setup

```
ResultScreenCanvas (inactive)       ← sorting order 20
  BgPanel
  FlashOverlay
  ResultLabel
  MultiplierText
  PayoutContainer
    PayoutText
  SymbolsRow
    Slot_0 … Slot_7
      Bg
      Icon
      Label
  Separator
  BalanceText
  BetText
  ButtonsRow
    PlayAgainBtn
    MenuBtn
  ResultScreenUI (component)

ResultScreenController (GameObject, root level)
```

---

## Тест чеклист

- [ ] 4 цикла ритм-фазы завершаются → появляется Result Screen
- [ ] Символы отображаются в правильном порядке
- [ ] WIN: выигрышные символы подсвечиваются, счётчик считает
- [ ] NO CAPTURE: экран показывает результат без анимации выигрыша
- [ ] Баланс обновляется правильно
- [ ] Play Again → возврат на Bet Screen с анимацией
- [ ] Menu → возврат на Main Menu
