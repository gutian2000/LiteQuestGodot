# LiteQuestGodot 项目手册

> 创建日期：2026-09-17 | 当前版本：v1.0.0 | 作者：gutian2000

---

## 一、项目概览

LiteQuest 是一个**跨引擎双版本**的轻量级、完全解耦任务系统框架。

| 版本 | 引擎 | 语言 | 状态 | 定价 |
|------|------|------|------|------|
| **LiteQuestGodot** | Godot 4.7.2+ | C# (.NET/mono) | ✅ v1.0.0 已发布 | 免费 + 捐赠 |
| **LiteQuest Unity** | Unity 2022.3+ | C# | ✅ v1.0.0 开发完成，待上架 | 计划 $10~$15（代理上架） |

### Godot 版核心功能
- 5 种目标类型：Collect / Kill / Damage / ReachLocation / Custom
- 任务链（前置条件 + 自动启动下游任务）
- 事件驱动运行时（4 个 C# event）
- 类型安全的上报 API（按 ObjectiveType 过滤，防止 Kill/Damage 串线）
- JSON 存档（向前兼容 + 坏档自动回退）
- 编辑器 Inspector 插件（UndoRedo + 实时校验）
- 任务追踪 HUD + 模态 NPC 对话
- 完整可玩的 2D 演示场景

### Unity 版核心功能
- 同一套 5 种目标类型 + 任务链 + 事件驱动架构
- ScriptableObject 定义任务
- UnityEvent + C# event 双层事件
- .asset 存档（PlayerPrefs + JSON）
- QuestTrackerUI / QuestPanelUI / QuestGiver 预制体
- 完整 Demo 场景（QuestSystem_Baseline.unity）
- 附带 8 张截图 + README + API Reference + Tutorial + Advanced 文档

