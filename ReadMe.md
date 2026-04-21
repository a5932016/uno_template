# Uno Platform 專案模板

這是一個完整的 **Uno Platform** 專案模板，包含各種常用功能和最佳實踐範例。

## 📋 目錄

- [專案結構](#專案結構)
- [功能特色](#功能特色)
- [快速開始](#快速開始)
- [模板詳細文件](#模板詳細文件)
- [頁面目錄](#頁面目錄)
- [控件說明](#控件說明)
- [服務說明](#服務說明)
- [開發指南](#開發指南)

---

## 🏗️ 專案結構

```
unoTest/
├── App.xaml / App.xaml.cs        # 應用程式入口
├── GlobalUsings.cs               # 全域 using 引用
│
├── Assets/                       # 資源檔案
│   ├── Icons/                    # 應用程式圖標
│   └── Splash/                   # 啟動畫面
│
├── Controls/                     # 自訂控件 (UserControl)
│   ├── CustomTitleBar.xaml       # 客製化標題列
│   ├── CardControl.xaml          # 卡片控件
│   ├── DataGridControl.xaml      # 資料表格控件
│   └── NodeLinkCanvas.xaml       # 節點連線畫布
│
├── Converters/                   # 值轉換器
│   └── CommonConverters.cs       # 通用轉換器集合
│
├── Models/                       # 資料模型
│   ├── AppConfig.cs              # 應用程式設定
│   ├── Entity.cs                 # 基礎實體
│   └── Product.cs                # 產品模型
│
├── Presentation/                 # 頁面 (View)
│   ├── Shell.xaml                # 主殼層
│   ├── MainPage.xaml             # 主頁面
│   ├── LoginPage.xaml            # 登入頁面
│   ├── LayoutDemoPage.xaml       # 布局教學
│   ├── CrudDemoPage.xaml         # CRUD 示範
│   ├── NodeLinkDemoPage.xaml     # 節點連線示範
│   └── SecondPage.xaml           # 次要頁面
│
├── ViewModels/                   # ViewModel
│   ├── MainViewModel.cs          # 主頁面 ViewModel
│   ├── ShellViewModel.cs         # Shell ViewModel
│   ├── LoginViewModel.cs         # 登入 ViewModel
│   ├── TitleBarViewModel.cs      # 標題列 ViewModel
│   └── CrudDemoViewModel.cs      # CRUD ViewModel
│
├── Services/                     # 服務層
│   ├── AuthService.cs            # 認證服務
│   ├── ProductService.cs         # 產品服務
│   └── Endpoints/                # API 端點
│       └── DebugHandler.cs       # 除錯處理器
│
├── Styles/                       # 樣式定義
│   ├── ColorPaletteOverride.xaml # 顏色覆寫
│   └── TitleBarStyles.xaml       # 標題列樣式
│
├── Strings/                      # 多語系資源
│   ├── en/Resources.resw         # 英文
│   ├── zh-TW/Resources.resw      # 繁體中文
│   ├── es/Resources.resw         # 西班牙文
│   ├── fr/Resources.resw         # 法文
│   └── pt-BR/Resources.resw      # 葡萄牙文
│
└── Platforms/                    # 平台特定程式碼
    ├── Android/                  # Android
    ├── iOS/                      # iOS
    ├── Desktop/                  # Windows/macOS/Linux
    └── WebAssembly/              # WebAssembly
```

---

## ✨ 功能特色

### 🎨 UI/UX
- ✅ **客製化 TitleBar** - 支援返回、首頁、搜尋、主題切換、使用者資訊
- ✅ **頁面布局教學** - Grid、StackPanel、響應式設計範例
- ✅ **Material Design** - 使用 Uno Material Toolkit
- ✅ **深色/淺色主題** - 動態切換支援

### 📱 頁面模板
- ✅ **登入頁面** - 帳密登入、第三方登入、表單驗證
- ✅ **CRUD 頁面** - 增刪改查、搜尋篩選、分頁功能
- ✅ **節點連線** - Button 之間用線連接的互動畫布

### 🔧 架構與服務
- ✅ **MVVM 架構** - CommunityToolkit.Mvvm
- ✅ **依賴注入** - Microsoft.Extensions.DependencyInjection
- ✅ **導航服務** - Uno.Extensions.Navigation
- ✅ **多語系支援** - Microsoft.Extensions.Localization
- ✅ **設定管理** - appsettings.json 配置
- ✅ **HTTP 客戶端** - Kiota HTTP 支援

### 🧩 自訂控件
- ✅ **CardControl** - 可重複使用的卡片元件
- ✅ **DataGridControl** - 資料表格（含分頁、搜尋）
- ✅ **NodeLinkCanvas** - 節點連線畫布

---

## 🚀 快速開始

### 前置需求
- .NET 9 SDK
- Visual Studio 2022 或 VS Code
- Uno Platform Extension

### 建置與執行

```bash
# 建置 WebAssembly 版本
dotnet build -f net9.0-browserwasm

# 建置 Desktop 版本
dotnet build -f net9.0-desktop

# 執行 Desktop
dotnet run -f net9.0-desktop

# 發布 WebAssembly
dotnet publish -f net9.0-browserwasm -c Release
```

### VS Code Tasks
專案已配置好以下 Tasks（可在終端機直接使用）：
- `build-wasm` - 建置 WebAssembly
- `publish-wasm` - 發布 WebAssembly
- `build-desktop` - 建置 Desktop
- `publish-desktop` - 發布 Desktop

---

## 📚 模板詳細文件

每一個模板（Page / Control）都已提供獨立 README，請從以下索引開始：

- `unoTest/Docs/Templates/README.md`

---

## 📄 頁面目錄

| 頁面 | 說明 | ViewModel |
|------|------|-----------|
| `MainPage` | 主頁面，導航入口 | `MainViewModel` |
| `LoginPage` | 登入頁面，支援帳密與第三方登入 | `LoginViewModel` |
| `LayoutDemoPage` | 布局教學，展示各種 Layout 技巧 | `LayoutDemoViewModel` |
| `CrudDemoPage` | CRUD 示範，產品管理增刪改查 | `CrudDemoViewModel` |
| `NodeLinkDemoPage` | 節點連線示範，互動式流程圖 | - |
| `SecondPage` | 次要頁面範例 | `SecondViewModel` |

---

## 🧩 控件說明

### CustomTitleBar
客製化標題列，支援：
- 應用程式圖標
- 導航按鈕（返回、首頁、刷新）
- 標題文字
- 搜尋、通知、主題切換
- 使用者資訊與選單
- 視窗控制按鈕（Desktop）

```xml
<controls:CustomTitleBar 
    Title="我的應用程式"
    ShowBackButton="True"
    ShowWindowControls="True"/>
```

### CardControl
可重複使用的卡片控件：
- 標題、副標題、圖標
- 內容區域
- 頁腳與動作按鈕
- 展開/收合功能

```xml
<controls:CardControl 
    Title="卡片標題"
    Subtitle="副標題"
    PrimaryActionText="確定"
    SecondaryActionText="取消">
    <controls:CardControl.CardContent>
        <TextBlock Text="卡片內容"/>
    </controls:CardControl.CardContent>
</controls:CardControl>
```

### NodeLinkCanvas
節點連線畫布：
- 新增/刪除節點
- 拖曳移動節點
- 節點之間連線
- 自動排列
- 格線顯示

```xml
<controls:NodeLinkCanvas 
    x:Name="Canvas"
    ShowGrid="True"
    NodeMinWidth="120"/>
```

```csharp
// 新增節點
var node1 = Canvas.AddNode("開始", 100, 100);
var node2 = Canvas.AddNode("結束", 300, 100);

// 建立連線
Canvas.AddLink(node1, node2);
```

---

## 🔌 服務說明

### IAuthService
認證服務介面：
```csharp
public interface IAuthService
{
    Task<AuthResult> LoginAsync(string username, string password, bool rememberMe = false);
    Task<AuthResult> LoginWithGoogleAsync();
    Task<AuthResult> LoginWithMicrosoftAsync();
    Task<AuthResult> LoginWithAppleAsync();
    Task LogoutAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    Task<bool> IsAuthenticatedAsync();
}
```

### IProductService
產品 CRUD 服務：
```csharp
public interface IProductService
{
    Task<IEnumerable<Product>> GetAllAsync();
    Task<Product?> GetByIdAsync(int id);
    Task<PagedResult<Product>> GetPagedAsync(int page, int pageSize, string? keyword = null);
    Task<Product> CreateAsync(Product product);
    Task<Product> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
}
```

---

## 📖 開發指南

### 新增頁面
1. 在 `Presentation/` 建立 `.xaml` 和 `.xaml.cs`
2. 在 `ViewModels/` 建立對應的 ViewModel
3. 在 `App.xaml.cs` 的 `RegisterRoutes` 註冊路由

```csharp
views.Register(
    new ViewMap<MyPage, MyViewModel>()
);

routes.Register(
    new RouteMap("MyPage", View: views.FindByViewModel<MyViewModel>())
);
```

### 新增服務
1. 在 `Services/` 建立介面和實作
2. 在 `App.xaml.cs` 的 `ConfigureServices` 註冊

```csharp
services.AddSingleton<IMyService, MyService>();
```

### 多語系支援
在 `Strings/{語言代碼}/Resources.resw` 新增翻譯：

```xml
<data name="MyString" xml:space="preserve">
    <value>我的字串</value>
</data>
```

使用方式：
```csharp
var text = _localizer["MyString"];
```

---

## 📚 參考資源

- [Uno Platform 官方文件](https://platform.uno/docs/)
- [Uno Toolkit](https://github.com/nicholastng/uno.toolkit.ui)
- [CommunityToolkit.Mvvm](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/)
- [Uno Extensions](https://github.com/nicholastng/uno.extensions)

---

## 📝 License

MIT License

---

**Built with ❤️ using Uno Platform**