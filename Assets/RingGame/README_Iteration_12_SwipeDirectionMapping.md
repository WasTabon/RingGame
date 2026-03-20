# README — Iteration 12: Swipe Direction Mapping + Nerf Payouts

## Что изменилось

### Новые скрипты
- **SwipeDirectionMap.cs** — синглтон, генерирует рандомный маппинг масть→направление (Up/Down/Left/Right) перед каждой игрой. 4 масти = 4 уникальных направления.
- **SwipeMapUI.cs** — панель-оверлей "SWIPE DIRECTIONS" с иконками мастей и стрелками направлений + кнопка GO. Появляется после BetScreen перед началом ритм-фазы.

### Изменённые скрипты
- **TapInputHandler.cs** — `OnTap` заменён на `OnSwipe(SwipeDirection)`. Теперь определяет направление свайпа (вверх/вниз/влево/вправо) на основе вектора движения пальца.
- **RhythmPhaseController.cs** — новое поле `swipeMapUI`. Перед началом ритм-фазы показывает SwipeMapUI. При свайпе проверяет направление: если масть — только правильное направление по маппингу, если Wild — любое. Неправильное направление просто игнорируется.
- **PayoutCalculator.cs** — все множители уменьшены в 3 раза. Было: x0.5/x1.5/x3.0, стало: x0.167/x0.5/x1.0. Пороги лейблов тоже скорректированы.
- **RhythmPhaseUI.cs** — `GetPreviewMultiplier` обновлён чтобы совпадать с новыми множителями.

- **RuntimeDiagnostics.cs** — `OnTap` заменён на `OnSwipe(SwipeDirection)` в подписках и логах для совместимости с новым TapInputHandler.

### Editor скрипт
- **Iteration12_Setup.cs** — EditorWindow: `RingGame → Iteration 12 → Setup Swipe Direction System`. Поле для перетаскивания Canvas-родителя (RhythmPhaseCanvas). Создаёт SwipeDirectionMap и SwipeMapUI панель.

## Как настроить

### Порядок действий

1. **Закинь скрипты:**
   - `SwipeDirectionMap.cs`, `SwipeMapUI.cs` → `Assets/RingGame/Scripts/`
   - Замени `TapInputHandler.cs`, `RhythmPhaseController.cs`, `PayoutCalculator.cs`, `RhythmPhaseUI.cs`, `RuntimeDiagnostics.cs` в `Assets/RingGame/Scripts/`
   - `Iteration12_Setup.cs` → `Assets/RingGame/Editor/`

2. **Дождись компиляции** (Unity может показать ошибки если скрипты ещё не все заменены — это нормально, после замены всех файлов ошибки уйдут).

3. **Открой Game сцену** (`Assets/RingGame/Scenes/Game.unity`).

4. **Запусти Editor скрипт:**
   - Меню: `RingGame → Iteration 12 → Setup Swipe Direction System`
   - Перетащи **RhythmPhaseCanvas** из Hierarchy в поле "Canvas Parent"
   - Нажми **"Setup Swipe Direction System"**

5. **Сохрани сцену** (Ctrl+S).

6. **Валидация:** `RingGame → Iteration 12 → Validate` — проверит что всё на месте.

## Как тестировать

1. Запусти игру из MainMenu сцены.
2. Нажми PLAY, выбери ставку, нажми START GAME.
3. **Должна появиться панель "SWIPE DIRECTIONS"** с 4 мастями и стрелками направлений.
4. Запомни маппинг, нажми GO.
5. Когда появляется жёлтый shrinking ring на кольце:
   - Свайпни **в правильном направлении** для масти этого кольца → HIT
   - Свайпни **в неправильном направлении** → ничего не происходит (не тратится попытка)
   - Не свайпни вовремя → MISS (попытка теряется)
6. Wild символы (появляются в циклах 3-4) принимают свайп в **любом** направлении.
7. Проверь что выигрыши стали примерно в 3 раза меньше (например, 2 совпадения раньше давали ~x1.0, теперь ~x0.33).

## Ожидаемый результат

- Перед каждой игрой появляется панель с рандомным маппингом масть→направление.
- Игрок должен запомнить 4 направления и свайпать правильно.
- Неправильный свайп не наказывает — просто игнорируется.
- Выигрыши уменьшены в 3 раза.
- Маппинг генерируется заново каждую игру.