### 技术栈（Godot 版）
| 项 | 值 |
|----|-----|
| 引擎 | Godot 4.7.2 stable (.NET / C# / mono) |
| 框架 | Godot.NET.Sdk 4.7.x |
| 语言 | C# |
| 许可证 | MIT |
| 程序集 | LiteQuestGodot |
| 命名空间 | LiteQuest.QuestSystem / LiteQuest.QuestSystem.Editor |

### Unity 版 package.json
| 项 | 值 |
|----|-----|
| 包名 | com.litequest.questsystem |
| displayName | LiteQuest System |
| Unity 版本 | 2022.3 |
| 依赖 | com.unity.textmeshpro@3.0.9, com.unity.inputsystem@1.14.2 |

### 本地路径
```
D:\Godot项目学习文件\LiteQuestGodot\          ← Godot 版（主项目）
├── Scripts/              ← 可复用框架
├── Demo/                 ← 演示场景 + DemoController
├── Assets/               ← QuestDefinition .tres 示例
├── addons/quest_inspector/  ← 编辑器插件
├── Screenshots/          ← 1920×1080 商店截图
├── LICENSE               ← MIT
├── README.md             ← 完整 API 参考
├── VERSION               ← 1.0.0
└── .gitignore

C:\Users\Administrator\OneDrive\Desktop\com.litequest.questsystem.tmp\  ← Unity 版（临时目录）
├── Runtime/              ← 运行时代码 + 预制体 + 资源
├── Samples/              ← 演示场景
└── Documentation/        ← README + API Reference + Tutorial + Advanced + Screenshots
```

---

## 二、发布平台 & 访问地址

| 平台 | 资产 | 状态 | 地址 | 说明 |
|------|------|------|------|------|
| **itch.io** | LiteQuestGodot | ✅ 已发布（Public） | https://gutian2000.itch.io/litequest-system | 主发布渠道，$0 + 可选捐赠 |
| **Godot Asset Store** | LiteQuestGodot | ⏳ 审核中（Pending） | https://store.godotengine.org/asset/gutian2000/litequest-godot/ | 新版 Godot 4.7+ 官方商店 |
| **GitHub** | LiteQuestGodot | ✅ 已推送 | https://github.com/gutian2000/LiteQuestGodot | 源码仓库，master 分支 |
| **Unity Asset Store（代理）** | LiteQuest Unity | ⏳ 协议待签署 | 待 Unity 中国注册账号后分配 | **主力收入渠道**，由 Unity 中国代理上架全球市场 |

### 各平台后台入口

| 平台 | 管理后台 |
|------|---------|
| itch.io | https://itch.io/game/edit/... （从 Dashboard 进） |
| Godot Asset Store | https://store.godotengine.org/asset/gutian2000/litequest-godot/manage/ |
| GitHub | https://github.com/gutian2000/LiteQuestGodot |
| Unity Asset Store | 待 Unity 中国注册后通知 |

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

### Unity Asset Store（代理）
- **代理方**：You San Di Technology HK Limited（Unity 中国）
- **代理模式**：框架协议 + 附件一（每产品登记，不用重签）
- **分佣**：Unity 30% 平台费后 → **乙方 70%**（代理零抽成）
- **免费代理期**：至 2028-12-31，甲方零手续费
- **结算周期**：季度结 + 季末后 90 日
- **独家范围**：仅附件一里列的资源，未列入的资产可自己上架
- **Unity 账号**：待 Unity 中国注册后分配

### Payoneer
- 用于接收 itch.io 代收的款项
- 注册时绑定国内储蓄卡，美元账户接收后可结汇提现

---

## 三-A、Unity 中国代理协议

### 协议文件
- **本地路径**：`C:\Users\Administrator\OneDrive\Desktop\Unity全球资源商店代理分发协议.docx`
- **签署状态**：⏳ 待回家打印签字
- **乙方信息**：谷田（个人开发者，填身份证号）
- **甲方**：You San Di Technology HK Limited（Unity 中国香港实体）

### 协议关键条款

| 条款 | 内容 | 意义 |
|------|------|------|
| 分佣 | 扣 Unity 30% 后，乙方拿 70% | 和自己上架同比例，代理零抽成 |
| 免费代理期 | 至 2028-12-31 | 白嫖 2 年多代理服务 |
| 协议性质 | 框架协议，附件一可不时更新 | 以后上新产品只需更新附件一，不用重签 |
| 知识产权 | 归乙方所有 | 只是授权代理，不是卖版权 |
| 独家代理 | 仅限附件一里列的资源 | 灵活选择哪些资产代理、哪些自己上 |
| 售后义务 | 乙方负责技术问答 + 客诉 | 不是只管上架，要当客服 |
| 怠于支持惩罚 | 长期不回问题 → 暂停结算 / 下架 / 终止 | 每月至少看一眼邮件 |
| 结算周期 | 季度结 + 季末后 90 日 | Q1 的钱 6 月底到账 |
| 适用法律 | 中国法律 | 对你有利 |
| 仲裁地 | 上海仲裁委员会 | 中国开发者友好 |

### 回家后操作清单

1. **填 Word 空白**（先不打印）
   - 乙方：谷田
   - 身份证号：填 18 位
   - 地址：身份证地址
   - 日期：签字当天
2. **打印一式两份**，A4 纸
3. **两份都签中文真名**（谷田）
4. **快递寄出**：
   ```
   上海市虹口区东大名路 501 号
   白玉兰大厦 39 楼
   Unity 中国资源商店
   联系电话：021-61486489
   ```
5. **回邮件**给 AssetStore@unitycn.zohodesk.com.cn：
   ```
   Unity 中国团队您好：
   附件协议已签署完毕，今天寄出。
   快递单号：【XXX】
   收件人：谷田
   收件地址：【XXX】
   手机号：【XXX】
   ```

### Unity 中国邮件往来
| 时间 | 方向 | 摘要 |
|------|------|------|
| 2026-09-15 | 我方发出 | 全球 Asset Store 代理上架申请 |
| 2026-09-16 | Unity 回复 | 代理分发协议附件 + 签署流程说明 |

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

## 六、商业化策略（双引擎）

### 收入管道对比

| 维度 | Godot Asset Store + itch.io | Unity Asset Store（代理） |
|------|---------------------------|--------------------------|
| 定价 | 免费 + 捐赠 | **$10~$15** |
| 分佣 | itch.io 代收后 Payoneer 提现 | Unity 30% → 你拿 70% |
| 结算周期 | itch.io 自定 | 季度 + 季末后 90 日 |
| 开发者基数 | Godot C# 10-15% | Unity 50%+ 全球市场 |
| 竞品数量 | 少（C# 框架空白） | 中（有 Quest System Lite PRO $5.99） |
| 预期销量 | 5~20 份（冷启动） | **100~500 份**（代理 + 全球市场） |
| 变现定位 | **引流 + 口碑** | **主力收入** |

### 路线图

| 阶段 | 时间 | 动作 |
|------|------|------|
| 现在 | 2026-09-17 | Godot 版免费发布；Unity 协议待签署 |
| 近期 | Unity 签回后 1~2 周 | Unity 版代理上架全球 Asset Store |
| 中期 | 1~3 个月 | Godot 版看下载量决定是否做 v1.1 付费版 |
| 长期 | Godot 4.8 stable | Godot v1.2 可视化图编辑器 |

### 定价策略
| 资产 | 版本 | 定价 | 渠道 |
|------|------|------|------|
| LiteQuestGodot | v1.0.0 | 免费 + 捐赠 | itch.io / Godot Asset Store |
| LiteQuestGodot | v1.1.0 | **$2~$3** | itch.io 付费版；Godot Asset Store 保持 v1.0 免费版引流 |
| LiteQuest Unity | v1.0.0 | **$12.99**（建议） | Unity Asset Store（代理上架） |
| LiteQuestGodot | v1.2.0 | $4~$5 | 加可视化图编辑器 |

### 商业化原则
1. **Unity 版 = 主力收入**：Godot 市场小，Unity 全球市场 50%+，代理上架零门槛
2. **Godot 版 = 引流口碑**：免费跑 3 个月，Godot Asset Store 下载量 >200 再改价
3. **免费版持续引流**：Godot Asset Store 保持 v1.0 免费，itch.io 卖 v1.1 付费
4. **免费版限制策略**：抄 Quest System Lite 的做法——免费版保留 v1.0 全部功能但**最多注册 5 个 QuestDefinition**，付费版解锁无限 + 自定义 ObjectiveType + 调试面板

---

## 七、下一步行动清单

### 🔴 最高优先级：Unity 协议签署
- [ ] 回家填协议 Word 空白（乙方信息 + 日期）
- [ ] 打印一式两份
- [ ] 两份都签字
- [ ] 快递寄到 Unity 中国（保留快递单号）
- [ ] 回邮件给 Unity 提供快递单号 + 收件信息

### 立即可做（审核期间）
- [ ] 等 Godot Asset Store 审核通过（Pending → Approved）
- [ ] 在 Reddit r/godot 发帖宣传（"LiteQuest — C# Quest Framework for Godot 4.7"）
- [ ] 在 Godot Discord #showcase 频道分享
- [ ] 给 GitHub 仓库加 GitHub Pages 做文档站

### Unity 协议签回后
- [ ] Unity 中国注册 Asset Store 账号（待通知）
- [ ] Unity 版资产打包为 .unitypackage
- [ ] Unity Asset Store 上架资料准备（视频 + 1280×720 截图 + API 文档）
- [ ] 定价建议 $12.99（同类 Quest System Lite PRO $5.99，我们功能更全）

### 1~3 个月后
- [ ] 检查 Godot Asset Store 下载量
- [ ] 检查 Unity Asset Store 销量（第一个季度结算）
- [ ] 决定是否做 Godot v1.1 付费版

### Godot v1.1 开发时
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

**文档维护者**：gutian2000 | **最后更新**：2026-09-17（新增 Unity 代理协议 + 双引擎商业化策略）
