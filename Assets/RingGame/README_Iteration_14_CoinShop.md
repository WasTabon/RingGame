# README — Iteration 14: Coin Shop + IAP

## Что изменилось с момента Iteration 13

### Новые скрипты
- **CoinShopUI.cs** — панель магазина по образцу IAPManager. Содержит: dim overlay (tap to close), карточка с балансом, пак "1000 COINS", кнопка покупки с ценой, loading button (перекрывает кнопку покупки пока грузится), статус текст, кнопка X для закрытия. Публичные методы для IAP Button: `OnBuyClicked()`, `OnPurchaseComplete(Product)`, `OnPurchaseFailed(Product, PurchaseFailureDescription)`, `OnProductFetched(Product)`.

### Изменённые скрипты
- **MainMenuUI.cs** — новые поля `shopButton` и `coinShopUI`. Кнопка SHOP рядом с PLAY, по нажатию открывает CoinShopUI.

### Editor скрипт
- **Iteration14_Setup.cs** — EditorWindow: `RingGame → Iteration 14 → Setup Coin Shop`. Два поля: MainMenuCanvas и ButtonsContainer. Создаёт кнопку SHOP и панель CoinShopPanel со всеми элементами.

## Как настроить

### Порядок действий

1. **Закинь скрипты:**
   - `CoinShopUI.cs` → `Assets/RingGame/Scripts/`
   - Замени `MainMenuUI.cs` в `Assets/RingGame/Scripts/`
   - `Iteration14_Setup.cs` → `Assets/RingGame/Editor/`

2. **Дождись компиляции.**

3. **Открой MainMenu сцену** (`Assets/RingGame/Scenes/MainMenu.unity`).

4. **Запусти Editor скрипт:**
   - Меню: `RingGame → Iteration 14 → Setup Coin Shop`
   - Перетащи **MainMenuCanvas** в поле "Canvas Parent"
   - Перетащи **ButtonsContainer** (child MainMenuCanvas) в поле "Buttons Parent"
   - Нажми **"Setup Coin Shop"**

5. **Сохрани сцену** (Ctrl+S).

6. **Настрой IAP вручную:**
   - Найди **BuyButton** внутри CoinShopPanel → Card
   - Добавь на него компонент **IAP Button** (из Unity IAP)
   - Установи Product ID (по умолчанию `com.ringgame.coins1000`, можно изменить в CoinShopUI Inspector)
   - В настройках IAP Button прокинь события:
     - **On Purchase Complete** → CoinShopUI.OnPurchaseComplete
     - **On Purchase Failed** → CoinShopUI.OnPurchaseFailed
     - **On Product Fetched** → CoinShopUI.OnProductFetched
   - В onClick кнопки BuyButton → CoinShopUI.OnBuyClicked

7. **Валидация:** `RingGame → Iteration 14 → Validate`

## Структура CoinShopPanel

```
CoinShopPanel (CoinShopUI)
├── DimOverlay (Image + CanvasGroup + Button для tap-to-close)
└── Card (Image + CanvasGroup)
    ├── Title — "COIN SHOP"
    ├── BalanceHeader — "YOUR BALANCE"
    ├── BalanceText — "$1,000"
    ├── PackBg
    │   ├── PackTitle — "1000 COINS"
    │   └── PackDesc — "Add 1000 coins to your balance"
    ├── BuyButton (← сюда IAP Button) — "$0.99"
    ├── LoadingButton (неактивен, перекрывает BuyButton пока грузится)
    ├── StatusText — "" (показывает "+1000 COINS!" или "Purchase failed")
    └── CloseButton — "X"
```

## Как работает LoadingButton

- При нажатии BuyButton вызывается `OnBuyClicked()` → LoadingButton становится active и перекрывает BuyButton (те же anchors/позиция)
- Пока LoadingButton active — нажать BuyButton невозможно
- При `OnPurchaseComplete` или `OnPurchaseFailed` — LoadingButton деактивируется

## Как тестировать

1. Запусти из MainMenu.
2. Нажми **SHOP** — должна появиться панель с dim overlay.
3. Проверь что баланс отображается правильно.
4. Нажми X или dim overlay — панель закрывается.
5. IAP покупка работает только с настроенным Unity IAP и реальным/sandbox store.

## Ожидаемый результат

- Кнопка SHOP на главном меню под кнопкой PLAY.
- Панель магазина с анимацией open/close.
- Один пак: 1000 монет.
- Loading button блокирует повторное нажатие во время покупки.
- При успешной покупке: +1000 к балансу через BalanceManager, статус "+1000 COINS!".
- При неудачной: статус "Purchase failed".
- Закрытие по X или по тапу на dim overlay.
