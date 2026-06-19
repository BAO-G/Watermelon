# 合成大西瓜 (Watermelon Game)

> 基于 Unity URP 2D 的 Suika Game 风格休闲小游戏 —— 合成水果，挑战最高分！

[![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black?logo=unity)](https://unity.com/)
[![Platform](https://img.shields.io/badge/Platform-Android%20%7C%20iOS%20%7C%20PC-lightgrey)]()
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)


---

## 项目简介

**合成大西瓜**是一款基于物理引擎的 2D 休闲合成类游戏，灵感来源于热门游戏 Suika Game。玩家通过控制水果下落位置，使相同水果碰撞合并为更高级的水果，最终目标是合成一个完整的大西瓜并获取最高分。

游戏包含完整的游戏循环：开始 → 游戏中 → 游戏结束 → 重新开始，并配有丰富的 UI 动画、特效反馈和得分系统。

---

## 主要功能

### 核心玩法
- **拖拽控制**：点击并拖拽屏幕控制水果水平位置，松开后水果受重力下落
- **物理碰撞合并**：两个相同等级的水果碰撞后自动合成为下一级水果
- **6 级水果**：从 Fruit1 到 Fruit6（西瓜），等级越高得分越多
- **死亡线判定**：水果堆积超过顶部死亡线并停留一定时间即游戏结束
- **边界限制**：水果拖拽范围受左右边界限制，确保在可视范围内

### 游戏系统
- **状态机管理**：`Ready → InProgress → GameOver` 三种游戏状态
- **分数系统**：合并得分，支持最高分本地持久化存储（PlayerPrefs）
- **下一个预览**：实时显示下一个待生成的水果类型
- **一键重启**：游戏结束后可快速重新开始

### 视觉与反馈
- **DOTween 动画**：面板切换淡入淡出、分数弹跳、合并缩放、子元素依次入场
- **飘分特效**：合并位置弹出 "+分数" 文本并向上飘动消失
- **屏幕震动**：合并时触发相机震动增强打击感
- **按钮动画**：悬停放大、按下缩小、透明度渐变
- **音效接口**：预留掉落、合并、游戏结束音效插槽（未赋值音频时静默运行）
- **移动端震动**：平台条件编译，合并和游戏结束时触发

### 编辑器工具
- **一键场景搭建**：菜单 `Watermelon/Setup Scene` 自动配置 Canvas、相机、管理器、UI 面板

---

## 技术栈

| 类别 | 技术 |
|------|------|
| 游戏引擎 | Unity 2022.3+ |
| 渲染管线 | Universal Render Pipeline (URP) 2D |
| 编程语言 | C# |
| 动画库 | [DOTween](http://dotween.demigiant.com/) (免费版) |
| 物理引擎 | Unity 2D Physics (Box2D) |
| 音频 | Unity AudioSource + AudioClip |
| 数据持久化 | PlayerPrefs |

---

## 项目结构

```
Watermelon/
├── Assets/
│   ├── Editor/
│   │   └── WatermelonSceneSetup.cs    # 编辑器一键场景搭建工具
│   ├── Material/
│   │   └── Fruit Physics Material 2D.physicsMaterial2D  # 水果物理材质
│   ├── Plugins/
│   │   └── Demigiant/DOTween/         # DOTween 动画插件
│   ├── Prefab/
│   │   ├── Fruit1.prefab ~ Fruit6.prefab  # 6 个水果预制体
│   │   └── ...
│   ├── Resources/
│   │   └── DOTweenSettings.asset      # DOTween 配置
│   ├── Scenes/
│   │   └── SampleScene.unity          # 主游戏场景
│   ├── Scripts/
│   │   ├── GameManager.cs             # 游戏全局管理（状态、分数、生成队列）
│   │   ├── Fruit.cs                   # 水果个体行为（拖拽、下落、碰撞合并）
│   │   ├── UIManager.cs               # UI 面板切换、分数更新、预览
│   │   ├── UIButtonAnimator.cs        # 按钮悬停/点击交互动画
│   │   ├── AudioManager.cs            # 音效播放管理
│   │   ├── EffectsManager.cs          # 合并特效与屏幕震动
│   │   ├── ScorePopupManager.cs       # 飘分文本管理
│   │   ├── ScorePopup.cs              # 单个飘分 DOTween 动画
│   │   ├── VibrationManager.cs        # 移动端震动反馈
│   │   └── DeathLine.cs               # 死亡线触发检测
│   ├── Settings/
│   │   └── UniversalRP.asset          # URP 渲染管线配置
│   └── Texture/                       # 14 张水果/背景/UI 贴图
├── Docs/
│   ├── ProjectCompletionAndTestingReport.md  # 项目完成与测试报告
│   └── ...
├── GameDevelopmentPlan.md             # 游戏开发计划文档
└── VersionComparison.md               # 版本差异对比分析
```

---

## 安装与配置

### 环境要求

- **Unity** 2022.3 LTS 或更高版本
- **构建平台**：Android / iOS / Windows / macOS / WebGL
- **Unity 模块**：Universal Render Pipeline (URP)

### 安装步骤

1. **克隆仓库**

```bash
git clone https://github.com/your-username/watermelon-game.git
```

2. **使用 Unity Hub 打开项目**

   - 打开 Unity Hub → 点击"添加" → 选择项目文件夹
   - 确保已安装 Unity 2022.3+ 版本（包含 URP 和对应平台构建支持）

3. **等待资源导入**

   Unity 打开项目后会自动导入资源和 DOTween 插件，首次打开可能需要几分钟。

4. **一键搭建场景**（推荐）

   在 Unity 编辑器中，点击菜单栏 `Watermelon → Setup Scene`，自动完成以下配置：

   - 创建/配置 Canvas（1080x1920 竖屏适配）
   - 创建/配置 EventSystem
   - 调整相机正交尺寸
   - 创建 Managers 对象并挂载所有管理器脚本
   - 创建三个 UI 面板（开始/游戏/结束）
   - 配置背景图片

5. **手动配置水果 Prefab 引用**

   在 Hierarchy 中选中 `Managers` 对象，在 Inspector 中将 `Assets/Prefab/` 下的 6 个水果 Prefab 拖入 `GameManager` 组件的 `Fruits` 数组中。

6. **运行游戏**

   点击 Unity 工具栏的 Play 按钮即可开始游戏。

---

## 使用方法

### 基本操作

| 操作 | 说明 |
|------|------|
| 点击"开始游戏"按钮 | 进入游戏状态，生成第一个水果 |
| 拖拽水果 | 按住鼠标左键（或触摸屏）左右移动水果 |
| 松开 | 水果受重力下落 |
| 等待落地 | 水果落地后自动生成下一个 |
| 合成 | 两个相同水果碰撞自动升级 |
| 重新开始 | 游戏结束后点击"重新开始"按钮 |

### 游戏规则

- 每次随机生成前 3 级水果（Fruit1 ~ Fruit3）
- 两个相同类型水果碰撞后合并为下一级水果
- 合成 Fruit6（西瓜）后不再升级
- 任意水果在死亡线上方停留超过 2 秒则游戏结束
- 分数随每次合并增加，打破最高分时会有特殊提示

### 构建发布

1. `File → Build Settings` 选择目标平台
2. 点击 `Player Settings` 配置应用名称、图标、包名等
3. 点击 `Build` 生成安装包

---

## 脚本说明

| 脚本 | 职责 | 关键特性 |
|------|------|---------|
| `GameManager` | 游戏全局管理 | 单例模式；状态机；分数系统；生成队列；Inspector 可调参数 |
| `Fruit` | 水果个体行为 | 状态机 (Ready→Standby→Falling→Landed)；物理稳定检测落地；DOTween 动画 |
| `UIManager` | UI 面板管理 | 面板淡入淡出；子元素交错入场；分数弹跳动画；最高分闪烁 |
| `UIButtonAnimator` | 按钮交互动画 | IPointerEnter/Exit/Down/Up 接口；缩放+透明度动画 |
| `AudioManager` | 音效管理 | 未赋值音频时静默运行；PlayOneShot 播放 |
| `EffectsManager` | 特效管理 | 摄像机震动 (DOShakePosition)；粒子特效接口 |
| `ScorePopupManager` | 飘分管理 | 世界坐标转屏幕坐标；动态生成飘分文本 |
| `ScorePopup` | 飘分动画 | DOTween 淡出+上浮+缩放循环 |
| `VibrationManager` | 震动反馈 | 平台条件编译；Android/iOS Handheld.Vibrate |
| `DeathLine` | 死亡线检测 | TriggerStay 累计停留时间；Dictionary 记录每个水果 |

---

## 贡献指南

欢迎贡献代码、报告问题或提出新功能建议！

### 贡献流程

1. **Fork** 本仓库
2. 创建特性分支：`git checkout -b feature/amazing-feature`
3. 提交更改：`git commit -m '添加某某功能'`
4. 推送到分支：`git push origin feature/amazing-feature`
5. 提交 **Pull Request**

### 代码规范

- 使用 C# 标准命名约定（PascalCase 用于公共成员，camelCase 用于私有字段）
- 为公共方法和复杂逻辑添加 XML 注释
- 避免在 `Update()` 中使用 `Debug.Log`
- 使用 `[Header]` 和 `[Tooltip]` 属性组织 Inspector 字段
- 提交前确保 `dotnet build` 无错误无警告

---

## 许可证

本项目基于 **MIT 许可证** 开源。详情请参阅 [LICENSE](LICENSE) 文件。

DOTween 插件遵循其[自有许可](http://dotween.demigiant.com/license.php)。

---

<p align="center">
  <sub>Made with Unity and DOTween</sub>
</p>