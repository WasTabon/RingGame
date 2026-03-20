# README — Iteration 13: In-Game Tutorial

## Что изменилось с момента Iteration 12

### Новые скрипты
- **GameTutorialUI.cs** — панель-оверлей на ритм-фазе. Показывает текущий маппинг свайпов (4 масти с иконками + направления и названия) и правила игры. Кнопка "GOT IT!" закрывает панель. Блокирует весь gameplay пока открыта (dim overlay).

### Изменённые скрипты
- **RhythmPhaseController.cs** — новое поле `gameTutorialUI`. При первой игре (проверка `SettingsManager.GameTutorialDone`) автоматически показывает туториал после SwipeMapUI и до старта BeatSequencer. Новый метод `PauseForTutorial()` — приостанавливает BeatSequencer, деактивирует ввод, показывает туториал, после закрытия возобновляет всё.
- **RhythmPhaseUI.cs** — новое поле `helpButton`. Кнопка "?" в TopBar, по клику вызывает `RhythmPhaseController.PauseForTutorial()`.
- **BeatSequencer.cs** — новые методы `PauseSequence()` / `ResumeSequence()`. При паузе корутина SequenceLoop ждёт (yield return null) пока `IsPaused == true`, в том числе внутри timing window — таймер окна не тикает пока пауза.
- **SettingsManager.cs** — новый ключ `game_tutorial_done`, свойство `GameTutorialDone`, методы `MarkGameTutorialDone()` и `ResetGameTutorial()`. Отдельно от menu tutorial.

### Editor скрипт
- **Iteration13_Setup.cs** — EditorWindow: `RingGame → Iteration 13 → Setup Game Tutorial`. Два поля: Canvas Parent (RhythmPhaseCanvas) и TopBar Parent. Создаёт GameTutorialUI панель и кнопку "?" в TopBar. Прокидывает ссылки в RhythmPhaseController и RhythmPhaseUI. Есть кнопка Reset Game Tutorial Flag для повторного тестирования.

## Как настроить

### Порядок действий

1. **Закинь скрипты:**
   - `GameTutorialUI.cs` → `Assets/RingGame/Scripts/`
   - Замени `RhythmPhaseController.cs`, `RhythmPhaseUI.cs`, `BeatSequencer.cs`, `SettingsManager.cs` в `Assets/RingGame/Scripts/`
   - `Iteration13_Setup.cs` → `Assets/RingGame/Editor/`

2. **Дождись компиляции.**

3. **Открой Game сцену** (`Assets/RingGame/Scenes/Game.unity`).

4. **Запусти Editor скрипт:**
   - Меню: `RingGame → Iteration 13 → Setup Game Tutorial`
   - Перетащи **RhythmPhaseCanvas** в поле "Canvas Parent"
   - Перетащи **TopBar** (child RhythmPhaseCanvas) в поле "TopBar Parent"
   - Нажми **"Setup Game Tutorial"**

5. **Сохрани сцену** (Ctrl+S).

6. **Валидация:** `RingGame → Iteration 13 → Validate`

### Важно
- SettingsManager должен быть на MainMenu сцене (создан в Iteration 11). Новое свойство `GameTutorialDone` добавлено в тот же скрипт, поэтому замени файл и в MainMenu сцене он подхватит новый функционал автоматически.
- Iteration 12 (SwipeDirectionMap, SwipeMapUI) должна быть уже настроена.

## Как тестировать

### Первый запуск (автоматический туториал)
1. Нажми `RingGame → Iteration 13 → Reset Game Tutorial Flag` чтобы сбросить флаг.
2. Запусти игру из MainMenu.
3. PLAY → выбери ставку → START GAME.
4. Появится панель SwipeMapUI (направления свайпов) → нажми GO.
5. **Автоматически появится GameTutorialUI** с текущим маппингом и правилами.
6. Нажми "GOT IT!" → туториал закрывается → BeatSequencer стартует.
7. При следующей игре туториал НЕ появится автоматически.

### Кнопка "?" (ручной вызов)
1. Во время ритм-фазы (когда кольца крутятся и идёт gameplay) нажми кнопку "?" в TopBar.
2. **Gameplay паузится:** BeatSequencer на паузе, ввод отключён.
3. GameTutorialUI показывает текущий маппинг и правила.
4. Нажми "GOT IT!" → gameplay возобновляется с того же места.
5. Timing window НЕ истекает во время паузы.

### Что проверить
- Туториал показывает правильные направления (те же что были на SwipeMapUI).
- Пауза действительно приостанавливает всё — shrinking ring не уменьшается.
- После закрытия туториала gameplay продолжается нормально.
- Кнопку "?" можно нажимать многократно.

## Ожидаемый результат

- При первой игре — автоматический туториал после маппинга свайпов.
- Кнопка "?" в TopBar для вызова туториала в любое время.
- Туториал показывает: маппинг масть→направление + правила (Wild = любое направление, неправильный свайп = ничего, медленно = потеря попытки).
- Пауза BeatSequencer во время туториала.
