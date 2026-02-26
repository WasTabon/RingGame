# README — Iteration 2: Bet Screen

## Что нового в этой итерации
- **BalanceManager** — синглтон, хранит баланс в PlayerPrefs, старт $1000
- **BetManager** — синглтон, управляет текущей ставкой (мин $10, макс $500, шаг $10)
- **BetScreenUI** — полноценный экран ставки с анимациями:
  - Баланс с анимированным счётчиком
  - Крупный дисплей текущей ставки
  - Кнопки +/− с punch-анимацией
  - Слайдер заполнения (визуальный, не интерактивный)
  - Пресеты MIN / HALF / MAX
  - Кнопка START GAME (блокируется если баланс < ставки)
  - Анимация появления всего экрана

## Изменения по сравнению с Iteration 1
- В Game сцену добавлены BalanceManager, BetManager, BetScreenCanvas
- START GAME снимает ставку с баланса и переводит GameManager в RhythmPhase
- (RhythmPhase пока пустой — будет в Iteration 3/4)

---

## Инструкция по настройке

### Шаг 1 — Импорт файлов
Скопируй новые файлы в проект:
```
Assets/RingGame/Scripts/BalanceManager.cs
Assets/RingGame/Scripts/BetManager.cs
Assets/RingGame/Scripts/UI/BetScreenUI.cs
Assets/RingGame/Editor/Iteration2_Setup.cs
```

### Шаг 2 — Запуск editor скрипта
**RingGame → Iteration 2 → Setup Game Scene - Bet Screen**
- Откроет Game.unity
- Добавит BalanceManager, BetManager
- Создаст BetScreenCanvas со всем UI
- Сохранит сцену

### Шаг 3 — Сброс баланса (если нужно)
**RingGame → Iteration 2 → Reset Player Balance**
- Сбрасывает PlayerPrefs баланс до $1000

---

## Как тестировать

1. Открой MainMenu сцену → Play
2. Нажми PLAY → переход в Game сцену
3. Появляется Bet Screen с анимацией появления
4. Тестируй:
   - Кнопки +/− меняют ставку с анимацией
   - MIN / HALF / MAX устанавливают пресеты
   - Слайдер отражает текущую ставку
   - START GAME списывает ставку с баланса
   - Если баланс = 0, START недоступен (затемняется)

## Ожидаемый результат
- Элегантный экран с тёмным фоном
- Баланс сверху, крупная ставка по центру
- Золотая кнопка START с пульсацией
- Все анимации плавные через DOTween
- После START → экран гаснет (RhythmPhase пока пустой)

---

## Структура сцены Game
```
Main Camera (2D orthographic)
EventSystem
BalanceManager               ← DontDestroyOnLoad
BetManager                   ← DontDestroyOnLoad
BetScreenCanvas (sortOrder 20)
  Background
  DecorCircle                ← декоративное кольцо
  Header
    BalanceLabel             ← "BALANCE"
    BalanceValue             ← "$1000"
  BetDisplay
    BetLabel                 ← "YOUR BET"
    BetValue                 ← "$10" (золотой)
  BetControls
    DecreaseBtn              ← "−"
    IncreaseBtn              ← "+"
    SliderTrack
      SliderFill             ← заполнение золотом
  PresetRow
    MinBtn / HalfBtn / MaxBtn
  StartButton                ← "START GAME" (золотой)
```

---

## Заметки
- BalanceManager и BetManager используют DontDestroyOnLoad — добавляй их только в одну сцену (Game)
- В MainMenu сцене этих менеджеров нет — они создаются при переходе в Game
- Баланс сохраняется между сессиями через PlayerPrefs
- Для сброса баланса используй: RingGame → Iteration 2 → Reset Player Balance
