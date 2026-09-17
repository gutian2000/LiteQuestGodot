# LiteQuestGodot 项目手册

> 创建日期：2026-09-17 | 当前版本：v1.0.0 | 作者：gutian2000

---

## 一、项目概览

LiteQuest 是一个轻量级、完全解耦的 **Godot 4.7+ .NET/C# 任务系统框架**。

### 核心功能
- 5 种目标类型：Collect / Kill / Damage / ReachLocation / Custom
- 任务链（前置条件 + 自动启动下游任务）
- 事件驱动运行时（4 个 C# event）
- 类型安全的上报 API（按 ObjectiveType 过滤，防止 Kill/Damage 串线）
- JSON 存档（向前兼容 + 坏档自动回退）
- 编辑器 Inspector 插件（UndoRedo + 实时校验）
- 任务追踪 HUD + 模态 NPC 对话
- 完整可玩的 2D 演示场景

### 技术栈
| 项 | 值 |
|----|-----|
| 引擎 | Godot 4.7.2 stable (.NET / C# / mono) |
| 框架 | Godot.NET.Sdk 4.7.x |
| 语言 | C# |
| 许可证 | MIT |
| 程序集 | LiteQuestGodot |
| 命名空间 | LiteQuest.QuestSystem / LiteQuest.QuestSystem.Editor |

### 本地路径
```
D:\Godot项目学习文件\LiteQuestGodot\
├── Scripts/              ← 可复用框架
├── Demo/                 ← 演示场景 + DemoController
├── Assets/               ← QuestDefinition .tres 示例
├── addons/quest_inspector/  ← 编辑器插件
├── Screenshots/          ← 1920×1080 商店截图
├── LICENSE               ← MIT
├── README.md             ← 完整 API 参考
├── VERSION               ← 1.0.0
└── .gitignore
```

---

## 二、发布平台 & 访问地址

| 平台 | 状态 | 地址 | 说明 |
|------|------|------|------|
| **itch.io** | ✅ 已发布（Public） | https://gutian2000.itch.io/litequest-system | 主发布渠道，$0 + 可选捐赠 |
| **Godot Asset Store** | ⏳ 审核中（Pending） | https://store.godotengine.org/asset/gutian2000/litequest-godot/ | 新版 Godot 4.7+ 官方商店 |
| **GitHub** | ✅ 已推送 | https://github.com/gutian2000/LiteQuestGodot | 源码仓库，master 分支 |

### 各平台后台入口

| 平台 | 管理后台 |
|------|---------|
| itch.io | https://itch.io/game/edit/... （从 Dashboard 进） |
| Godot Asset Store | https://store.godotengine.org/asset/gutian2000/litequest-godot/manage/ |
| GitHub | https://github.com/gutian2000/LiteQuestGodot |

---

## 三、账号 & 收款

### GitHub
- **用户名**：gutian2000
- **CLI 登录**：`gh auth status`（已登录，keyring 存储 token）

### itch.io
- **用户名**：gutian2000
- **收款模式**：Collected by itch.io, paid later（代收）
- **收款渠道**：Payoneer（已连接并通过审核）
- **定价**：$0 + 可选捐赠

### Godot Asset Store
- **登录方式**：Godot SSO → GitHub OAuth
- **Publisher**：gutian2000

### Payoneer
- 用于接收 itch.io 代收的款项
- 注册时绑定国内储蓄卡，美元账户接收后可结汇提现

---

## 四、开发工具 & 常用命令

### Godot 可执行文件
```
D:\Godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe
```

### 自动截图命令行
```powershell
# 在 PowerShell 里跑，LITEQUEST_SHOT 触发截图导演
$env:LITEQUEST_SHOT="overview"; & "D:\Godot\Godot_v4.7.2-stable_mono_win64\Godot_v4.7.2-stable_mono_win64_console.exe" --path "D:\Godot项目学习文件\LiteQuestGodot"

# 其他 shot 值：combat / dialog / completion / gate
```

### dotnet build
```powershell
cd "D:\Godot项目学习文件\LiteQuestGodot"
dotnet build
```

### GitHub CLI
```powershell
gh repo view gutian2000/LiteQuestGodot
gh api repos/gutian2000/LiteQuestGodot/contents/ --jq '.[].name'
```

### itch.io 上架 zip 生成
```powershell
# 已生成在 D:\ai编程\ai编程测试\LiteQuestGodot_v1.0.0.zip
# 如需重新生成：
Compress-Archive -Path "D:\Godot项目学习文件\LiteQuestGodot\*" -DestinationPath "D:\ai编程\ai编程测试\LiteQuestGodot_v1.0.0.zip" -Force
```

---

## 五、关键技术坑（已踩过，别再踩）

| 坑 | 现象 | 解法 |
|----|------|------|
| **编辑器进程 CLR 类型退化** | `obj is QuestDefinition` 在编辑器进程恒为 false | 编辑器代码用 `obj.GetScript()` 比 res:// 路径 + Variant/Get/Set；游戏运行时用强类型 |
| **Git 初始化时 .godot/ 和 .import/ 未排除** | Godot 缓存被提交，仓库膨胀 | .gitignore 必须包含 `.godot/` `bin/` `obj/` `*.import` |
| **Ctrl+A 在浏览器自动化里不生效** | 清空 textbox 失败 | 用 JS evaluate 直接设 value + 触发 input/change 事件 |
| **Asset Store 提交 Asset Name 被拼接脏数据** | Tags 的 "Editor Tool" 被追加到 Asset Name | 不要在 Asset Name textbox 旁边操作，或用 JS 修正 |
| **Godot Asset Store 必须有 Store Thumbnail 才能 Publish** | Publish 按钮灰掉 | Media 标签 → Store Thumbnail → 上传封面图 |
| **itch.io 中国开发者收款** | Stripe 不支持中国大陆主体 | 用 Payoneer + itch.io 代收模式 |
| **Godot 4.7 旧 Asset Library 已废弃** | godotengine.org/asset-library 全部 404 | 必须走 store.godotengine.org 新版 |

---

## 六、v1.1 规划（差异化收费版）

### 当前市场判断
| 维度 | 事实 |
|------|------|
| Godot 开发者 | GDScript 70% / C# 10-15% |
| C# Quest 框架竞品 | 空白（所有竞品都是 GDScript） |
| C# + 可视化图编辑器 | 真空白，但 Godot 4.7 C# 编辑器工具链坑多 |
| itch.io 冷启动下载量 | 5~20 份（无营销） |
| 竞品定价 | Nexus Quest Weaver MIT 免费，Quest System Lite PRO $5.99 |

### v1.1 功能清单

#### 🔥 P0：运行时调试面板（3~5 天）
- F8 快捷键弹出（游戏内 CanvasLayer, ProcessMode=Always）
- 实时显示：所有任务状态/进度/前置 + 最近 20 条事件日志
- 手动触发按钮：ReportCollect/ReportKill/CompleteQuest/ResetAll
- **竞品对标**：Nexus 有但只在编辑器里；游戏运行时的 C# 面板 = 市场空白

#### 🔥 P0：自定义 ObjectiveType（1~2 天）
- QuestManager 新增 `RegisterObjectiveHandler(type, handler)` + `Report(type, targetId, amount)`
- 让用户扩展 "对话 X 次"、"升级到 X 级" 等非内置目标
- **竞品对标**：没有框架提供 C# 级别的类型安全扩展点

#### ⭐ P1：前置条件门控扩展（2~3 天）
- QuestDefinition.StartCondition：AllPrerequisites / AnyPrerequisite / Custom(Func<bool>)
- 让 CanStart 支持条件分支

#### 💡 P2：Godot 4.8 稳定后再做
- 可视化任务链编辑器（GraphEdit + GraphNode）
- Godot 4.8 dev 1 已改善 C# 编辑器工具链，届时重踩坑

### 定价策略
| 版本 | 时间 | 定价 | 渠道 |
|------|------|------|------|
| v1.0.0 | 2026-09-17 | 免费 + 捐赠 | itch.io / Godot Asset Store |
| **v1.1.0** | 审核通过后 1~3 周 | **$2~$3** | itch.io 付费版；Godot Asset Store 保持 v1.0 免费版引流 |
| v1.2.0 | Godot 4.8 stable 后 | $4~$5 | 加可视化图编辑器 |

### 商业化原则
1. **先看数据再改价**：免费版跑 3 个月，Godot Asset Store 下载量 >200 再改价
2. **免费版持续引流**：Godot Asset Store 保持 v1.0 免费，itch.io 卖 v1.1 付费
3. **免费版限制策略**：抄 Quest System Lite 的做法——免费版保留 v1.0 全部功能但**最多注册 5 个 QuestDefinition**，付费版解锁无限 + 自定义 ObjectiveType + 调试面板

---

## 七、下一步行动清单

### 立即可做（审核期间）
- [ ] 等 Godot Asset Store 审核通过（Pending → Approved）
- [ ] 在 Reddit r/godot 发帖宣传（"LiteQuest — C# Quest Framework for Godot 4.7"）
- [ ] 在 Godot Discord #showcase 频道分享
- [ ] 给 GitHub 仓库加 GitHub Pages 做文档站

### 1~3 个月后
- [ ] 检查 Godot Asset Store 下载量
- [ ] 决定是否做 v1.1 付费版
- [ ] 如有 200+ 下载，开始 v1.1 开发

### v1.1 开发时
- [ ] 运行时调试面板（P0）
- [ ] 自定义 ObjectiveType（P0）
- [ ] 条件门控扩展（P1）
- [ ] 免费版限制 5 个 QuestDefinition
- [ ] 改 itch.io 定价为 $2~$3

### Godot 4.8 stable 后
- [ ] 评估可视化图编辑器可行性
- [ ] v1.2.0 规划

---

## 八、数据备份 & 版本管理

| 物品 | 位置 | 频率 |
|------|------|------|
| GitHub 仓库 | gutian2000/LiteQuestGodot | 每次提交自动备份 |
| itch.io 定价/文案 | 已在 ITCH_IO_PAGE.md 留档 | 改价时更新 |
| 截图文件 | Screenshots/ 目录 + GitHub | 自动同步 |
| 上架 zip | D:\ai编程\ai编程测试\LiteQuestGodot_v1.0.0.zip | 手动备份 |
| 项目手册 | 本文件（README 同级） | 每次迭代更新 |

---

## 九、竞品清单（持续关注）

| 产品 | 语言 | 图编辑器 | 运行时调试 | 存档 | 价格 | 链接 |
|------|------|---------|-----------|------|------|------|
| Nexus Quest Weaver 1.5 | GDScript | ✅ | ✅ (仅编辑器) | ✅ | MIT | - |
| Questify | GDScript | ✅ | ❌ | ✅ | MIT | - |
| Game Lattice | C# | ❌ | ❌ | ✅ | MIT | - |
| Quest System (Shomy) | GDScript | ❌ | ❌ | ✅ | MIT | - |
| Quest System Lite PRO | GDScript | ✅ | ❌ | ✅ | $5.99 | itch.io |

---

## 十、快速恢复清单（如果一切搞砸了）

1. **Git reset 一切**：`cd D:\Godot项目学习文件\LiteQuestGodot; git fetch origin; git reset --hard origin/master`
2. **Godot 重新导入**：打开 Godot → Project → Reimport
3. **重新生成上架 zip**：`Compress-Archive -Path "D:\Godot项目学习文件\LiteQuestGodot\*" -DestinationPath "D:\ai编程\ai编程测试\LiteQuestGodot_v1.0.0.zip" -Force`
4. **itch.io 重新上传**：Dashboard → Edit project → Uploads
5. **Godot Asset Store 重新提交**：https://store.godotengine.org/asset/gutian2000/litequest-godot/manage/

---

**文档维护者**：gutian2000 | **最后更新**：2026-09-17
