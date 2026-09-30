# 当前任务

任务编号：

P-002.1

任务名称：

RedMemory 人工交互与 UI 层级 Hotfix

场景：

RedMemory

状态：

Completed

---

# 本轮目标

在不改变剧情、小游戏规则或既有 gameplay 功能的前提下，修复 RedMemory 的 StoryRoot UI sibling 顺序、透明全屏 Image Raycast 拦截与自动 Verifier 普通 Play 接管问题，确保玩家可用真实鼠标完成剧情、选项、任务卡并进入 GameplayRoot。

---

# 强制复用与规则

- 复用 `PortraitDisplayController`、`CursorEffectController`、`UIButtonAnimator` 与现有 StudyBook / Handbook 系统，不创建重复系统。
- 小禾说话只显示小禾；志愿者说话只显示志愿者；旁白、系统与标题隐藏两人。
- 章节标题可使用标题字体，其余文字统一预留“汉仪傲娇体简”。
- 正式美术均建立 Inspector 可替换插槽，本轮不自行绘图、不联网下载素材、不编造真实历史资料。
- 剧情与 Gameplay 在同一 RedMemory 场景内切换。
- 完成后通过真实鼠标执行剧情 → 选项 → Mission → Gameplay 路径，并用真实键盘验证 D / Space 输入；自动 verifier 不得代替人工点击验收或干扰普通 Play。

---

# 交付

- 更新 `AI_CODEX_REPORT.md`，记录场景、脚本、Prefab、完整流程、Play Mode / Console 结果、待提供美术与音频、已知问题。
- 提交并推送：`Fix P-002 RedMemory manual UI interaction`
- 不继续 P-003。
