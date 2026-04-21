# Uno Platform Architecture Template (MVVM + Repository + SQLite)

## 1. Layer Responsibilities

- View (XAML + code-behind):
  - Handle visual tree, animations, pointer/gesture events, and control rendering.
  - No business rules, no DB calls.
- ViewModel:
  - Hold state, commands, validation, and business flow.
  - Talk only to interfaces (Repository/Service).
- Repository / Infrastructure:
  - Handle data persistence (SQLite, API, file).
  - No UI references.

## 2. Current NodeLink Template in This Project

- Canvas state + commands:
  - ViewModels/NodeLinkCanvasViewModel.cs
- Page-level workflow (load/save/reset):
  - Presentation/NodeLinkEditorViewModelBase.cs
  - Presentation/NodeLinkDemoViewModel.cs
  - Presentation/NodeLinkTemplateViewModel.cs
- Data contract:
  - Models/NodeGraphDocument.cs
- Data access abstraction:
  - Services/INodeGraphRepository.cs
- Infrastructure implementation:
  - Services/SqliteNodeGraphRepository.cs (Desktop/Mobile)
  - Services/InMemoryNodeGraphRepository.cs (WASM)

## 3. Data Flow

1. Page enters -> initialize command.
2. Page ViewModel calls repository load.
3. Repository returns NodeGraphDocument.
4. NodeLinkCanvasViewModel updates state.
5. NodeLinkCanvas renders from ViewModel state.
6. User interaction updates ViewModel only.
7. Save command serializes ViewModel state and persists.

## 4. How to Add a New Feature (Recommended Template)

1. Create feature models in Models/.
2. Create repository interface in Services/.
3. Create repository implementation (SQLite/API) in Services/.
4. Create page workflow ViewModel in Presentation/.
5. Keep control/page code-behind thin and UI-only.
6. Register interface + implementation in App.xaml.cs.
7. Register view + viewmodel + route in App.xaml.cs.
8. Add entry card in DemoIndexViewModel.

## 5. Practical Rules

- ViewModel should not reference ContentDialog, XamlRoot, Button, TextBox.
- Repository should not reference INavigator or XAML controls.
- Keep command names consistent: Initialize, Save, Reset, Delete.
- Use DTO/document models for persistence boundaries.
- Prefer interface-first design for testability.

## 6. Cross-Platform Note

- WASM does not use SQLite in this template.
- App.xaml.cs registers InMemoryNodeGraphRepository for __WASM__.
- Desktop/Mobile use SqliteNodeGraphRepository.

## 7. Next Refactor Target

- CrudDemoViewModel still contains ContentDialog and XamlRoot UI logic.
- Recommended: move dialog orchestration into an ICrudDialogService to keep ViewModel pure.
